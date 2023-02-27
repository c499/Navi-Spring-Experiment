using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation;
using UnityEngine;
using UnityEditor;

namespace Assets.Created_Assets.Diego.Script.TaskManager
{
    class TaskManager: InteractionListener
    {
        UserTrialSequenceData userSequence;
        Task currentTask=null;
        int userId;
        //Store global data:
        List<string> travelContent, maneuvreContent, questionnaireContent;

        TaskManager() {
            //0. Setup storage to gather results from travel trials.
            travelContent = new List<string>();
            string headerTravel = "UserID, Technique, M_FACTOR, PATH_LENGTH, PATH_ID, FLAG1, FLAG2, FLAG3, FLAG4, FLAG5, RANDOM_F_OFFSET, _T_TCT,  _AVG_DEVIATION, _REAL_DIST_TRAVELLED, _VIRT_DIST_TRAVELLED";
            //HEADER COPIED TO TravelTask2::writeGlobalParametersToCollection -> If this format changes, the changes should propagate to that method too
            travelContent.Add(headerTravel);

            //1. Setup storage to gather results from maneuvre trials.
            maneuvreContent = new List<string>();
            string headerManeuvre = "UserID, Technique, M_FACTOR, TRAVEL_PATH_LENGHT(2/4), _M_TCT,  _M_ORIENT_ERROR, _M_POS_ERROR, NUM_EXIT_ERRORS, _M_TCT_High_T_T,  _M_ORIENT_ERROR_High_T_T, _M_POS_ERROR_High_T_T,NUM_EXIT_ERRORS_High_T_T,  _M_ORIENT_ERROR_IN_High_T_T, _M_POS_ERROR_IN_High_T_T, _M_TCT_High_T_L,  _M_ORIENT_ERROR_High_T_L, _M_POS_ERROR_High_T_L,  NUM_EXIT_ERRORS_High_T_L,_M_ORIENT_ERROR_IN_High_T_L, _M_POS_ERROR_IN_High_T_L , _M_TCT_High_L_T, _M_ORIENT_ERROR_High_L_T, _M_POS_ERROR_High_L_T, NUM_EXIT_ERRORS__High_L_T, _M_ORIENT_ERROR_IN_High_L_T, _M_POS_ERROR_IN_High_L_T, _M_TCT_High_L_L, _M_ORIENT_ERROR_High_L_L, _M_POS_ERROR_High_L_L, NUM_EXIT_ERRORS_High_L_L, _M_ORIENT_ERROR_IN_High_L_L, _M_POS_ERROR_IN_High_L_L     , _M_TCT_Med_T_T, _M_ORIENT_ERROR_Med_T_T, _M_POS_ERROR_Med_T_T, NUM_EXIT_ERRORS_Med_T_T, _M_ORIENT_ERROR_IN_Med_T_T, _M_POS_ERROR_IN_Med_T_T, _M_TCT_Med_T_L, _M_ORIENT_ERROR_Med_T_L, _M_POS_ERROR_Med_T_L, NUM_EXIT_ERRORS_Med_T_L, _M_ORIENT_ERROR_IN_Med_T_L, _M_POS_ERROR_IN_Med_T_L, _M_TCT_Med_L_T, _M_ORIENT_ERROR_Med_L_T, _M_POS_ERROR_Med_L_T, NUM_EXIT_ERRORS_Med_L_T,_M_ORIENT_ERROR_IN_Med_L_T, _M_POS_ERROR_IN_Med_L_T, _M_TCT_Med_L_L, _M_ORIENT_ERROR_Med_L_L, _M_POS_ERROR_Med_L_L, NUM_EXIT_ERRORS_Med_L_T,_M_ORIENT_ERROR_IN_Med_L_L, _M_POS_ERROR_IN_Med_L_L, _M_TCT_Low_T_T, _M_ORIENT_ERROR_Low_T_T, _M_POS_ERROR_Low_T_T, NUM_EXIT_ERRORS_LOW_T_T,_M_ORIENT_ERROR_IN_Low_T_T, _M_POS_ERROR_IN_Low_T_T, _M_TCT_Low_T_L, _M_ORIENT_ERROR_Low_T_L, _M_POS_ERROR_Low_T_L, NUM_EXIT_ERRORS_Low_T_L,_M_ORIENT_ERROR_IN_Low_T_L, _M_POS_ERROR_IN_Low_T_L, _M_TCT_Low_L_T, _M_ORIENT_ERROR_Low_L_T, _M_POS_ERROR_Low_L_T, NUM_EXIT_ERRORS_Low_L_T, _M_ORIENT_ERROR_IN_Low_L_T, _M_POS_ERROR_IN_Low_L_T, _M_TCT_Low_L_L, _M_ORIENT_ERROR_Low_L_L, _M_POS_ERROR_Low_L_L, NUM_EXIT_ERRORS_Low_L_L, _M_ORIENT_ERROR_IN_Low_L_L, _M_POS_ERROR_IN_Low_L_L"; 
             //HEADER COPIED TO ManeuverTask::writeGlobalParametersToCollection -> If this format changes, the changes should propagate to that method too
            maneuvreContent.Add(headerManeuvre);

            //2. Setup storage to gather results from questionnaire.
            questionnaireContent = new List<string>();
            string headerQuestionnaire = "UserID, Technique, M_FACTOR, TRAVEL_PATH_LENGHT(2/4), EASY_TRAVEL, COMFORT_TRAVEL, EASY_MANEUVRE, COMFORT_MANEUVRE";
            //HEADER COPIED TO ManeuverTask::writeGlobalParametersToCollection -> If this format changes, the changes should propagate to that method too
            questionnaireContent.Add(headerQuestionnaire);


            //2. Prepare the user sequences (pre-load).
            UserTrialSequenceData.getTrialSequenceData(0); //We call once so that sequences are generated...
        }


        static TaskManager _instance = null;
        public static TaskManager instance() {
            if (_instance==null)
                _instance = new TaskManager();
            return _instance;
        }

        public void setupExperiment(int userId) {
            //0. Let's retrieve our set of tasks
            this.userId = userId;
            userSequence= UserTrialSequenceData.getTrialSequenceData(userId);
            string message = "Sequence " + userSequence;
            Debug.Log(message);
           
            //DEBUG: Let's print all sequences to a file, to check experiment design...
            string fileSequences= Application.dataPath + "/../ExperimentResults/experimentTasks.csv";
            List<string> sequencesPerUser = new List<string>();
            for (int u = 0; u < 12; u++)
                sequencesPerUser.Add(UserTrialSequenceData.getTrialSequenceData(u).ToString());
            System.IO.File.WriteAllLines(fileSequences, sequencesPerUser.ToArray());

            //1. Setup first trial:

            UserTrialSequenceData auxUserSequence = UserTrialSequenceData.getTrialSequenceData(11-(userId%12)); ;
            for (int i = 0; i < 12; i++) auxUserSequence.getNextTrialData();//Ignore 12 sequences in between... we want the last natural trials
            TaskTrialData training1=auxUserSequence.getNextTrialData();//The first natural travel
            if (training1.travellingTrialData.length != 4)
            {   //I want a four step travel task here...        
                training1 = auxUserSequence.getNextTrialData();//The second natural travel
            }
            //Let's have training finish at flag 3: we set the random offset to do this...
            int lastFlag = training1.travellingTrialData.path.getFlagFromOrder(training1.travellingTrialData.length);
            training1.travellingTrialData.randomFlagOffset = (3 - lastFlag>=0? 3 - lastFlag : 9 - lastFlag);//Just make sure it is still positive...
            currentTask = TaskTrialFactory.buildIntroductoryTaask(training1);
            currentTask.allocateTask();
            NavigationControl.instance().addListener(this); //To receive head updates and button presses...
        }

        void _setupNextTrial() {
            if (userSequence.sequenceFinished())
                return;
            //Retruieve data for the next trial
            TaskTrialData trialData=userSequence.getNextTrialData();
            //Build the tasks (TaskTrialFactory provides an implementation for the task in Unity...)
            currentTask = TaskTrialFactory.buildTaskTrial(trialData);
            currentTask.allocateTask();

        }

        public void onTriggerPressed(float time) {
            currentTask.triggerIsPressed(true);
        }
        static bool onlyOne = true;
        public void onHeadUpdate(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR) {
            //EnvironmentManager.instance().centralText(currentTask.taskData.ToString() + "\n K="+ cur_M_Factor);
            //Let's check the status of the current trial.
            if (currentTask.finished()) {
                //If this trial ended, call end_of_life methods
                currentTask.writeGlobalParametersToCollection(travelContent,maneuvreContent, questionnaireContent);
                currentTask.deallocateTask();
                //And load following trial.
                if (!userSequence.sequenceFinished() && onlyOne)
                {
                    _setupNextTrial();
                    //onlyOne = false;
                }
                else
                {   //If no more trials, FINISH APPLICATION
                    string fileNameTravel = Application.dataPath + "/../ExperimentResults/" +
                                            this.userId + "_Travel_Global.csv";
                    System.IO.File.WriteAllLines(fileNameTravel, travelContent.ToArray());
                    string fileNameManeuvre = Application.dataPath + "/../ExperimentResults/" +
                            this.userId + "_Maneuvre_Global.csv";
                    System.IO.File.WriteAllLines(fileNameManeuvre, maneuvreContent.ToArray());
                    string fileNameQuestionnaire = Application.dataPath + "/../ExperimentResults/" +
                                            this.userId + "_Questionnaire_Global.csv";
                    System.IO.File.WriteAllLines(fileNameQuestionnaire, questionnaireContent.ToArray());
                    Application.Quit();
                    EditorApplication.isPlaying = false;
                }
            }else
                currentTask.update(headToTracking, delta_headToTracking, headToVR, delta_headToVR, time, cur_M_Factor,handInVR);
        }
    }
}

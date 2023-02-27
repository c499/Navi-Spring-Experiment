using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation.HelperTasks
{
    class QuestionnaireTask: Task {
        int curQuestion;
        bool firstTime;
        int curAnswer;
        int[] answers;
        public QuestionnaireTask( TaskTrialData taskData) : base(taskData)
        {
            curQuestion = 0;
            firstTime = true;
            curAnswer = -1;
            answers = new int[4];
            for (int a = 0; a < 4; a++)
                answers[a] = -1;
        }

        public override void allocateTask()
        {
            //Show the questionnaire.
            EnvironmentManager.instance().highlightAllFlags(false);
            for (int f = 0; f < 6; f++)
                EnvironmentManager.instance().showFlag(f, false);
            EnvironmentManager.instance().showQuestion(curQuestion);
            EnvironmentManager.instance().showQuestionnaire(true);

        }
        public override void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR)
        {
            if (firstTime) {
                //Position the questionnaire in front of the user. 
                EnvironmentManager.instance().showQuestionnaireToUser();
                firstTime = false;
            }

            //Highlight the right curent answer.
            int answer=EnvironmentManager.instance().checkQuestionnaireOption(handInVR);
            //If option changed sound some feedback
            if(answer!=curAnswer)
                EnvironmentManager.instance().playEffect(SoundEffects.NEGATIVE_FEEDBACK);
            //and register it
            curAnswer = answer;
        }

        public override void triggerIsPressed(bool isPressed)
        {
            if (!isPressed)
                return;
            if (curAnswer == -1)
                EnvironmentManager.instance().playEffect(SoundEffects.NEGATIVE_FEEDBACK);
            else {
                answers[curQuestion] = curAnswer;
                curQuestion++;
                if (curQuestion == 4)
                {
                    EnvironmentManager.instance().playEffect(SoundEffects.CROWD_FEEDBACK);
                    _finished = true;
                }
                else
                {
                    EnvironmentManager.instance().playEffect(SoundEffects.POSITIVE_FEEDBACK);
                    EnvironmentManager.instance().showQuestion(curQuestion);
                }
            }
        }

        public override void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents)
        {
            //string headerQuestionnaire = "UserID, Technique, M_FACTOR, TRAVEL_PATH_LENGHT(2/4), EASY_TRAVEL, COMFORT_TRAVEL, EASY_MANEUVRE, COMFORT_MANEUVRE";
            string entry = taskData.travellingTrialData.UserID + ","
                           + taskData.travellingTrialData.technique + ","
                           + taskData.travellingTrialData.M_factor + ","
                           + taskData.travellingTrialData.length + ","
                           + answers[0] + ","
                           + answers[1] + ","
                           + answers[2] + ","
                           + answers[3] + ",";

            questionnaireContents.Add(entry);
            return;
        }
        public override void deallocateTask()
        {
            // Hide all questions. 
            EnvironmentManager.instance().showQuestionnaire(false) ;
        }
    }
}

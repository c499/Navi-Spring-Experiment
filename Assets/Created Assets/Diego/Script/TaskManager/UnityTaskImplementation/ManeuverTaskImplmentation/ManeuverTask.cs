using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Created_Assets.Diego.Script.TaskManager.ManeuvreData;

namespace Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation.TravelTaskImplementation
{
    class ManeuverTask : Task
    {
        int curFlag;                //Flag where the maneuvring task is taking place.
        int curParafrustum;         //Number of the trial [1..4]    
        int curParafrustumPosition; //Position of the parafrustum [0..5] in the hexagon.
        bool firstTime = true;      //Flag to detect execution starts (basically to safe the starting time).
        float curStartTime;         //Time since this trial started.
        float absoluteStartTime;    //Time since the whole task (four parafrustum trials) started.
        float curStartInsideTime;   //the user has been consistently inside the Parafrustum since this time (if >1.5s, it is a succesfull trial)
        bool wasInside;
        public ManeuverTask(TaskTrialData taskData) : base(taskData)
        {
            wasInside = false;
            curParafrustum=0;
            string entry = "UserID, Technique, M_FACTOR, TRAVEL_PATH_LENGHT(2/4), PARAF_POSITION(0-5), HEAD, TAIL, PARAF_X, PARAF_Y, PARAF_Z, _TIME,  _R_HEAD_X, _R_HEAD_Y, _R_HEAD_Z, _V_HEAD_X, _V_HEAD_Y, _V_HEAD_Z, _CUR_M_FACTOR, _CUR_ANGLE_ERROR, _CUR_POS_ERROR";
            entries.Add(entry);
        }

        public override void allocateTask()
        {
            EnvironmentManager.instance().playEffect(SoundEffects.TICK_TACK_FEEDBACK);
            EnvironmentManager.instance().showParafrustum(true);
            //1. Place parafrustum in the correct flag
            curFlag = taskData.travellingTrialData.path.getFlagFromOrder(taskData.travellingTrialData.length);
            if (curFlag < 0 || curFlag > 5)//
                return;
            //Apply randomization
            curFlag = (curFlag + taskData.travellingTrialData.randomFlagOffset) % 6;
            EnvironmentManager rs = EnvironmentManager.instance();
            rs.moveParaFrustumToFlag(curFlag);
            //2. Setup next parafrustum location
            _setupNextParafrustumTarget();
        }


        public override void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR)
        {
            //EnvironmentManager.instance().centralText(time.ToString("F3"));
            if (firstTime) {
                absoluteStartTime = time;
                curStartTime = time;
                firstTime = false;
            }
            //A. Check parafrustum 
            float posError = 0, angError = 0;
            bool inside=EnvironmentManager.instance().checkParafrustum(ref posError, ref angError);

            Vector3 parafrustum3DCoords = EnvironmentManager.instance().getParafrustumPosition();
            //B.  Update dependent variables (M_TCT is done at the end...): 
            taskData.maneuvringTrialData.updateDependentVariables(curParafrustum-1,  angError,posError);
            if (inside) taskData.maneuvringTrialData.updateDependentVariablesInsideParafrustum(curParafrustum - 1, angError, posError);
            //C. Update File         
            string entry = "" + taskData.maneuvringTrialData.UserID + "," +
                                taskData.maneuvringTrialData.technique + "," +
                                taskData.maneuvringTrialData.M_factor + "," +
                                taskData.travellingTrialData.length + "," +
                                taskData.maneuvringTrialData.trials[curParafrustum - 1] +","+
                                parafrustum3DCoords.x.ToString("F3") + "," +
                                parafrustum3DCoords.y.ToString("F3") + "," +
                                parafrustum3DCoords.z.ToString("F3") + "," +
                                time.ToString("F3") + "," +
                                headToTracking.x.ToString("F3") + "," +
                                headToTracking.y.ToString("F3") + "," +
                                headToTracking.z.ToString("F3") + "," +
                                headToVR.x.ToString("F3") + "," +
                                headToVR.y.ToString("F3") + "," +
                                headToVR.z.ToString("F3") + "," +
                                cur_M_Factor.ToString("F3") + "," +
                                posError.ToString("F3") + "," +
                                angError.ToString("F3") + ","; ;
            entries.Add(entry);
            
            //C. Check task finished: Either suxxesfully or 30 s timeout
            if( (inside && (time - this.curStartInsideTime > 1.5))
                || (time - this.curStartTime > 30))
            {
                EnvironmentManager.instance().playEffect(SoundEffects.POSITIVE_FEEDBACK);
                this.taskData.maneuvringTrialData.globalVaraibles.M_TCT = time - absoluteStartTime;//This would be our current total elapsed time. It will be rewriten later, if still more trials remain (avoiding an "if(finished())" statement here, as it is unnecessary...
                taskData.maneuvringTrialData.updateTrialTCT(curParafrustum - 1, time - curStartTime);
                this.curStartInsideTime = time;
                curStartTime = time;
                _finished = !_setupNextParafrustumTarget();                
            }
            else if(!inside)
            {
                this.curStartInsideTime = time;
                if (wasInside)
                    this.taskData.maneuvringTrialData.increaseParafrustumErrors(curParafrustum - 1);
            }

            wasInside = inside;

        }
        public override void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents)
        {
            //A. Let's order the parafrustum results according to the column order in the file
            int[] trialPerColumn = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            for (int trial = 0; trial < 3; trial++)//6
            {
                trialPerColumn[taskData.maneuvringTrialData.getParafrustumColumnOrderForTrial(trial)] = trial;
            }
            //string headerManeuvre = "UserID, Technique, M_FACTOR, TRAVEL_PATH_LENGHT(2/4), _M_TCT,  _M_ORIENT_ERROR, _M_POS_ERROR, NUM_EXIT_ERRORS, _M_TCT_High_T_T,  _M_ORIENT_ERROR_High_T_T, _M_POS_ERROR_High_T_T,NUM_EXIT_ERRORS_High_T_T,  _M_ORIENT_ERROR_IN_High_T_T, _M_POS_ERROR_IN_High_T_T, _M_TCT_High_T_L,  _M_ORIENT_ERROR_High_T_L, _M_POS_ERROR_High_T_L,  NUM_EXIT_ERRORS_High_T_L,_M_ORIENT_ERROR_IN_High_T_L, _M_POS_ERROR_IN_High_T_L , _M_TCT_High_L_T, _M_ORIENT_ERROR_High_L_T, _M_POS_ERROR_High_L_T, NUM_EXIT_ERRORS__High_L_T, _M_ORIENT_ERROR_IN_High_L_T, _M_POS_ERROR_IN_High_L_T, _M_TCT_High_L_L, _M_ORIENT_ERROR_High_L_L, _M_POS_ERROR_High_L_L, NUM_EXIT_ERRORS_High_L_L, _M_ORIENT_ERROR_IN_High_L_L, _M_POS_ERROR_IN_High_L_L     , _M_TCT_Med_T_T, _M_ORIENT_ERROR_Med_T_T, _M_POS_ERROR_Med_T_T, NUM_EXIT_ERRORS_Med_T_T, _M_ORIENT_ERROR_IN_Med_T_T, _M_POS_ERROR_IN_Med_T_T, _M_TCT_Med_T_L, _M_ORIENT_ERROR_Med_T_L, _M_POS_ERROR_Med_T_L, NUM_EXIT_ERRORS_Med_T_L, _M_ORIENT_ERROR_IN_Med_T_L, _M_POS_ERROR_IN_Med_T_L, _M_TCT_Med_L_T, _M_ORIENT_ERROR_Med_L_T, _M_POS_ERROR_Med_L_T, NUM_EXIT_ERRORS_Med_L_T,_M_ORIENT_ERROR_IN_Med_L_T, _M_POS_ERROR_IN_Med_L_T, _M_TCT_Med_L_L, _M_ORIENT_ERROR_Med_L_L, _M_POS_ERROR_Med_L_L, NUM_EXIT_ERRORS_Med_L_T,_M_ORIENT_ERROR_IN_Med_L_L, _M_POS_ERROR_IN_Med_L_L, _M_TCT_Low_T_T, _M_ORIENT_ERROR_Low_T_T, _M_POS_ERROR_Low_T_T, NUM_EXIT_ERRORS_LOW_T_T,_M_ORIENT_ERROR_IN_Low_T_T, _M_POS_ERROR_IN_Low_T_T, _M_TCT_Low_T_L, _M_ORIENT_ERROR_Low_T_L, _M_POS_ERROR_Low_T_L, NUM_EXIT_ERRORS_Low_T_L,_M_ORIENT_ERROR_IN_Low_T_L, _M_POS_ERROR_IN_Low_T_L, _M_TCT_Low_L_T, _M_ORIENT_ERROR_Low_L_T, _M_POS_ERROR_Low_L_T, NUM_EXIT_ERRORS_Low_L_T, _M_ORIENT_ERROR_IN_Low_L_T, _M_POS_ERROR_IN_Low_L_T, _M_TCT_Low_L_L, _M_ORIENT_ERROR_Low_L_L, _M_POS_ERROR_Low_L_L, NUM_EXIT_ERRORS_Low_L_L, _M_ORIENT_ERROR_IN_Low_L_L, _M_POS_ERROR_IN_Low_L_L";
            //B. Lets add the global results first
            string entry = "";
            if (taskData.maneuvringTrialData.globalVaraibles.numSamples == 0 || taskData.maneuvringTrialData.globalVaraibles.numSamplesIn == 0)
                entry += " ";
            entry += taskData.travellingTrialData.UserID + ","
                           + taskData.maneuvringTrialData.technique + ","
                           + taskData.maneuvringTrialData.M_factor + ","
                           + taskData.travellingTrialData.length + ","
                           + taskData.maneuvringTrialData.globalVaraibles.M_TCT.ToString("F3") + ","
                           + (taskData.maneuvringTrialData.globalVaraibles.M_TOTAL_ORIENT_ERROR / taskData.maneuvringTrialData.globalVaraibles.numSamples).ToString("F3") + ","
                           + (taskData.maneuvringTrialData.globalVaraibles.M_TOTAL_POS_ERROR / taskData.maneuvringTrialData.globalVaraibles.numSamples).ToString("F3") + ","
                           + taskData.maneuvringTrialData.globalVaraibles.numParafrfustumErrors + ","
                           ;
            //C. Let's add the results per trial
            for (int c = 0; c < 12; c++)//12
                if (trialPerColumn[c] != -1) {
                    ManeuvreTrialData.ParafrustumDependentVariables td = taskData.maneuvringTrialData.perTrialVariables[trialPerColumn[c]];
                    if (td.numSamples == 0 || td.numSamplesIn==0)
                        entry = "  ";
                    entry += td.M_TCT.ToString("F3") + ","
                           + (td.M_TOTAL_ORIENT_ERROR / td.numSamples).ToString("F3") + ","
                           + (td.M_TOTAL_POS_ERROR / td.numSamples).ToString("F3") + ","
                           + (td.numParafrfustumErrors)+","
                           + (td.M_TOTAL_ORIENT_ERROR_IN / td.numSamplesIn).ToString("F3") + ","
                           + (td.M_TOTAL_POS_ERROR_IN / td.numSamplesIn).ToString("F3") + ",";
                }else
                {
                    entry += "-1, -1, -1, -1, -1, -1, ";//This sample was not in this task (will be in the complementary one). 
                }
            
            maneuvreContents.Add(entry);
            return;
        }
        public override void deallocateTask()
        {
            EnvironmentManager.instance().stopEffect(SoundEffects.TICK_TACK_FEEDBACK);
            EnvironmentManager.instance().playEffect(SoundEffects.CROWD_FEEDBACK);
            //EnvironmentManager.instance().centralText("END MANEUVER"); ;
            EnvironmentManager.instance().showParafrustum(false);
            //WRITE FILE FOR THIS TRIAL:
            string fileName = UnityEngine.Application.dataPath + "/../ExperimentResults/Maneuver_User_" +
                             taskData.maneuvringTrialData.UserID + "_" +
                               taskData.maneuvringTrialData.technique + "_" +
                               taskData.maneuvringTrialData.M_factor+ "_L" +
                               taskData.travellingTrialData.length + ".csv";
            System.IO.File.WriteAllLines(fileName, entries.ToArray());
        }

        protected bool _setupNextParafrustumTarget()
        {
            /* LITTLE ADJUSTMENT: The first parafrustum (L_L) is not a part of the task.
             * We just want the user to start from one of our 6 positions, so that our 2-1-2-1 path stays valid.
             * Thus, the absoluteStartTime should not be from the beginning of the task, but since the user
             * arrives to the first parafrustum.            
             // 
            if (curParafrustum == 1)
                absoluteStartTime = curStartTime;*/

            //Lets check if this was the last parafrustum to test.
            if (curParafrustum >= taskData.maneuvringTrialData.trials.Length)
                return false;
            //2. Get Parafrustum position (index)
            curParafrustumPosition = taskData.maneuvringTrialData.trials[curParafrustum++].paraFrustumIndex;
            EnvironmentManager.instance().setParafrustum(taskData.maneuvringTrialData.trials[curParafrustum-1].head, taskData.maneuvringTrialData.trials[curParafrustum-1].tail, curParafrustumPosition);
            return true;
        }
    }
}

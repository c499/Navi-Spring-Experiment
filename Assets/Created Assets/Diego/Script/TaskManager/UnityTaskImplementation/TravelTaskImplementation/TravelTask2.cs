using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Created_Assets.Diego.Script;
using UnityEngine;

namespace Assets.Created_Assets.Diego.Script.TaskManager
{
    /**
     * This class manages the execution of a task in our experimental environment. 
     
         */
    class TravelTask2 : Task
    {
        const int WAIT_START = 0, STARTED = 1;
        int state = WAIT_START;
        //Variables for task, while waiting for trigger press.
        bool insideCentralArea = false;
        //Variables for task, once started
        int curFlag;
        float curStartTime;         //Time for the current "hop" in the path
        float absoluteStartTime;    //Time since we started the travel task.
        Vector3 curTargetFlagPosition_OnFloor;//Y=0
        Vector3 curOriginalPosition_OnFloor;

        public TravelTask2(TaskTrialData taskData) : base( taskData) {
            //Write a header for the file: 
            string entry = "UserID, Technique, M_FACTOR, M_FACTOR_applied, PATH_LENGTH, PATH_ID, FLAG1, FLAG2, FLAG3, FLAG4, FLAG5, RANDOM_F_OFFSET, _CUR_FLAG ,_TIME,  _R_HEAD_X, _R_HEAD_Y, _R_HEAD_Z, _V_HEAD_X, _V_HEAD_Y, _V_HEAD_Z, _CUR_M_FACTOR, _CUR_DEVIATION"; 
            entries.Add(entry);
            curFlag = -1; //Central area (for Pathfinder line).
        }

        public override void allocateTask() {
            NavigationControl.instance().setTechnique(taskData.travellingTrialData.technique);
            NavigationControl.instance().setMFactor(taskData.travellingTrialData.M_factor);
            EnvironmentManager.instance().setupTravelScene(taskData.travellingTrialData.M_factor);//The scene chosen depends on the scaling factor

            EnvironmentManager.instance().highlightCentralArea(false);
            EnvironmentManager.instance().showCentralArea(false);
            EnvironmentManager.instance().playEffect(SoundEffects.TICK_TACK_FEEDBACK);
            

        }
        public override void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR) {
            switch (state) {
                case WAIT_START:
                    Vector3 headPosInVR_OnFloor = headToVR; headPosInVR_OnFloor.y = 0;
                    curOriginalPosition_OnFloor = headPosInVR_OnFloor; //Keep it, we need to know the starting position if travel task starts.
                    absoluteStartTime = time;
                    curStartTime = time;
                    _setupNextFlagTarget();
                    state = STARTED;
                    break;
                case STARTED:
                    _updateTravel( headToTracking, delta_headToTracking, headToVR,  delta_headToVR,  time,  cur_M_Factor);
                    break;
            }  
        }

        protected void _updateTravel(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor)
        {
            Vector3 headPosInVR_OnFloor = new Vector3(headToVR.x, headToVR.y, headToVR.z); headPosInVR_OnFloor.y = 0;
            float travelDistance = (curTargetFlagPosition_OnFloor - headPosInVR_OnFloor).magnitude;
            if (taskData.travellingTrialData.curStep != 1) { //First flag not part of the task (see explanation in _setupNextFlagTarget)
                //A. Update dependent variables: 
                taskData.travellingTrialData.realDistanceTravelled += delta_headToTracking.magnitude;
                taskData.travellingTrialData.virtualDistanceTravelled += delta_headToVR.magnitude;
            
                float curDeviation = _computeDeviation(curTargetFlagPosition_OnFloor - curOriginalPosition_OnFloor, headPosInVR_OnFloor - curOriginalPosition_OnFloor);
                taskData.travellingTrialData.totalDeviation += curDeviation;
                taskData.travellingTrialData.numSamples++;
                //B. Update File
                //string header = "UserID, Technique, M_FACTOR, M_FACTOR_applied, PATH_LENGTH, PATH_ID, FLAG1, FLAG2, FLAG3, FLAG4, FLAG5, RANDOM_F_OFFSET, _CUR_FLAG ,_TIME,  _R_HEAD_X, _R_HEAD_Y, _R_HEAD_Z, _V_HEAD_X, _V_HEAD_Y, _V_HEAD_Z, _CUR_M_FACTOR, _CUR_DEVIATION"; 
                string entry = "" + taskData.travellingTrialData.UserID + "," +
                                   taskData.travellingTrialData.technique + "," +
                                   taskData.travellingTrialData.M_factor + "," +
                                   taskData.travellingTrialData.M_factor_equivalent+","+
                                   taskData.travellingTrialData.length + "," +
                                   taskData.travellingTrialData.path + "," +
                                   taskData.travellingTrialData.randomFlagOffset+","+
                                   curFlag + ","+
                                   (time- curStartTime).ToString("F3") + "," + 
                                   headToTracking.x.ToString("F3") + "," + 
                                   headToTracking.y.ToString("F3") + "," + 
                                   headToTracking.z.ToString("F3") + "," +
                                   headToVR.x.ToString("F3") + "," + 
                                   headToVR.y.ToString("F3") + "," + 
                                   headToVR.z.ToString("F3") + "," +
                                   cur_M_Factor.ToString("F3") + "," +
                                   curDeviation.ToString("F3");
                entries.Add(entry);
            }
            //B. Check if task finished and go to next step. 

            /*EnvironmentManager.instance().centralText("Travel(GO): target=" + curFlag + "D=" + travelDistance + "K=" + cur_M_Factor
                                                   + "\n target:" + curTargetFlagPosition_OnFloor.ToString()
                                                   + "\n headPos:" + headPosInVR_OnFloor);*/
            if (travelDistance < NavigationControl.thresholdTravelDistance())
            {
                EnvironmentManager.instance().playEffect(SoundEffects.POSITIVE_FEEDBACK);
                curOriginalPosition_OnFloor = headPosInVR_OnFloor;
                curStartTime = time;
                _finished = !_setupNextFlagTarget();//Try and setup the next flag, or notify task finished               
                if (taskData.travellingTrialData.curStep != 1)
                { //First flag not part of the task (see explanation in _setupNextFlagTarget)
                    taskData.travellingTrialData.T_TCT = time - absoluteStartTime ;//This would be our current total elapsed time. It will be rewriten later, if still more trials remain (avoiding an "if(finished())" statement here, as it is unnecessary...
                }
            }
        
        }

        public override void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents) {
            //string headerTravel = "UserID, Technique, M_FACTOR, PATH_LENGTH, PATH_ID, FLAG1, FLAG2, FLAG3, FLAG4, FLAG5, RANDOM_F_OFFSET, _T_TCT,  _AVG_DEVIATION, _REAL_DIST_TRAVELLED, _VIRT_DIST_TRAVELLED";
            string entry =  taskData.travellingTrialData.UserID + ","
                            + taskData.travellingTrialData.technique + ","
                            + taskData.travellingTrialData.M_factor + ","
                            + taskData.travellingTrialData.length + ","
                            + taskData.travellingTrialData.path + ","
                            + taskData.travellingTrialData.randomFlagOffset+","
                            + taskData.travellingTrialData.T_TCT.ToString("F3") + ","
                            + (taskData.travellingTrialData.totalDeviation / taskData.travellingTrialData.numSamples).ToString("F3") + ","
                            + taskData.travellingTrialData.realDistanceTravelled.ToString("F3") + ","
                            + taskData.travellingTrialData.virtualDistanceTravelled.ToString("F3");
            travelContents.Add(entry);
        }

        public override void deallocateTask() {
            EnvironmentManager.instance().highlightAllFlags(false);
            EnvironmentManager.instance().stopEffect(SoundEffects.TICK_TACK_FEEDBACK);
            EnvironmentManager.instance().hidePathfinderLine();
            //WRITE FILE FOR THIS TASK: This is the "trace file" with the intermendiate head positions, etc...
            string fileName = Application.dataPath + "/../ExperimentResults/Travel_User_" +
                               taskData.travellingTrialData.UserID + "_" +
                               taskData.travellingTrialData.technique + "_" +
                               taskData.travellingTrialData.M_factor + "_" +
                               "L"+taskData.travellingTrialData.length+".csv";
            System.IO.File.WriteAllLines(fileName, entries.ToArray());
        }

        protected float _computeDeviation(Vector3 a, Vector3 b) {//Point to line distance.
            float distance= (Vector3.Cross(a, b).magnitude / a.magnitude); //Returns closest distance between two vectors (shared origin).
            //Distance can be positive or negative. Lets do absolute value
            return (distance > 0 ? distance : -distance);
        }

        protected bool _setupNextFlagTarget() {
            /* LITTLE ADJUSTMENT: The first flag in the path is not a part of the task.
             * We just want the user to start from one of our 6 flags, so that our paths stays valid (2-1-2-1, 2-1,etc).
             * Thus, the absoluteStartTime should not be from the beginning of the task, but since the user
             * arrives to the first flag.            
             */
            if (taskData.travellingTrialData.curStep == 1)
                absoluteStartTime = curStartTime;

            int lastFlag = curFlag;
            //Get target flag
            curFlag= taskData.travellingTrialData.path.getFlagFromOrder(taskData.travellingTrialData.curStep++);
            if (curFlag < 0 || curFlag > 5)
                return false;
            //Apply randomization
            curFlag=(curFlag+ taskData.travellingTrialData.randomFlagOffset)% 6;

            //Valid Flag-> Setup scene
            try
            {
                curTargetFlagPosition_OnFloor = EnvironmentManager.instance().getPositionOfFlagNumber(curFlag);
            }
            catch (IndexOutOfRangeException e) {
                Debug.Log(e+ "Index: "+curFlag);
            }

            curTargetFlagPosition_OnFloor.y = 0;
            //0. Set all flags to gray
            EnvironmentManager.instance().highlightAllFlags(false);
            //1. Highlight flag in green
            EnvironmentManager.instance().highlightFlag(curFlag, true);
            //2. Show pathfinder line:
            EnvironmentManager.instance().showPathfinderLineBetweenPoints(lastFlag, curFlag);


            return true;
        }
    }
}

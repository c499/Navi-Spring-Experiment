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
    class TravelTask : Task
    {
        const int WAIT_START = 0, STARTED = 1;
        int state = WAIT_START;
        //Variables for task, while waiting for trigger press.
        bool insideCentralArea = false;
        //Variables for task, once started
        int curFlag;
        Vector3 curTargetFlagPosition_OnFloor;//Y=0
        Vector3 curOriginalPosition_OnFloor;

        public TravelTask(TaskTrialData taskData) : base( taskData) {
              
        }

        public override void allocateTask() {
            NavigationControl.instance().setTechnique(taskData.travellingTrialData.technique);
            NavigationControl.instance().setMFactor(taskData.travellingTrialData.M_factor);
            EnvironmentManager.instance().setupTravelScene(taskData.travellingTrialData.M_factor);//The scene chosen depends on the scaling factor
                                                                                              //Set natural navigation
            //Setup environment:
            EnvironmentManager rs = EnvironmentManager.instance();
            //Show central area: 
            rs.showCentralArea(true);
            rs.highlightCentralArea(false);
            //Show all flags and highlight those in path            
            for (int f = 0; f < 6; f++)
            {
                rs.showFlag(f, true);
                rs.highlightFlag(f, taskData.travellingTrialData.path.hasFlag(f));
            }
            //Show central text. 
            //rs.setupCentralText("Travel to: "+ taskData.travellingTrialData.path);
            //rs.showCentralText(false);//Not yet, we need to know head position to place it in the right location, before showing.


        }
        public override void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR) {
            switch (state) {
                case WAIT_START:
                    Vector3 headPosInVR_OnFloor = headToVR; headPosInVR_OnFloor.y = 0;
                    curOriginalPosition_OnFloor = headPosInVR_OnFloor; //Keep it, we need to know the starting position if travel task starts.
                    insideCentralArea = headPosInVR_OnFloor.magnitude < 0.3;
                    EnvironmentManager.instance().centralText("Travel(WAIT)" + insideCentralArea + "," + headPosInVR_OnFloor.magnitude);
                    break;
                case STARTED:
                    _updateTravel( headToTracking, delta_headToTracking, headToVR,  delta_headToVR,  time,  cur_M_Factor);
                    break;
            }  
        }

        public override void triggerIsPressed(bool isPressed)
        {
            if (state == WAIT_START && isPressed && insideCentralArea)
            {
                _setupNextFlagTarget();
                state = STARTED;
            }
        }
        protected void _updateTravel(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor)
        {

            //A. Update dependent variables: 
            taskData.travellingTrialData.realDistanceTravelled += delta_headToTracking.magnitude;
            taskData.travellingTrialData.virtualDistanceTravelled += delta_headToVR.magnitude;
            Vector3 headPosInVR_OnFloor = headToVR; headPosInVR_OnFloor.y = 0;
            float curDeviation = _computeDeviation(curTargetFlagPosition_OnFloor - curOriginalPosition_OnFloor, headPosInVR_OnFloor - curOriginalPosition_OnFloor);
            taskData.travellingTrialData.totalDeviation += curDeviation;
            taskData.travellingTrialData.numSamples++;
            //B. Update File
            if (taskData.travellingTrialData.M_factor != M_FACTOR.M_NONE)
            {
                taskData.travellingTrialData.technique = NavigationTechnique.SPRINGGRID;
            }
            string entry = "" + taskData.travellingTrialData.UserID + "," +
                               "T" + taskData.travellingTrialData.technique + "," +
                               taskData.travellingTrialData.M_factor + "," +
                               taskData.travellingTrialData.length + "," +
                               taskData.travellingTrialData.path + "," +
                               +time + "," + headToTracking + "," + headToVR + "," + cur_M_Factor + "," + curDeviation + "\n";
            entries.Add(entry);
            //B. Check if task finished and go to next step. 
            float travelDistance = (curTargetFlagPosition_OnFloor - headPosInVR_OnFloor).magnitude;
            EnvironmentManager.instance().centralText("Travel(GO): target=" + curFlag + "D=" + travelDistance + "K=" + cur_M_Factor
                                                   + "\n target:" + curTargetFlagPosition_OnFloor.ToString()
                                                   + "\n headPos:" + headPosInVR_OnFloor);
            if (travelDistance < NavigationControl.thresholdTravelDistance())
            {
                _finished = !_setupNextFlagTarget();//Try and setup the next flag, or notify task finished
                curOriginalPosition_OnFloor = headPosInVR_OnFloor;
            }
        
        }

        public override void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents) {
            if (taskData.travellingTrialData.M_factor != M_FACTOR.M_NONE && taskData.travellingTrialData.technique == NavigationTechnique.HOMOGENEOUS)
            {
                taskData.travellingTrialData.technique = NavigationTechnique.SPRINGGRID;
            }
            string entry = "" + taskData.travellingTrialData.UserID + ","
                            + "T" + taskData.travellingTrialData.technique + ","
                            + taskData.travellingTrialData.M_factor + ","
                            + taskData.travellingTrialData.length + ","
                            + taskData.travellingTrialData.path + ","
                            + taskData.travellingTrialData.T_TCT + ","
                            + (taskData.travellingTrialData.totalDeviation / taskData.travellingTrialData.numSamples) + ","
                            + taskData.travellingTrialData.realDistanceTravelled + ","
                            + taskData.travellingTrialData.virtualDistanceTravelled + "\n";
            travelContents.Add(entry);
        }

        public override void deallocateTask() {
            //WRITE FILE FOR THIS TRIAL:
            if (taskData.travellingTrialData.M_factor != M_FACTOR.M_NONE && taskData.travellingTrialData.technique == NavigationTechnique.HOMOGENEOUS)
            {
                taskData.travellingTrialData.technique = NavigationTechnique.SPRINGGRID;
            }
            string fileName = Application.dataPath + "/../ExperimentResults/" +
                             taskData.travellingTrialData.UserID + "_" +
                               "T" + taskData.travellingTrialData.technique + "_" +
                               taskData.travellingTrialData.M_factor + "_" +
                               "L"+taskData.travellingTrialData.length+".csv";
            System.IO.File.WriteAllLines(fileName, entries.ToArray());

            //SETUP SCENE FOR FOLLOWING TASKS      
            //EnvironmentManager.instance().setupTravelScene(M_FACTOR.M_EMPTY);//Set the empty scene, so that user can return to centre.;
        }

        protected float _computeDeviation(Vector3 a, Vector3 b) {
            float distance= Vector3.Cross(a, b).magnitude; //Returns closest distance between two vectors (shared origin).
            //Distance can be positive or negative. Lets do absolute value
            return (distance > 0 ? distance : -distance);
        }

        protected bool _setupNextFlagTarget() {
            //Get base flag
            curFlag= taskData.travellingTrialData.path.getFlagFromOrder(taskData.travellingTrialData.curStep++);
            if (curFlag < 0 || curFlag > 5)
                return false;
            //Apply randomization
            curFlag=(curFlag+ taskData.travellingTrialData.randomFlagOffset)% 6;

            //Valid Flag-> Setup scene
            curTargetFlagPosition_OnFloor = EnvironmentManager.instance().getPositionOfFlagNumber(curFlag);
            curTargetFlagPosition_OnFloor.y = 0;
            //0. Set all flags to gray
            EnvironmentManager.instance().highlightAllFlags(false);
            //1. Highlight flag in green
            EnvironmentManager.instance().highlightFlag(curFlag, true);



            return true;
        }
    }
}

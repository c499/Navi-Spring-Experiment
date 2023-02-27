using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation
{
    class ReturnToCentreTask : Task
    {
        public ReturnToCentreTask(TaskTrialData taskData) : base(taskData)
        {
        }

        public override void allocateTask() {
            //Set natural navigation
            NavigationControl.instance().setMFactor( M_FACTOR.M_NONE);
            NavigationControl.instance().resetDrift();

            //Setup environment:
            EnvironmentManager rs = EnvironmentManager.instance();
            //Show central area: 
            rs.showCentralArea(true);
            rs.highlightCentralArea(true);
            //Hide all flags            
            for (int f = 0; f < 6; f++) rs.showFlag(f, false);
           
        }
        public override void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR) {
            
            if (!_finished)
            {
                UnityEngine.Vector3 headPosInVR_OnFloor = headToVR; headPosInVR_OnFloor.y = 0;
                _finished = (headPosInVR_OnFloor.magnitude < 0.25f);
                EnvironmentManager.instance().centralText("Please, return to the centre"/*+ _finished + ","+ headPosInVR_OnFloor.magnitude*/);
            }

        }
        public override void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents) {
            return;
        }
        public override void deallocateTask() {
            //EnvironmentManager.instance().centralText("END RETURN TO CENTRE"); ;
        }
    }
}

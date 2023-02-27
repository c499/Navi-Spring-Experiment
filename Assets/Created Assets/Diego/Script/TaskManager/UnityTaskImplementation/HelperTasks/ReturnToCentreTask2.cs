using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation
{
    class ReturnToCentreTask2 : Task
    {
        const int RETURN_TO_CENTRE = 0, WAIT_START = 1;
        int state = RETURN_TO_CENTRE;
        bool insideCentralArea;
        public ReturnToCentreTask2(TaskTrialData taskData) : base(taskData)
        {
            insideCentralArea = true;
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
            rs.stopEffect(SoundEffects.TICK_TACK_FEEDBACK);
            rs.playEffect(SoundEffects.RELAX_FEEDBACK);
           
        }
        public override void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR) {
            UnityEngine.Vector3 headPosInVR_OnFloor = headToVR; headPosInVR_OnFloor.y = 0;
            switch (state) {
                case RETURN_TO_CENTRE:
                    if(EnvironmentManager.instance().english)
                        EnvironmentManager.instance().centralText("Please, return to the centre"/* + _finished + "," + headPosInVR_OnFloor.magnitude*/);
                    else
                        EnvironmentManager.instance().centralText("Por favor, regresa al centro."/* + _finished + "," + headPosInVR_OnFloor.magnitude*/);

                    //Check if we arrived, and go to next state
                    if (headPosInVR_OnFloor.magnitude < 0.25f)
                    {
                        EnvironmentManager.instance().playEffect(SoundEffects.POSITIVE_FEEDBACK);
                        //Prepare environment: Show the flags in the next trial, to let the user plan the path
                        EnvironmentManager.instance().setupTravelScene(taskData.travellingTrialData.M_factor);//The scene chosen depends on the scaling factor
                        EnvironmentManager rs = EnvironmentManager.instance();
                        rs.highlightCentralArea(false);
                        for (int f = 0; f < 6; f++)
                        {
                            rs.showFlag(f, true);
                            rs.highlightFlag(f, false);
                        }
                        //Show informative text about the sequence and highlight flags too:
                        string text;
                        if (EnvironmentManager.instance().english)
                            text = "PLEASE GO TO TARGETS:\n ";
                        else
                            text = "POR FAVOR, VE A:\n ";
                            for (int f = 0; f < taskData.travellingTrialData.length+1; f++) {
                            //Get base flag and apply randomization offset.
                            int curFlag = taskData.travellingTrialData.path.getFlagFromOrder(f);
                            curFlag = (curFlag + taskData.travellingTrialData.randomFlagOffset) % 6;
                            text += curFlag+" ";
                            rs.highlightFlag(curFlag, true);
                        }
                        if (EnvironmentManager.instance().english)
                            text += "\nPress TRIGGER when ready...";
                        else
                            text += "\nPulsa TRIGGER cuando estes listo/a...";
                        EnvironmentManager.instance().centralText( text);
                        //Go to next state
                        state = WAIT_START;
                    } 
                    else
                        EnvironmentManager.instance().reorientCentralArea(headPosInVR_OnFloor);

                    break;
                case WAIT_START:
                    {
                        insideCentralArea = headPosInVR_OnFloor.magnitude < 0.3;
                        if (!insideCentralArea) {
                            EnvironmentManager.instance().playEffect(SoundEffects.NEGATIVE_FEEDBACK);
                            EnvironmentManager rs=EnvironmentManager.instance();
                            //Show central area: 
                            rs.showCentralArea(true);
                            rs.highlightCentralArea(true);
                            rs.highlightAllFlags(false);
                            state = RETURN_TO_CENTRE;
                        }
                        //EnvironmentManager.instance().centralText("Travel(WAIT)" + insideCentralArea + "," + headPosInVR_OnFloor.magnitude);
                    }
                    break;

            }
        }

        public override void triggerIsPressed(bool isPressed)
        {
            //This task finished once the user presses the trigger while standing in the area.
            _finished = (state == WAIT_START && isPressed && insideCentralArea);
            //Now show feedback related to trigger presses.
            if (!_finished && isPressed)
                EnvironmentManager.instance().playEffect(SoundEffects.NEGATIVE_FEEDBACK);
            if (_finished)
            {
                EnvironmentManager.instance().playEffect(SoundEffects.CROWD_FEEDBACK);
                EnvironmentManager.instance().stopEffect(SoundEffects.RELAX_FEEDBACK);
            }
        }

        public override void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents) {
            return;
        }
        public override void deallocateTask() {
           // EnvironmentManager.instance().centralText("END RETURN TO CENTRE"); ;
        }
    }
}

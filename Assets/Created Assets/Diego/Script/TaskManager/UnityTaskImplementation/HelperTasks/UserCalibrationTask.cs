using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation.HelperTasks
{
    class UserCalibrationTask: Task { 
        const int RETURN_TO_CENTRE = 0, WAIT_STANDING = 1,  WAIT_CROACHING=2;
        int state = RETURN_TO_CENTRE;
        bool insideCentralArea;
        float standingHeight, croachingHeight;

        public UserCalibrationTask(TaskTrialData taskData) : base(taskData)
        {
            insideCentralArea = true;
        }

        public override void allocateTask()
        {
            //Set natural navigation
            NavigationControl.instance().setMFactor(M_FACTOR.M_NONE);
            NavigationControl.instance().resetDrift();

            //Setup environment:
            EnvironmentManager rs = EnvironmentManager.instance();
            rs.playEffect(SoundEffects.RELAX_FEEDBACK);
            //Show central area: 
            rs.showCentralArea(true);
            rs.highlightCentralArea(true);
            //Hide all flags            
            for (int f = 0; f < 6; f++) rs.showFlag(f, false);
            rs.stopEffect(SoundEffects.TICK_TACK_FEEDBACK);
            rs.playEffect(SoundEffects.RELAX_FEEDBACK);

        }
        public override void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR)
        {
            UnityEngine.Vector3 headPosInVR_OnFloor = headToVR; headPosInVR_OnFloor.y = 0;
            EnvironmentManager.instance().setCentralTextHeight(headToVR.y);//Try to force users to look straight while standing/croaching...
            switch (state)
            {
                case RETURN_TO_CENTRE:
                    if (EnvironmentManager.instance().english)
                        EnvironmentManager.instance().centralText("Please, return to the centre."/* + _finished + "," + headPosInVR_OnFloor.magnitude*/);
                    else
                        EnvironmentManager.instance().centralText("Por favor, regresa al centro."/* + _finished + "," + headPosInVR_OnFloor.magnitude*/);
                    //Check if we arrived, and go to next state
                    if (headPosInVR_OnFloor.magnitude < 0.25f)
                    {
                        EnvironmentManager.instance().playEffect(SoundEffects.POSITIVE_FEEDBACK);
                        //Show informative text about the sequence and highlight flags too:
                        string text;
                        if (EnvironmentManager.instance().english)
                            text = "Please, stand up comfortably and press TRIGGER.\n ";
                        else text = "Por favor, ponte recto, en postura comoda y pulsa TRIGGER.\n";
                        EnvironmentManager.instance().centralText(text);
                        //Go to next state
                        state = WAIT_STANDING;
                    }else
                        EnvironmentManager.instance().reorientCentralArea(headPosInVR_OnFloor);

                    break;
                case WAIT_STANDING:
                    {
                        standingHeight = headToVR.y;
                        //EnvironmentManager.instance().centralText("Travel(WAIT)" + insideCentralArea + "," + headPosInVR_OnFloor.magnitude);
                    }
                    break;
                case WAIT_CROACHING:
                    {
                        croachingHeight = headToVR.y;
                        //EnvironmentManager.instance().centralText("Travel(WAIT)" + insideCentralArea + "," + headPosInVR_OnFloor.magnitude);
                    }
                    break;

            }
        }

        public override void triggerIsPressed(bool isPressed)
        {
            if (!isPressed)
                return;
            if (state == WAIT_STANDING)
            {
                EnvironmentManager.instance().playEffect(SoundEffects.POSITIVE_FEEDBACK);
                if(EnvironmentManager.instance().english)
                    EnvironmentManager.instance().centralText("Please, kneel down (knight-style, one knee on the ground) and press TRIGGER.");
                else
                    EnvironmentManager.instance().centralText("Por favor, arrodillate estilo caballero (una rodilla en tierra) y pulsa TRIGGER.");
                state = WAIT_CROACHING;
            }
            else if (state == WAIT_CROACHING) {
                EnvironmentManager.instance().playEffect(SoundEffects.CROWD_FEEDBACK);
                EnvironmentManager.instance().centralText("DONE!");
                EnvironmentManager.instance().stopEffect(SoundEffects.RELAX_FEEDBACK);
                EnvironmentManager.instance().setupUserParameters(standingHeight, croachingHeight);
                EnvironmentManager.instance().setCentralTextHeight(standingHeight); //Set the information board at a comfortable height to look at it.
                _finished = true;
            }           
        }

        public override void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents)
        {
            return;
        }
        public override void deallocateTask()
        {
            // EnvironmentManager.instance().centralText("END RETURN TO CENTRE"); ;
        }
    }
}

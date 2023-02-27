using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation.HelperTasks
{
    class IntroductionTask: Task {
        public abstract class IntroductionTask_Tunning{
            public abstract void allocateTask();
            public abstract void deallocateTask();

        };
        string[] texts;
        int curText;
        IntroductionTask_Tunning tunning; 
        public IntroductionTask(string[] texts, TaskTrialData taskData, IntroductionTask_Tunning tunning=null) : base(taskData)
        {
            this.texts = (string[])texts.Clone();
            curText = 0;
            this.tunning = tunning;
        }

        public override void allocateTask()
        {
            if(tunning!=null)tunning.allocateTask();

        }
        public override void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR)
        {
            EnvironmentManager.instance().centralText(texts[curText]) ;
            EnvironmentManager.instance().setCentralTextHeight(headToVR.y-0.20f);//Set it a bit lower than the eyes...
        }

        public override void triggerIsPressed(bool isPressed)
        {
            if (!isPressed)
                return;
            EnvironmentManager.instance().playEffect(SoundEffects.POSITIVE_FEEDBACK);
            this.curText++;
            if (curText == texts.Length)
                _finished = true;          
        }

        public override void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents)
        {
            return;
        }
        public override void deallocateTask()
        {
            if (tunning != null) tunning.deallocateTask();// EnvironmentManager.instance().centralText("END RETURN TO CENTRE"); ;
        }
    }
}

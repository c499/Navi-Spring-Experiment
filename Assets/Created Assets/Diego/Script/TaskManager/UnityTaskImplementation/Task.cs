using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Created_Assets.Diego.Script.TaskManager
{
    /**
     * This is an abstract class that manages the execution of a task in our experimental environment. 
     * It contains:
     *      - Description of the task to complete (TaskTrialData)
     *      - References to relevant modules (EnvironmentManager)
     *      
     */
    public abstract class Task
    {
        public TaskTrialData taskData;
        protected bool _finished;
        protected List<string> entries;

        public Task(TaskTrialData taskData) {
            this.taskData = taskData;
            _finished = false;
            entries = new List<string>();
        }
        public abstract void allocateTask();
        public virtual bool finished() { return _finished; }
        public abstract void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR);
        public virtual void triggerIsPressed(bool isPressed) {
            return;
        }
        public abstract void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents);
        public abstract void deallocateTask();
    }
}

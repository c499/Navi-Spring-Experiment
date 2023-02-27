using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Created_Assets.Diego.Script;
using UnityEngine;

namespace Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation
{
    class CompositeTask : Task
    {
        protected Queue<Task> subtasks;
        Task curTask;
        List<string> travelContent, maneuvreContent, questionnaireContent;

        public CompositeTask(TaskTrialData taskData) : base(taskData) {
            subtasks=new Queue<Task>();
            travelContent = new List<string>();
            maneuvreContent = new List<string>();
            questionnaireContent = new List<string>();
        }
        public void addTask(Task t) { subtasks.Enqueue(t); }
        public override void allocateTask() {
            _allocateNextSubtask();
        }

        protected void _allocateNextSubtask() {
            if (subtasks.Count <= 0)
            {
                curTask = null;
                _finished = true;
                return;
            }
            //Get next subtask, and allocate it. 
            curTask = subtasks.Dequeue();
            curTask.allocateTask();
        }

        public override void update(UnityEngine.Vector3 headToTracking, UnityEngine.Vector3 delta_headToTracking, UnityEngine.Vector3 headToVR, UnityEngine.Vector3 delta_headToVR, float time, float cur_M_Factor, UnityEngine.Vector3 handInVR) {
            if (curTask.finished())
            {
                curTask.writeGlobalParametersToCollection(travelContent, maneuvreContent, questionnaireContent);
                curTask.deallocateTask();
                _allocateNextSubtask();
            }
            else
                curTask.update(headToTracking, delta_headToTracking, headToVR, delta_headToVR, time, cur_M_Factor,handInVR);
            
        }
        public override void triggerIsPressed(bool isPressed)
        {
            curTask.triggerIsPressed(isPressed);
            return;
        }
        public override void writeGlobalParametersToCollection(List<string> travelContents, List<string> maneuvreContents, List<string> questionnaireContents) {
            travelContents.AddRange(this.travelContent) ;
            maneuvreContents.AddRange(this.maneuvreContent);
            questionnaireContents.AddRange(this.questionnaireContent);
        }
        public override void deallocateTask() {
            subtasks = null;
           
        }
    }
}

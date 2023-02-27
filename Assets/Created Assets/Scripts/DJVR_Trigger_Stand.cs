namespace VRTK
{
    using UnityEngine;
    using UnityEngine.UI;
    using Valve.VR;
    using System.Linq;
    using System.Collections;
    using System;

    public class DJVR_Trigger_Stand : MonoBehaviour
    {
        #region Fields

        [Header("Misc Settings")]


        protected DJVR_Trigger_Base triggerbase;
        protected GameObject sequenceCanvas;


        #endregion Fields

        #region Methods

        private void Start()
        {
            triggerbase = transform.gameObject.GetComponent<DJVR_Trigger_Base>();
            sequenceCanvas = GameObject.Find("SequenceCanvas");
        }

        // Update is called once per frame
        private void Update()
        {
        }

       

       

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "[CameraRig]")
            {
                Debug.Log("This Object: " + transform.name + " is hit by: " + other.gameObject.name);

                if (triggerbase.numberOfTriggers < triggerbase.maximumTriggers)
                {
                    triggerbase.TriggerEvent();
                }
                if (this.gameObject.tag != "NotSequence")
                {
                    triggerbase.SequenceCheck();
                }
                else
                {
                    sequenceCanvas.GetComponentInChildren<Text>().text = "";
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "[CameraRig]")
            {
                if (triggerbase.numberOfTriggers < triggerbase.maximumTriggers)
                {
                    Debug.Log("This Object: " + transform.name + " is hit by: " + other.gameObject.name);

                    triggerbase.time += Time.deltaTime;
                    Debug.Log("time: " + triggerbase.time);
                    if (triggerbase.setTimer)
                    {
                        if (triggerbase.time >= triggerbase.triggerTime)
                        {
                            triggerbase.TriggerEvent();
                        }
                    }
                    else
                    {
                        triggerbase.triggerTime = 0;
                        if (triggerbase.time >= triggerbase.triggerTime)
                        {
                            triggerbase.TriggerEvent();
                        }
                    }
                }
            }
        }

        


        #endregion Methods
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;

public class DJVR_Trigger_Controller : MonoBehaviour
{
    #region Fields

    [Header("Controller Settings")]
    public float maxTriggerTime = 5;

    protected GameObject notSequence;
    private List<GameObject> ListOfNotSequences = new List<GameObject>();
    private float heldTime = -1;

    #endregion Fields

    #region Methods

    private void Start()
    {
        if (GetComponent<VRTK_ControllerEvents>() == null)
        {
            Debug.LogError("VRTK_RoomExtender_ControllerExample is required to be attached to a SteamVR Controller that has the VRTK_ControllerEvents script attached to it");
            return;
        }
        GetComponent<VRTK_ControllerEvents>().TriggerClicked += new ControllerInteractionEventHandler(DoTriggerClicked);
        GetComponent<VRTK_ControllerEvents>().ApplicationMenuPressed += new ControllerInteractionEventHandler(DoApplicationMenuPressed);
    }

    /// <summary>
    /// Method is called when a VR controller presses a trigger.Triggers DJVR_Trigger_ controllerTrigger()
    /// </summary>
    private void DoTriggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        foreach (GameObject notSequence in ListOfNotSequences)
        {
            notSequence.GetComponent<DJVR_Trigger_>().ControllerTrigger();
        }
    }

    /// <summary>
    /// Method is called when a VR controller presses a trigger.Triggers DJVR_Trigger_Base FlashSequence()
    /// </summary>
    private void DoApplicationMenuPressed(object sender, ControllerInteractionEventArgs e)
    {
        GameObject AOI = GameObject.FindGameObjectWithTag("PointOfInterest");

        AOI.GetComponentInChildren<DJVR_Trigger_Base>().FlashSequence();
    }

    /// <summary>
    /// Finds all gameObjects with tag "NotSequence" and arranges them into a list
    /// </summary>
    private void GetTriggers()
    {
        ListOfNotSequences.AddRange(GameObject.FindGameObjectsWithTag("NotSequence"));
        ListOfNotSequences = ListOfNotSequences.Distinct().ToList();

        foreach (GameObject notSequence in ListOfNotSequences.ToArray())
        {
            if (notSequence == null)
            {
                ListOfNotSequences.Remove(notSequence);
            }

            if (ListOfNotSequences.Count == 0)
            {
                ListOfNotSequences.Clear();
            }
        }
    }

    /// <summary>
    /// Resets the position of the CameraRig to position 0,0,0
    /// </summary>
    private void Reset()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("PointOfInterest");

        obj.GetComponentInChildren<DJVR_Trigger_Base>().ResetCameraRig();
        Debug.Log("Reset!!!!!");
    }

    /// <summary>
    /// Method is called every frame. Starts a timer if a VR controllers trigger is pressed and exectues reset if the trigger is held for longer than "maxTriggerTime".
    /// </summary>
    private void Update()
    {
        GetTriggers();

        if (GetComponent<VRTK_ControllerEvents>().triggerPressed)
        {
            heldTime += Time.deltaTime;

            //Debug.Log("HeldTime: " + heldTime);
        }

        if (GetComponent<VRTK_ControllerEvents>().triggerPressed == false)
        {
            heldTime = 0;
        }

        if (heldTime >= maxTriggerTime)
        {
            Reset();
            heldTime = 0;
        }
    }

    #endregion Methods
}
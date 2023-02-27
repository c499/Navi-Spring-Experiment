using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class DJVR_Trigger_ : VRTK_InteractableObject
{
    #region Fields

    

    [Header("Trigger Settings")]
    public bool Touching = false;

    public bool Standing = false;
    public bool ControllerTriggering = false;

    [Header("Touch Settings")]
    public bool onlyTouchInAreaClose = true;

    public bool onlyTouchInAreaMedium = false;
    public bool canTouchInBoth = false;
    public bool canTouchAlways = false;

    [Header("Stand Settings")]
    public bool onlyStandInAreaClose = true;

    public bool onlyStandInAreaMedium = false;
    public bool canStandInBoth = false;
    public bool canStandAlways = false;

    [Header("Controller Trigger Settings")]
    public bool onlyTriggerInAreaClose = true;

    public bool onlyTriggerInAreaMedium = false;
    public bool canTriggerInBoth = false;
    public bool canTriggerAlways = false;

    public bool notSequence = false;
    public Color thisColour;
    public bool glowInArea = false;
    protected DJVR_Trigger_Base triggerbase;
    protected GameObject sequenceCanvas;
    protected EventManager eventManager;

    #endregion Fields

    #region Methods

    public override void StartTouching(GameObject currentTouchingObject)
    {
        if (Touching)
        {
            base.StartTouching(currentTouchingObject);

            Debug.Log("Current Touching Object: " + currentTouchingObject);

            if (triggerbase.numberOfTriggers < triggerbase.maximumTriggers)
            {
                if (onlyTouchInAreaClose && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideClose)
                {
                    rumbleOnTouch.x = 0.5f;
                    rumbleOnTouch.y = 2500f;

                    triggerbase.TriggerEvent();
                    triggerbase.ConfirmSequence();
                }
                else
                {
                    transform.GetComponent<Renderer>().sharedMaterial.color = thisColour;
                    rumbleOnTouch.x = 0f;
                    rumbleOnTouch.y = 0f;
                }

                if (onlyTouchInAreaMedium && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideMedium)
                {
                    rumbleOnTouch.x = 0.5f;
                    rumbleOnTouch.y = 2500f;

                    triggerbase.TriggerEvent();
                    triggerbase.ConfirmSequence();
                }

                if (canTouchInBoth && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideMedium && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideClose)
                {
                    rumbleOnTouch.x = 0.5f;
                    rumbleOnTouch.y = 2500f;

                    triggerbase.TriggerEvent();
                    triggerbase.ConfirmSequence();
                }

                if (canTouchAlways)
                {
                    rumbleOnTouch.x = 0.5f;
                    rumbleOnTouch.y = 2500f;

                    triggerbase.TriggerEvent();
                    triggerbase.ConfirmSequence();
                }
            }
        }
    }


    public override void Grabbed(GameObject currentGrabbingObject)
    {

        base.Grabbed(currentGrabbingObject);

        Debug.Log("Object Grabbed: " + transform.name);
        eventManager.GetComponent<InputVCR>().SyncProperty("Object Grabbed: ", transform.name);

    }


    public override void Ungrabbed(GameObject previousGrabbingObject)
    {

        base.Ungrabbed(previousGrabbingObject);

        Debug.Log("Object Released: " + transform.name);
        eventManager.GetComponent<InputVCR>().SyncProperty("Object Released: ", transform.name);

    }

    public void ControllerTrigger()
    {
        if (ControllerTriggering)
        {
            if (triggerbase.numberOfTriggers < triggerbase.maximumTriggers)
            {
                if (onlyTriggerInAreaClose && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideClose)
                {
                    triggerbase.TriggerEvent();
                    triggerbase.ConfirmSequence();
                }

                if (onlyTriggerInAreaMedium && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideMedium)
                {
                    triggerbase.TriggerEvent();
                    triggerbase.ConfirmSequence();
                }

                if (canTriggerInBoth && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideMedium && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideClose)
                {
                    triggerbase.TriggerEvent();
                    triggerbase.ConfirmSequence();
                }

                if (canTriggerAlways)
                {
                    triggerbase.TriggerEvent();
                    triggerbase.ConfirmSequence();
                }
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        triggerbase = transform.gameObject.GetComponent<DJVR_Trigger_Base>();
        sequenceCanvas = GameObject.Find("SequenceCanvas");
        eventManager = FindObjectOfType<EventManager>();

        if (transform.tag == "NotSequence")
        {
            notSequence = true;
        }

        //thisColour = gameObject.GetComponent<Renderer>().material.color;
    }

    protected override void Update()
    {
        base.Update();

        if (glowInArea)
        {
            if (this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideClose)
            {
                transform.GetComponent<Renderer>().sharedMaterial.EnableKeyword("_EMISSION");
                transform.GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", thisColour);
            }
            else
            {
                transform.GetComponent<Renderer>().sharedMaterial.DisableKeyword("_EMISSION");
            }
        }
    }

    private void OnEnable()
    {
        thisColour = gameObject.GetComponent<Renderer>().material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Standing)
        {
            if (other.gameObject.name == "[CameraRig]" || other.gameObject.tag == "Ball")
            {
                if (onlyStandInAreaClose && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideClose)
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

                if (onlyStandInAreaMedium && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideMedium)
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

                if (canStandInBoth && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideMedium && this.transform.GetComponentInParent<DJVR_Trigger_AOI>().insideClose)
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

                if (canStandAlways)
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
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Standing)
        {
            if (other.gameObject.name == "[CameraRig]" || other.gameObject.tag == "Ball")
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
    }

    private void OnValidate()
    {
        if (onlyTouchInAreaClose && onlyTouchInAreaMedium)
        {
            onlyTouchInAreaClose = false;
            onlyTouchInAreaMedium = false;
            canTouchInBoth = true;
        }

        if (onlyTouchInAreaClose && canTouchInBoth)
        {
            onlyTouchInAreaClose = false;
            canTouchInBoth = true;
        }

        if (onlyTouchInAreaMedium && canTouchInBoth)
        {
            onlyTouchInAreaMedium = false;
            canTouchInBoth = true;
        }

        if (canTouchAlways && canTouchInBoth)
        {
            canTouchAlways = true;
            canTouchInBoth = false;
        }

        if (canTouchAlways && onlyTouchInAreaMedium)
        {
            canTouchAlways = true;
            onlyTouchInAreaMedium = false;
        }

        if (canTouchAlways && onlyTouchInAreaClose)
        {
            canTouchAlways = true;
            onlyTouchInAreaClose = false;
        }

        if (onlyTriggerInAreaClose && onlyTriggerInAreaMedium)
        {
            onlyTriggerInAreaClose = false;
            onlyTriggerInAreaMedium = false;
            canTriggerInBoth = true;
        }

        if (onlyTriggerInAreaClose && canTriggerInBoth)
        {
            onlyTriggerInAreaClose = false;
            canTriggerInBoth = true;
        }

        if (onlyTriggerInAreaMedium && canTriggerInBoth)
        {
            onlyTriggerInAreaMedium = false;
            canTriggerInBoth = true;
        }

        if (canTriggerAlways && canTriggerInBoth)
        {
            canTriggerAlways = true;
            canTriggerInBoth = false;
        }

        if (canTriggerAlways && onlyTriggerInAreaMedium)
        {
            canTriggerAlways = true;
            onlyTriggerInAreaMedium = false;
        }

        if (canTriggerAlways && onlyTriggerInAreaClose)
        {
            canTriggerAlways = true;
            onlyTriggerInAreaClose = false;
        }

        if (onlyStandInAreaClose && onlyStandInAreaMedium)
        {
            onlyStandInAreaClose = false;
            onlyStandInAreaMedium = false;
            canStandInBoth = true;
        }

        if (onlyStandInAreaClose && canStandInBoth)
        {
            onlyStandInAreaClose = false;
            canStandInBoth = true;
        }

        if (onlyStandInAreaMedium && canStandInBoth)
        {
            onlyStandInAreaMedium = false;
            canStandInBoth = true;
        }

        if (canStandAlways && canStandInBoth)
        {
            canStandAlways = true;
            canStandInBoth = false;
        }

        if (canStandAlways && onlyStandInAreaMedium)
        {
            canStandAlways = true;
            onlyStandInAreaMedium = false;
        }

        if (canStandAlways && onlyStandInAreaClose)
        {
            canStandAlways = true;
            onlyStandInAreaClose = false;
        }

        if (transform.tag == "NotSequence")
        {
            notSequence = true;
        }
    }
}

#endregion Methods
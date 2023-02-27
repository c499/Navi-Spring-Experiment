using UnityEngine;

public class DJVR_Navigation : MonoBehaviour
{
    #region Fields

    public static DJVR_Navigation current;

    [Header("Multiplier Settings")]
    public bool additionalMovementEnabled = true;

    public float realMultiplier;

    [Range(0, 20)]
    public float additionalMovementMultiplier = 1.0f;

    [Range(0, 20)]
    public float defaultAdditionalMovementMultiplier = 2.0f;

    protected Transform headsetTransform;
    protected Transform controllerTransform;
    protected Transform cameraRig;
    protected Vector3 lastHeadsetPosition;
    protected Vector3 incrementReality;
    protected EventManager eventManager;
    private Vector3 positionReal;
    private Vector3 positionVR;
    private Vector3 driftPos;
    private float driftExcY;


    #endregion Fields

    #region Methods

    /// <summary>
    /// Finds all associated GameObjects and Scripts and links them all to the class fields. 
    /// </summary>
    private void Start()
    {
        headsetTransform = VRTK.DeviceFinder.HeadsetTransform();
        cameraRig = GameObject.FindObjectOfType<SteamVR_PlayArea>().gameObject.transform;
        eventManager = EventManager.current;

        if (GameObject.Find("[CameraRig]").transform.GetChild(1) != null)
        {
            controllerTransform = GameObject.Find("[CameraRig]").transform.GetChild(1).transform;
        }
    }

    private void Awake()
    {
        current = this;
    }

    // Update is called once per frame
    private void Update()
    {
        Move();
        ShowDrift();
        realMultiplier = additionalMovementMultiplier + 1;
    }

    private void UpdateLastMovement()
    {
        incrementReality = headsetTransform.localPosition - lastHeadsetPosition;
        incrementReality.y = 0;
        lastHeadsetPosition = headsetTransform.localPosition;
    }
    private void MoveCameraRig()
    {
        if (additionalMovementEnabled)
        {
            cameraRig.localPosition += incrementReality * additionalMovementMultiplier;
        }
    }

    private void Move()
    {
        UpdateLastMovement();
        MoveCameraRig();
    }

    private void ShowDrift()
    {
        positionReal = headsetTransform.localPosition;
        positionVR = headsetTransform.position;

        Vector3 cameraRigDrift = cameraRig.position;
        cameraRigDrift.y = 0f;

        float driftIncY = Vector3.Distance(positionReal, positionVR);

        float lookAngle = Mathf.Atan2(headsetTransform.forward.z, headsetTransform.forward.x) * Mathf.Rad2Deg;


        if (eventManager.recording == true)
        {
            eventManager.GetComponent<InputVCR>().SyncProperty("Time Elapsed", eventManager.GetComponent<InputVCR>().currentTime.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Current Frame", eventManager.GetComponent<InputVCR>().currentFrame.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Current Multiplier", additionalMovementMultiplier.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Current Direction(X)", lookAngle.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("position Real X", positionReal.x.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("position Real Z", positionReal.z.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("position Real Y", positionReal.y.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("position VR X", positionVR.x.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("position VR Z", positionVR.z.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("position VR Y", positionVR.y.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Controller Real X", controllerTransform.position.x.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Controller Real Z", controllerTransform.position.z.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Controller Real Y", controllerTransform.position.z.ToString());
        }

        positionReal.y = 0f;
        positionVR.y = 0f;

        driftExcY = Vector3.Distance(positionReal, positionVR);
        driftPos = positionVR - positionReal;

        if (eventManager.recording == true)
        {
            eventManager.GetComponent<InputVCR>().SyncProperty("Current Drift with Y", driftIncY.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Current Drift without Y", driftExcY.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Current Drift Position X", driftPos.x.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Current Drift Position Z", driftPos.z.ToString());
            eventManager.GetComponent<InputVCR>().SyncProperty("Current Drift Position Y", driftPos.y.ToString());
        }
    }

    private void OnValidate()
    {
        realMultiplier = additionalMovementMultiplier + 1;
    }

    #endregion Methods
}
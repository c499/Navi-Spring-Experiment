using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Assets.Created_Assets.Diego.Script.TaskManager;
using Assets.Created_Assets.Diego.Script.TaskManager.ManeuvreData;

public class SoundEffects {
    public const int POSITIVE_FEEDBACK=0;
    public const int NEGATIVE_FEEDBACK =1;
    public const int TICK_TACK_FEEDBACK =2;
    public const int CROWD_FEEDBACK = 3;
    public const int RELAX_FEEDBACK = 4;
};

public class EnvironmentManager : MonoBehaviour {
    [Header("Language")]
    public bool english;
    [Header("Experiment Objects")]
    public GameObject centralArea;
    public GameObject questionnaire;
    public GameObject parafrustum;
    public GameObject flag0;
    public GameObject flag1;
    public GameObject flag2;
    public GameObject flag3;
    public GameObject flag4;
    public GameObject flag5;
    public LineRenderer pathfinderLine;
    [Header ("Audio Feedback")]
    public AudioSource positive;
    public AudioSource negative;
    public AudioSource tickTack;
    public AudioSource crowd;
    public AudioSource relax;

   
    //User calibration data (standing height and croaching height)
    float userStandingHeight, userCroachingHeight;

    //const float rotationAngleParafrustum = 28.65f;//Half a radian... I do not like it, it feels too close.
    const float rotationAngleParafrustum = 22.5f;      //This is not as fancy as half a radian, but it feels more comfortable
    //Environment parameters that depend upon the user calibration data:
    float targetHeight;
    float halfDistance ;
    float targetToAreaDist;
    //Let's speed up things a bit...
    protected GameObject[] flags;
    protected Transform[] flagPositions;
    protected Transform[] highlightPositions;
    protected Transform[] flagLabelPositions;//Node with the label of the flag (parafrustums will look at these points)
    protected TextController centreText;
    protected GameObject centralAreaHighlight;
    protected GameObject[] questionsEnglish;
    protected GameObject[] questionsSpanish;
    protected GameObject[][] feedbackPerOption;//Seven feedback items with two options (enabled/disabled)
    protected GameObject head;

    protected void _setupQuestionnaireInternalReferences() {
        head =  GameObject.FindObjectOfType<Camera>().gameObject;
        questionsEnglish = new GameObject[4];
        questionsSpanish = new GameObject[4];
        feedbackPerOption = new GameObject[7][];

        Transform[] children = questionnaire.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
            if (child.name == "Question1")
                questionsEnglish[0] = child.gameObject;
            else if (child.name == "Question2")
                questionsEnglish[1] = child.gameObject;
            else if (child.name == "Question3")
                questionsEnglish[2] = child.gameObject;
            else if (child.name == "Question4")
                questionsEnglish[3] = child.gameObject;
            else if (child.name == "Question1_SP")
                questionsSpanish[0] = child.gameObject;
            else if (child.name == "Question2_SP")
                questionsSpanish[1] = child.gameObject;
            else if (child.name == "Question3_SP")
                questionsSpanish[2] = child.gameObject;
            else if (child.name == "Question4_SP")
                questionsSpanish[3] = child.gameObject;
            else if (child.name == "Option0")
                feedbackPerOption[0] = _getFeedbackNodes(child.gameObject);
            else if (child.name == "Option1")
                feedbackPerOption[1] = _getFeedbackNodes(child.gameObject);
            else if (child.name == "Option2")
                feedbackPerOption[2] = _getFeedbackNodes(child.gameObject);
            else if (child.name == "Option3")
                feedbackPerOption[3] = _getFeedbackNodes(child.gameObject);
            else if (child.name == "Option4")
                feedbackPerOption[4] = _getFeedbackNodes(child.gameObject);
            else if (child.name == "Option5")
                feedbackPerOption[5] = _getFeedbackNodes(child.gameObject);
            else if (child.name == "Option6")
                feedbackPerOption[6] = _getFeedbackNodes(child.gameObject);
    }

    protected GameObject[] _getFeedbackNodes(GameObject OptionI) {
        GameObject[] result = new GameObject[2];

        Transform[] children = OptionI.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
            if (child.name == "Selected")
                result[0] = child.gameObject;
            else if (child.name == "NotSelected")
                result[1] = child.gameObject;
        return result;
    }

    public void showQuestionnaire(bool show)
    {
        questionnaire.SetActive(show);
    }
    public void showQuestionnaireToUser()
    {
        questionnaire.transform.position=head.transform.position;
        questionnaire.transform.rotation = head.transform.rotation;
    }
    public void showQuestion(int curQuestion)
    {
        for (int q = 0; q < 4; q++) {
            questionsEnglish[q].SetActive(false);
            questionsSpanish[q].SetActive(false);
        }
        if (this.english)
            questionsEnglish[curQuestion].SetActive(true);
        else
            questionsSpanish[curQuestion].SetActive(true);
    }

    public int checkQuestionnaireOption(Vector3 handInVR)
    {
        float minDist =  0.055f;//This is the max radius of an answer.
        int curOption=-1;
        float curDist;
        for (int opt = 0; opt < 7; opt++) {
            //Check if this is the one...
            curDist = Vector3.Distance(feedbackPerOption[opt][0].transform.position, handInVR);
            if (curDist < minDist) {
                curOption = opt;
                minDist = curDist;
            }
            //Disable, for the time being.
            feedbackPerOption[opt][0].SetActive(false);
            feedbackPerOption[opt][1].SetActive(true);

        }

        //Highlight the current option
        if (curOption != -1) {
            feedbackPerOption[curOption][0].SetActive(true);
            feedbackPerOption[curOption][1].SetActive(false);
        }
        //And return it!
        return curOption;
    }


    // Use this for initialization
    void Start () {
        //Code for Singleton behaviour (It is not the usual patern, as this object is already created in the Unity Editor).
        _instance = this;
        //Put flags in an array (better to write algorithms)
        flags = new GameObject[6];
        flags[0] = flag0; flags[1] = flag1; flags[2] = flag2;
        flags[3] = flag3; flags[4] = flag4; flags[5] = flag5;
        //Hide all flags and central area.
        for (int f = 0; f < 6; f++)
            showFlag(f, false);
        centralArea.SetActive(false);

        //Retrieve all flag positions and keep in a local variable (avoid fetching)
        flagPositions = new Transform[6];
        flagPositions[0] = flag0.transform.GetComponentsInChildren<Transform>()[1];
        flagPositions[1] = flag1.transform.GetComponentsInChildren<Transform>()[1];
        flagPositions[2] = flag2.transform.GetComponentsInChildren<Transform>()[1];
        flagPositions[3] = flag3.transform.GetComponentsInChildren<Transform>()[1];
        flagPositions[4] = flag4.transform.GetComponentsInChildren<Transform>()[1];
        flagPositions[5] = flag5.transform.GetComponentsInChildren<Transform>()[1];
        //Retrieve all Highlight nodes and keep in local variables (avoid fectching)
        highlightPositions = new Transform[6];
        highlightPositions[0] = _getHighlightNode(flag0);
        highlightPositions[1] = _getHighlightNode(flag1);
        highlightPositions[2] = _getHighlightNode(flag2);
        highlightPositions[3] = _getHighlightNode(flag3);
        highlightPositions[4] = _getHighlightNode(flag4);
        highlightPositions[5] = _getHighlightNode(flag5);
        //Retrieve all text label nodes and keep in local variables (avoid fectching)
        flagLabelPositions = new Transform[6];
        flagLabelPositions[0] = _getLabelPositionNode(flag0);
        flagLabelPositions[1] = _getLabelPositionNode(flag1);
        flagLabelPositions[2] = _getLabelPositionNode(flag2);
        flagLabelPositions[3] = _getLabelPositionNode(flag3);
        flagLabelPositions[4] = _getLabelPositionNode(flag4);
        flagLabelPositions[5] = _getLabelPositionNode(flag5);
        //Get pointer to text area
        centreText = centralArea.GetComponentInChildren<TextController>();

        //Setup stuff related to Questionnaire
        _setupQuestionnaireInternalReferences();
        //Get pointer to highlight node in central area:
        Transform[] children = centralArea.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
            if (child.name == "RingProjector")
                this.centralAreaHighlight=child.gameObject;
        _setupNaturalScene();
        moveParaFrustumToFlag(1);


    }
    
    public void setupUserParameters(float standing, float croaching) {
        this.userStandingHeight = 0.95f * standing; //OK, when you look down, your eyes are a bit lower than when you are looking straight
        this.userCroachingHeight=1.03f * croaching;//Sme here, when you are looking up, your eyes are a bit higher...
        //Compute parameters in the environment that depend upon these...
        float targetHeight=(userStandingHeight + userCroachingHeight) / 2;
        float halfDistance = (userStandingHeight - userCroachingHeight) / 2;
        float targetToAreaDist = halfDistance / Mathf.Tan(rotationAngleParafrustum*Mathf.Deg2Rad);
        //Get all targets and set their height and distance.
        for(int f = 0; f < 6; f++)
        {
            Transform[] children = flagPositions[f].GetComponentsInChildren<Transform>();
            Vector3 v;
            foreach (Transform child in children)
            {
                if (child.name == "Line")
                {   //Move the line backwards 
                    v = child.transform.localPosition;
                    v.z = -targetToAreaDist;
                    child.transform.localPosition = v;
                    //Length of the pole connecting the target to the floor.
                    child.GetComponent<LineRenderer>().SetPositions(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, targetHeight - 0.1f, 0), });
                }
                if (child.name == "TooltipCanvas")
                {   //Height of the text area
                    v = child.transform.localPosition;
                    v.y = targetHeight;
                    child.transform.localPosition = v;
                }
                if (child.name == "TooltipCanvasAnchor")
                {   //Height of the point we will look at (same height than above)
                    v = child.transform.localPosition;
                    v.y = targetHeight;
                    child.transform.localPosition = v;
                }
            }

        }

    }

    public bool checkParafrustum(ref float posError, ref float angError)
    {
        ParaFrustum paraScript = parafrustum.GetComponentInChildren<ParaFrustum>();
        
        return paraScript.checkInside(ref posError, ref angError);
    }

    public Vector3 getParafrustumPosition()
    {
        return parafrustum.transform.position;
    }

    // Update is called once per frame
    void Update () {
		
	}

    //Singleton pattern
    protected static EnvironmentManager _instance = null;
    protected EnvironmentManager()
    {
        //Get access to all required nodes in the scene;
    }
    public static EnvironmentManager instance()
    {
        return _instance;
    }
    public void setupTravelScene(M_FACTOR factor)
    {
        switch (factor)
        {
            case M_FACTOR.M_NONE:
                _setupScene(1.0f);
                break;
            case M_FACTOR.M_2:
                _setupScene(2f);//2
                break;
            case M_FACTOR.M_4:
                _setupScene(4f);//4
                break;
            case M_FACTOR.M_8:
                _setupScene(8f);//8
                break;
            default:
                break;

        }
    }

    public Vector3 getPositionOfFlagNumber(int flagNumber)
    {
        //TODO: retrun the actual position of the flag.
        return flagPositions[flagNumber].position;
    }

    public void moveFlag(int flag, Vector3 pos)
    {
        flagPositions[flag].localPosition = pos;
    }
    //This is less efficient (looks for the node everytime)
    protected void _moveFlag(GameObject flag, Vector3 pos) {
        Transform t = flag.GetComponentsInChildren<Transform>()[1];
        t.localPosition=pos;
    }

    protected Transform _getHighlightNode(GameObject flag) { 
        Transform[] children=flag.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
            if (child.name == "RingProjector")
                return child;
        return null;
    }

    protected Transform _getLabelPositionNode(GameObject flag)
    {
        Transform[] children = flag.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
            if (child.name == "TooltipCanvasAnchor")
                return child;
        return null;
    }

    public void highlightAllFlags(bool highlight) {
        for (int f = 0; f < 6; f++)
            highlightFlag(f, highlight);
    }

    public void highlightFlag(int flag, bool highlight) {
        highlightPositions[flag].gameObject.SetActive(highlight);
    }

    public void highlightCentralArea(bool highlight) {
        centralAreaHighlight.SetActive(highlight);
    }

    public void showFlag(int flag, bool show)
    {
        flags[flag].gameObject.SetActive(show);
    }

    public void showCentralArea(bool show)
    {
        centralArea.gameObject.SetActive(show);
    }

    public void reorientCentralArea(Vector3 userFloorPosInVR) {
        /*float direction=Vector3.Dot(centralArea.transform.position, userFloorPosInVR);
        Vector3 userPosToUse;
        if (direction > 0)
            userPosToUse = new Vector3(userFloorPosInVR.x, userFloorPosInVR.y, userFloorPosInVR.z);
        else
            userPosToUse = new Vector3(-userFloorPosInVR.x, userFloorPosInVR.y, -userFloorPosInVR.z);
        centralArea.transform.LookAt(userPosToUse);*/
        centralArea.transform.LookAt(userFloorPosInVR);
    }

    public void hidePathfinderLine() {
        _plotPathfinderLine(new Vector3 ( 0,-1,0), new Vector3(0, -1.1f, 0));
    }


    //Shows the pathfinder line between two points in the scene, identified by flag number. Use -1 for centralArea.
    public void showPathfinderLineBetweenPoints(int start, int end) {
        Vector3 startPos, endPos;
        if (start != -1)
            startPos = flagPositions[start].position;
        else
            startPos = centralArea.transform.position;
        //Set end position
        if (end != -1)
            endPos = flagPositions[end].position;
        else
            endPos = centralArea.transform.position;
        //Plot between those positions:
        _plotPathfinderLine(startPos,endPos);

    }

    protected void _plotPathfinderLine(Vector3 start, Vector3 end) {
        Vector3[] positions = new Vector3[] { start, end };
        pathfinderLine.SetPositions(positions);
    }
    public void centralText(string text) {
        centreText.backText.text = text;
        centreText.frontText.text = text;
    }

    public void setCentralTextHeight(float height)
    {
        Transform[] children = centralArea.GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
            if (child.name == "TooltipCanvas")
            {
                Vector3 v = child.transform.localPosition;
                v.y = height;
                child.transform.localPosition = v;
            }
    }

    public void showParafrustum(bool show) {
        parafrustum.SetActive(show);
        parafrustum.GetComponentInChildren<ParaFrustum>().showHeadCursor(show);
    }
    public void moveParaFrustumToFlag(int flag) {
        this.parafrustum.transform.parent = flagLabelPositions[flag].transform;
        parafrustum.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void setParafrustum(PrecisionLevel head, PrecisionLevel tail, int parafrustumPosition) {
        float posTolerance = (head == PrecisionLevel.COARSE ? 0.1f : 0.05f);
        float angTolerance = (tail == PrecisionLevel.COARSE ? 10.0f : 5.0f);
        //A. set sixes of areas
        ParaFrustum paraScript= parafrustum.GetComponentInChildren<ParaFrustum>();
        paraScript.posTolerance = posTolerance;
        paraScript.angleTolerance = angTolerance;
        paraScript.setupParaFrustumFromParameters();
        //B. Set the orientation
        float xAngle=0, yAngle=0;

        switch (parafrustumPosition) {
            case 0:
                xAngle = rotationAngleParafrustum; yAngle = -rotationAngleParafrustum/2;
                break;
            case 1:
                xAngle = rotationAngleParafrustum; yAngle = rotationAngleParafrustum / 2;
                break;
            case 2:
                xAngle = 0; yAngle = -rotationAngleParafrustum;
                break;
            case 3:
                xAngle = 0; yAngle = rotationAngleParafrustum;
                break;
            case 4:
                xAngle = -rotationAngleParafrustum; yAngle = -rotationAngleParafrustum / 2;
                break;
            case 5:
                xAngle = -rotationAngleParafrustum; yAngle = rotationAngleParafrustum / 2;
                break;
        }
        parafrustum.transform.localRotation=Quaternion.AngleAxis( 0, new Vector3(0, 1, 0));
        parafrustum.transform.Rotate(new Vector3(1, 0, 0), xAngle, Space.Self);
        parafrustum.transform.Rotate(new Vector3(0, 1, 0), yAngle, Space.World);
    }

    public void playEffect(int soundEffect) {
        switch (soundEffect) {
            case SoundEffects.POSITIVE_FEEDBACK:
                positive.Play();
                break;
            case SoundEffects.NEGATIVE_FEEDBACK:
                negative.Play();
                break;
            case SoundEffects.TICK_TACK_FEEDBACK:
                tickTack.Play();
                break;
            case SoundEffects.CROWD_FEEDBACK:
                crowd.Play();
                break;
            case SoundEffects.RELAX_FEEDBACK:
                relax.Play();
                break;
        }            
    }

    public void stopEffect(int soundEffect)
    {
        switch (soundEffect)
        {
            case SoundEffects.POSITIVE_FEEDBACK:
                positive.Stop();
                break;
            case SoundEffects.NEGATIVE_FEEDBACK:
                negative.Stop();
                break;
            case SoundEffects.TICK_TACK_FEEDBACK:
                tickTack.Stop();
                break;
            case SoundEffects.CROWD_FEEDBACK:
                crowd.Stop();
                break;
            case SoundEffects.RELAX_FEEDBACK:
                relax.Stop();
                break;
        }
    }
    //Shows the highlight volume and projector. Less efficient (REMOVE). 
    protected void setFlagHighlighted(GameObject flag, bool highlight) {
        Transform[] children=flag.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
            if (child.name == "RingProjector")
                child.gameObject.SetActive(highlight);
    }

    protected void _setupScene(float distanceToCentre)
    {
        NavigationControl nc=NavigationControl.instance();
        if(distanceToCentre<6)
            nc.setOuterRadius((distanceToCentre - 0.25f)/3);
        else
            nc.setOuterRadius((distanceToCentre - 0.25f) /4f);
        centralArea.SetActive(true);

        showFlag(0, true);
        moveFlag(0, new Vector3(0, 0, -distanceToCentre));
        highlightFlag(0, true);

        showFlag(1, true);
        moveFlag(1, new Vector3(0, 0, -distanceToCentre));
        highlightFlag(1, false);

        showFlag(2, true);
        moveFlag(2, new Vector3(0, 0, -distanceToCentre));
        highlightFlag(2, true);

        showFlag(3, true);
        moveFlag(3, new Vector3(0, 0, -distanceToCentre));
        highlightFlag(3, false);

        showFlag(4, true);
        moveFlag(4, new Vector3(0, 0, -distanceToCentre));
        highlightFlag(4, true);

        showFlag(5, true);
        moveFlag(5, new Vector3(0, 0, -distanceToCentre));
        highlightFlag(5, false);



    }

    protected void _setupNaturalScene() {
        float distanceToCentre = -2;
        centralArea.SetActive(true);

        showFlag(0, true);
        moveFlag(0, new Vector3(0, 0, distanceToCentre));
        highlightFlag(0, true);

        showFlag(1, true);
        moveFlag(1, new Vector3(0, 0, distanceToCentre));
        highlightFlag(1, false);

        showFlag(2, true);
        moveFlag(2, new Vector3(0, 0, distanceToCentre));
        highlightFlag(2, true);

        showFlag(3, true);
        moveFlag(3, new Vector3(0, 0, distanceToCentre));
        highlightFlag(3, false);

        showFlag(4, true);
        moveFlag(4, new Vector3(0, 0, distanceToCentre));
        highlightFlag(4, true);

        showFlag(5, true);
        moveFlag(5, new Vector3(0, 0, distanceToCentre));
        highlightFlag(5, false);

        showParafrustum(false);

    }
}

using System.Collections;
using System.Collections.Generic;
using Assets.Created_Assets.Diego.Script.TaskManager;
using Assets.Created_Assets.Diego.Script;
using UnityEngine;
using VRTK;

//Helper class, returns the actual scaling value to use for each M and technique.
public class ScalingValuesPerTechnique
{
    public static bool springScalingActivate;
    public static int springNavigationScale;
    public static float homogeneousScalingValue(M_FACTOR m)
    {
        switch (m) //I've hijacked homogenous scaling to be at 1 at all times so I can apply my spring based scaling instead. This is no longer homogenous scaling this is my spring based scale-adaptive navigation
        {
            case M_FACTOR.M_NONE:
                return 1;
            case M_FACTOR.M_2:
                springNavigationScale = 2;
                return 1;
            case M_FACTOR.M_4:
                springNavigationScale = 4;
                return 1;
            case M_FACTOR.M_8:
                springNavigationScale = 8;
                return 1;
        }
        return 0;
    }
    public static float naviFieldScalingValue(M_FACTOR m)
    {
        switch (m)
        {
            case M_FACTOR.M_NONE:
                return 1;
            case M_FACTOR.M_2:
                return 2.185f * 2; 
            case M_FACTOR.M_4:
                return  4.586f * 2;
            case M_FACTOR.M_8:
                return  9.167f * 2;
        }
        return 0;
    }
    public static float getScalingValue(NavigationTechnique t, M_FACTOR m)
    {
        switch (t)
        {
            case NavigationTechnique.NAVIFIELD:
                springScalingActivate = false;
                Debug.Log("Navigation: Not spring (Navifields/no scaling)");
                return naviFieldScalingValue(m);
            case NavigationTechnique.HOMOGENEOUS:
                if (m != M_FACTOR.M_NONE)
                {
                    springScalingActivate = true; 
                    Debug.Log("Navigation: Spring");

                }
                return homogeneousScalingValue(m);
        }
        return 0;
    }

    //public static int hijackedSpringNavigationValue(NavigationTechnique t, M_FACTOR m) //feeds the correct value to 
    //{
    //    if (t == NavigationTechnique.HOMOGENEOUS)
    //    {
    //        switch (m)
    //        {
    //            case M_FACTOR.M_NONE:
    //                return 1;
    //            case M_FACTOR.M_2:
    //                return 2;
    //            case M_FACTOR.M_4:
    //                return 4;
    //            case M_FACTOR.M_8:
    //                return 8;
    //        }
    //    }
    //    return 1;
    //}

}


public class NavigationControl : MonoBehaviour {
    // Number of points in each dimension of the grid
    [Header("WARNING: Turn off grid visualization during use")]
    [Header("It takes a ridiculous amount of performance in this unity version")]
    public bool showOriginalGrid = false;
    public bool showX2Grid = false;
    public bool showX4Grid = false;
    public bool showX8Grid = false;
    public bool showProgress = false;
    public bool showResult = false;
    [Header("More = more precise simulation")]
    public int numPointsX = 50;
    public int numPointsY = 50;
    private float firstX = -3.5f;
    private float firstY = -3.5f;
    private float secondX = 3.5f;
    private float secondY = 3.5f;
    private float firstX4 = -6f;
    private float firstY4 = -6f;
    private float secondX4 = 6f;
    private float secondY4 = 6f;
    private float firstX8 = -13.5f;
    private float firstY8 = -13.5f;
    private float secondX8 = 13.5f;
    private float secondY8 = 13.5f;
    [Header("tweak details of M=2 spring simulation")]
    public float minK = 0.2f;
    public float maxK = 4f;
    public float fieldR = 0.8f;
    public float innerFieldR = 0.05f;
    public float intensity = 8.0f;
    [Header("tweak details of M=4 spring simulation")]
    public float minK4 = 0.2f;
    public float maxK4 = 4f;
    public float fieldR4 = 0.8f;
    public float innerFieldR4 = 0.05f;
    public float intensity4 = 8.0f;
    [Header("tweak details of M=8 spring simulation")]
    public float minK8 = 0.2f;
    public float maxK8 = 4f;
    public float fieldR8 = 0.8f;
    public float innerFieldR8 = 0.05f;
    public float intensity8 = 8.0f;
    private int numLinesX;
    private int numLinesY;
    [Header("keep at low value")]
    public float restLength = 0.0002f;
    public static int frameCount = 0;
    [Header("How many frames to simulate before stopping")]
    public int framesToSimulate = 100;


    // Variables for the playable space grid and scaling
    //public XRNode HeadPos;
    public Transform HeadSet; //Assign Main Camera
    public Transform RigPos; //Assign XRRig
    public float playableFirstX = -6.0f;
    public float playableFirstY = -6.0f;
    public float playableSecondX = 6.0f;
    public float playableSecondY = 6.0f;

    // Spacing between points
    private float pointSpacingX;
    private float pointSpacingY;
    private float pointSpacingX4;
    private float pointSpacingY4;
    private float pointSpacingX8;
    private float pointSpacingY8;

    // Prefab for the point game objects
    public GameObject pointPrefab;

    // Array to store the points
    GameObject[,] points;
    public Vector3[,] pointsVector;
    GameObject[,] points4;
    public Vector3[,] pointsVector4;
    GameObject[,] points8;
    public Vector3[,] pointsVector8;
    public Vector3[,] tempPointsVector;
    private bool hasConvertedPoints = false;
    float[,] linesHorizontalK;
    float[,] linesVerticalK;
    float[,] linesHorizontalK4;
    float[,] linesVerticalK4;
    float[,] linesHorizontalK8;
    float[,] linesVerticalK8;


    [Header("Introduce User number for experiment:")]
    public int UserId = 0;
    [Header("Visualize Rig displacement")]
    public float x = 0;
    public float y = 0;
    public float z = 0;
    public float k = 0;
    [Header("Demo Settings")]
    public bool Demo = false;
    public string M_Factor = "M values: 1, 2, 4, 8";
    public int M_2_use = 1;
    public string Tech_Factor = "Tech values: 0(NaviFields), 1(Homogeneous)";
    public int technique_2_use = 0;
    [Header("Controllers")]
    public GameObject leftController, rightController;
    public GameObject initialisingText;
    public GameObject toolTip;
    //Control of the position of the headset
    protected Transform headsetTransform; //Transform component (to get local/world coords, get rotations, etc...)
    protected Vector3 lastHeadPosInLocal; //Local positions of the headset
    protected Vector3 curHeadPosInLocal;
    protected Vector3 lastHeadPosInWorld; //Local positions of the headset in World
    protected Vector3 curHeadPosInWorld;
    protected AreaOfInterest[] AOIlist;
    //Control of the position of the playArea rig (adaptive navigation basically moves the playArea around...)
    protected Transform playAreaTransform;
    //Interaction listeners for events in the environment
    List<InteractionListener> listeners;
    
    // Use this for initialization
    void Start () {
        headsetTransform = GameObject.FindObjectOfType<SteamVR_Camera>().GetComponent<Transform>();
        curHeadPosInLocal = headsetTransform.localPosition; curHeadPosInLocal.y = 0;

        playAreaTransform = GameObject.FindObjectOfType<SteamVR_PlayArea>().gameObject.transform;
        AOIlist = GameObject.FindObjectsOfType<AreaOfInterest>();
        //Matrix4x4 headsetToCamera=new Matrix4x4();
        //headsetToCamera.SetTRS(headsetTransform.position, headsetTransform.rotation, headsetTransform.localScale);
        //GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
        //GetComponent<VRTK_ControllerEvents>().TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);
        listeners = new List<InteractionListener>();

        // Initialize the points arrays
        points = new GameObject[numPointsX, numPointsY];
        pointsVector = new Vector3[numPointsX, numPointsY];
        linesHorizontalK = new float[numPointsX, numPointsY];
        linesVerticalK = new float[numPointsX, numPointsY];
        points4 = new GameObject[numPointsX, numPointsY];
        pointsVector4 = new Vector3[numPointsX, numPointsY];
        linesHorizontalK4 = new float[numPointsX, numPointsY];
        linesVerticalK4 = new float[numPointsX, numPointsY];
        points8 = new GameObject[numPointsX, numPointsY];
        pointsVector8 = new Vector3[numPointsX, numPointsY];
        linesHorizontalK8 = new float[numPointsX, numPointsY];
        linesVerticalK8 = new float[numPointsX, numPointsY];

        pointSpacingX = (secondX - firstX) / (numPointsX - 1);
        pointSpacingY = (secondY - firstY) / (numPointsY - 1);
        pointSpacingX4 = (secondX4 - firstX4) / (numPointsX - 1);
        pointSpacingY4 = (secondY4 - firstY4) / (numPointsY - 1);
        pointSpacingX8 = (secondX8 - firstX8) / (numPointsX - 1);
        pointSpacingY8 = (secondY8 - firstY8) / (numPointsY - 1);

        numLinesX = numPointsX - 1;
        numLinesY = numPointsY - 1;

        //Draws a singular yellow square to represent what a 1x scale square looks like
        Debug.DrawLine(new Vector3(4f, 0.01f, 0f), new Vector3(4f + (playableSecondX - playableFirstX) / (numPointsX - 1), 0.01f, 0f), Color.yellow, 500f);
        Debug.DrawLine(new Vector3(4f, 0.01f, 0f), new Vector3(4f, 0.01f, (playableSecondY - playableFirstY) / (numPointsY - 1)), Color.yellow, 500f);
        Debug.DrawLine(new Vector3(4f + (playableSecondX - playableFirstX) / (numPointsX - 1), 0.01f, (playableSecondY - playableFirstY) / (numPointsY - 1)), new Vector3(4f, 0.01f, (playableSecondY - playableFirstY) / (numPointsY - 1)), Color.yellow, 500f);
        Debug.DrawLine(new Vector3(4f + (playableSecondX - playableFirstX) / (numPointsX - 1), 0.01f, (playableSecondY - playableFirstY) / (numPointsY - 1)), new Vector3(4f + (playableSecondX - playableFirstX) / (numPointsX - 1), 0.01f, 0f), Color.yellow, 500f);

        // Create the point game objects and add them to the scene
        for (int y = 0; y < numPointsY; y++)
        {
            for (int x = 0; x < numPointsX; x++)
            {
                // Calculate the position of the point
                Vector3 pointPosition = new Vector3(firstX + (x * pointSpacingX), 0.0f, firstY + (y * pointSpacingY));
                Vector3 pointPosition4 = new Vector3(firstX4 + (x * pointSpacingX4), 0.0f, firstY4 + (y * pointSpacingY4));
                Vector3 pointPosition8 = new Vector3(firstX8 + (x * pointSpacingX8), 0.0f, firstY8 + (y * pointSpacingY8));


                // Create the point game object and add it to the scene
                GameObject point = Instantiate(pointPrefab, pointPosition, Quaternion.identity);
                point.name = "Point " + x + "," + y;
                point.transform.parent = transform;
                GameObject point4 = Instantiate(pointPrefab, pointPosition4, Quaternion.identity);
                point4.name = "Point4 " + x + "," + y;
                point4.transform.parent = transform;
                GameObject point8 = Instantiate(pointPrefab, pointPosition8, Quaternion.identity);
                point8.name = "Point8 " + x + "," + y;
                point8.transform.parent = transform;

                // Add the point to the points array
                points[x, y] = point;
                points4[x, y] = point4;
                points8[x, y] = point8;

            }
        }

        for (int y = 0; y < numLinesX; y++)
        {
            for (int x = 0; x < numPointsY; x++) //Determine k value for all Horizontal lines
            {
                Vector3 lineCentre = new Vector3(firstX + (pointSpacingX / 2 + pointSpacingX * x), 0, firstY + y * pointSpacingY);
                linesHorizontalK[x, y] = kFromXY(lineCentre, 2f);
                Vector3 lineCentre4 = new Vector3(firstX4 + (pointSpacingX4 / 2 + pointSpacingX4 * x), 0, firstY4 + y * pointSpacingY4);
                linesHorizontalK4[x, y] = kFromXY(lineCentre4, 4f);
                Vector3 lineCentre8 = new Vector3(firstX8 + (pointSpacingX8 / 2 + pointSpacingX8 * x), 0, firstY8 + y * pointSpacingY8);
                linesHorizontalK8[x, y] = kFromXY(lineCentre8, 8f);
                //Debug.Log(linesHorizontalK[x,y]);
            }
        }

        for (int y = 0; y < numLinesY; y++)
        {
            for (int x = 0; x < numPointsX; x++) //Determine k value for all Vertical lines
            {
                Vector3 lineCentre = new Vector3(firstX + x * pointSpacingX, 0, firstY + (pointSpacingY / 2 + pointSpacingY * y));
                linesVerticalK[x, y] = kFromXY(lineCentre, 2f);
                Vector3 lineCentre4 = new Vector3(firstX4 + x * pointSpacingX4, 0, firstY4 + (pointSpacingY4 / 2 + pointSpacingY4 * y));
                linesVerticalK4[x, y] = kFromXY(lineCentre4, 4f);
                Vector3 lineCentre8 = new Vector3(firstX8 + x * pointSpacingX8, 0, firstY8 + (pointSpacingY8 / 2 + pointSpacingY8 * y));
                linesVerticalK8[x, y] = kFromXY(lineCentre8, 8f);
            }
        }

        if (showOriginalGrid)
        {
            for (int y = 0; y < (numPointsY); y++) //COLORED DebugLines for ORIGINAL GRID
            {
                for (int x = 0; x < (numPointsX); x++) //draws the lines between each dot to show where the springs are
                {
                    if (x < (numPointsX - 1))
                    {
                        Debug.DrawLine(points[x, y].transform.position / ((firstX - secondX) / (playableFirstX - playableSecondX)) + new Vector3(0f, 0.01f, 0f), points[x + 1, y].transform.position / ((firstX - secondX) / (playableFirstX - playableSecondX)) + new Vector3(0f, 0.01f, 0f), Color.red, 500f);
                    }
                    if (y < (numPointsY - 1))
                    {
                        Debug.DrawLine(points[x, y].transform.position / ((firstY - secondY) / (playableFirstY - playableSecondY)) + new Vector3(0f, 0.01f, 0f), points[x, y + 1].transform.position / ((firstY - secondY) / (playableFirstY - playableSecondY)) + new Vector3(0f, 0.01f, 0f), Color.red, 500f);
                    }
                }
            }
        }

    }

    private float kFromXY(Vector3 lineCentre, float n)
    {
        //var fields = GameObject.FindGameObjectsWithTag("navifields");
        //float m = 4f;
        Vector3[] p4 = new Vector3[6];

        float m = n - 0.1f;

        p4[0] = new Vector3(m, 0, 0);
        p4[1] = new Vector3(m/2, 0, Mathf.Sqrt(3) * m / 2);
        p4[2] = new Vector3(-m/2, 0, Mathf.Sqrt(3) * m / 2);
        p4[3] = new Vector3(-m/2, 0, -Mathf.Sqrt(3) * m / 2);
        p4[4] = new Vector3(m/2, 0, -Mathf.Sqrt(3) * m / 2);
        p4[5] = new Vector3(-m, 0, 0);

        float tempIFR = 0;
        float tempFR = 0;
        float tempMaxK = 0;
        float k = 0;

        if (n == 2.0f)
        {
            tempIFR = innerFieldR;
            tempFR = fieldR;
            k = minK;
            tempMaxK = maxK;
        }
        if (n == 4.0f)
        {
            tempIFR = innerFieldR4;
            tempFR = fieldR4;
            k = minK4;
            tempMaxK = maxK4;
        }
        if (n == 8.0f)
        {
            tempIFR = innerFieldR8;
            tempFR = fieldR8;
            k = minK8;
            tempMaxK = maxK8;
        }

        float tempMinK = k;

        for (int i = 0; i < p4.Length; i++)
        {
            float DistX = lineCentre.x - p4[i].z;
            float DistZ = lineCentre.z - p4[i].x;
            float Dist = Mathf.Sqrt(DistX * DistX + DistZ * DistZ);
            float InnerR = tempIFR * 1.7f;
            float OuterR = tempFR * 1.7f;
            float relativeDist = OuterR - InnerR;
            if (Dist >= OuterR)
            {

            }
            else if (Dist <= InnerR)
            {
                k = maxK;

            }
            else if (Dist < OuterR && Dist > InnerR)
            { //can be used to gradually decrease k value when leaving field
                //float gradualK = (((OuterR - InnerR) - (Dist - InnerR)) / (OuterR - InnerR)) * (tempMaxK - 0.5f) + 0.5f;
                float gradualK = ((relativeDist - (Dist - InnerR)) / relativeDist) * tempMaxK + tempMinK;
                if (gradualK > k)
                {
                    k = gradualK;
                    // Debug.Log(k);
                }
                // k = maxK;
            }
        }
        return k;
    }

    public Vector3 VectorForce(Vector3 original, Vector3 compared, float k, float tempIntensity) //Hooke's law is applied, the intensity variable serves a similar function as changing mass, but inversed
    {
        Vector3 difference = original - compared;
        Vector3 vecDirection = difference.normalized;
        float mag = difference.magnitude;
        float x = mag - restLength;
        float force = -k * x * tempIntensity;
        Vector3 applyForce = force * vecDirection;
        return applyForce;
    }

    private void _compute_M_eq(float M, float targetToAreaDist) {
        //Make sure flags are at correct distance (-M)

        float D_half = M+targetToAreaDist;
        float totalK = 0;
        int numPoints = 0;

        for (float x = -D_half; x <= D_half; x += 0.05f)
            for (float z = -D_half; z <= D_half; z += 0.05f)
                if (x * x + z * z < D_half * D_half)
                {
                    //selectCurrentAOI(new Vector3(x, 0, z)))
                    totalK += this.selectCurrentAOI(new Vector3(x, 0, z)).getKValue(new Vector3(x, 0, z));
                    numPoints++;
                }
        float K_eq = (totalK / numPoints);
        float M_ew = 1 + ((M - 1) / K_eq);
        Debug.Log("M_eq=" + (totalK / numPoints));

    }

    // Update is called once per frame
    protected bool registeredVRTKListeners = false;
    static bool _experimentReady = false;
    void Update () {


        if (frameCount < framesToSimulate)
        {
            frameCount += 1;

            for (int y = 1; y < (numPointsY - 1); y++)
            {
                for (int x = 1; x < (numPointsX - 1); x++) //Runs for every point that isn't at the edge
                {
                    //points[x,y].transform.position += new Vector3(0f,0.01f,0f);
                    Vector3 self = points[x, y].transform.position;
                    Vector3 self4 = points4[x, y].transform.position;
                    Vector3 self8 = points8[x, y].transform.position;
                    Vector3 up = points[x, y + 1].transform.position;
                    Vector3 right = points[x + 1, y].transform.position;
                    Vector3 down = points[x, y - 1].transform.position;
                    Vector3 left = points[x - 1, y].transform.position;
                    Vector3 up4 = points4[x, y + 1].transform.position;
                    Vector3 right4 = points4[x + 1, y].transform.position;
                    Vector3 down4 = points4[x, y - 1].transform.position;
                    Vector3 left4 = points4[x - 1, y].transform.position;
                    Vector3 up8 = points8[x, y + 1].transform.position;
                    Vector3 right8 = points8[x + 1, y].transform.position;
                    Vector3 down8 = points8[x, y - 1].transform.position;
                    Vector3 left8 = points8[x - 1, y].transform.position;
                    float ksc = 0.01f;
                    float ksc4 = 0.01f;
                    float ksc8 = 0.01f;
                    Vector3[] dirSum = { up, right, down, left };
                    Vector3[] dirSum4 = { up4, right4, down4, left4 };
                    Vector3[] dirSum8 = { up8, right8, down8, left8 };
                    Vector3 totalForce = new Vector3(0.0f, 0.0f, 0.0f);
                    Vector3 totalForce4 = new Vector3(0.0f, 0.0f, 0.0f);
                    Vector3 totalForce8 = new Vector3(0.0f, 0.0f, 0.0f);

                    foreach (Vector3 dir in dirSum)
                    {
                        if (dir == up)
                        {
                            ksc = linesVerticalK[x, y];
                        }
                        else if (dir == right)
                        {
                            ksc = linesHorizontalK[x, y];
                        }
                        else if (dir == down)
                        {
                            ksc = linesVerticalK[x, y - 1];
                        }
                        else if (dir == left)
                        {
                            ksc = linesHorizontalK[x - 1, y];
                        }
                        totalForce += VectorForce(self, dir, ksc, intensity);
                    }
                    foreach (Vector3 dir4 in dirSum4)
                    {
                        if (dir4 == up4)
                        {
                            ksc4 = linesVerticalK4[x, y];
                        }
                        else if (dir4 == right4)
                        {
                            ksc4 = linesHorizontalK4[x, y];
                        }
                        else if (dir4 == down4)
                        {
                            ksc4 = linesVerticalK4[x, y - 1];
                        }
                        else if (dir4 == left4)
                        {
                            ksc4 = linesHorizontalK4[x - 1, y];
                        }
                        totalForce4 += VectorForce(self4, dir4, ksc4, intensity4);
                    }
                    foreach (Vector3 dir8 in dirSum8)
                    {
                        if (dir8 == up8)
                        {
                            ksc8 = linesVerticalK8[x, y];
                        }
                        else if (dir8 == right8)
                        {
                            ksc8 = linesHorizontalK8[x, y];
                        }
                        else if (dir8 == down8)
                        {
                            ksc8 = linesVerticalK8[x, y - 1];
                        }
                        else if (dir8 == left8)
                        {
                            ksc8 = linesHorizontalK8[x - 1, y];
                        }
                        totalForce8 += VectorForce(self8, dir8, ksc8, intensity8);
                    }
                    // Debug.Log(totalForce);
                    points[x, y].transform.position += totalForce;
                    points4[x, y].transform.position += totalForce4;
                    points8[x, y].transform.position += totalForce8;

                }
            }
            if(showX2Grid && showProgress)
            {
                for (int y = 0; y < (numPointsY); y++) //DebugLines for SPRING SIMULATION
                {
                    for (int x = 0; x < (numPointsX); x++) //draws the lines between each dot to show where the springs are
                    {
                        if (x < (numPointsX - 1))
                        {
                            Debug.DrawLine(points[x, y].transform.position, points[x + 1, y].transform.position, Color.white, 0f);
                        }
                        if (y < (numPointsY - 1))
                        {
                            Debug.DrawLine(points[x, y].transform.position, points[x, y + 1].transform.position, Color.white, 0f);
                        }
                        if ((y < (numPointsY - 1)) && (x < (numPointsX - 1)))
                        {
                            Debug.DrawLine(points[x, y].transform.position, points[x + 1, y + 1].transform.position, new Vector4(0.1f, 0.1f, 0.1f, 1), 0f);
                        }
                    }
                }
            }
            if (showX4Grid && showProgress)
            {
                for (int y = 0; y < (numPointsY); y++) //DebugLines for SPRING SIMULATION
                {
                    for (int x = 0; x < (numPointsX); x++) //draws the lines between each dot to show where the springs are
                    {
                        if (x < (numPointsX - 1))
                        {
                            Debug.DrawLine(points4[x, y].transform.position, points4[x + 1, y].transform.position, Color.white, 0f);
                        }
                        if (y < (numPointsY - 1))
                        {
                            Debug.DrawLine(points4[x, y].transform.position, points4[x, y + 1].transform.position, Color.white, 0f);
                        }
                        if ((y < (numPointsY - 1)) && (x < (numPointsX - 1)))
                        {
                            Debug.DrawLine(points4[x, y].transform.position, points4[x + 1, y + 1].transform.position, new Vector4(0.1f, 0.1f, 0.1f, 1), 0f);
                        }
                    }
                }
            }
            if (showX8Grid && showProgress)
            {
                for (int y = 0; y < (numPointsY); y++) //DebugLines for SPRING SIMULATION
                {
                    for (int x = 0; x < (numPointsX); x++) //draws the lines between each dot to show where the springs are
                    {
                        if (x < (numPointsX - 1))
                        {
                            Debug.DrawLine(points8[x, y].transform.position, points8[x + 1, y].transform.position, Color.white, 0f);
                        }
                        if (y < (numPointsY - 1))
                        {
                            Debug.DrawLine(points8[x, y].transform.position, points8[x, y + 1].transform.position, Color.white, 0f);
                        }
                        if ((y < (numPointsY - 1)) && (x < (numPointsX - 1)))
                        {
                            Debug.DrawLine(points8[x, y].transform.position, points8[x + 1, y + 1].transform.position, new Vector4(0.1f, 0.1f, 0.1f, 1), 0f);
                        }
                    }
                }
            }
        }

        if (frameCount == framesToSimulate && hasConvertedPoints == false) //Converts the vectors of each point from gameobject to just vector form to save performance.
        {
            for (int y = 0; y < numPointsY; y++)
            {
                for (int x = 0; x < numPointsX; x++)
                {
                    pointsVector[x, y] = points[x, y].transform.position;
                    Destroy(points[x, y]);
                    pointsVector4[x, y] = points4[x, y].transform.position;
                    Destroy(points4[x, y]);
                    pointsVector8[x, y] = points8[x, y].transform.position;
                    Destroy(points8[x, y]);
                }
            }
            if (showX2Grid && showResult)
            {
                for (int y = 0; y < (numPointsY); y++) //DebugLines for SPRING SIMULATION
                {
                    for (int x = 0; x < (numPointsX); x++) //draws the lines between each dot to show where the springs are
                    {
                        if (x < (numPointsX - 1))
                        {
                            Debug.DrawLine(pointsVector[x, y], pointsVector[x + 1, y], Color.white, 500f);
                        }
                        if (y < (numPointsY - 1))
                        {
                            Debug.DrawLine(pointsVector[x, y], pointsVector[x, y + 1], Color.white, 500f);
                        }
                        if ((y < (numPointsY - 1)) && (x < (numPointsX - 1)))
                        {
                            Debug.DrawLine(pointsVector[x, y], pointsVector[x + 1, y + 1], new Vector4(0.1f, 0.1f, 0.1f, 1), 500f);
                        }
                    }
                }
            }
            if (showX4Grid && showResult)
            {
                for (int y = 0; y < (numPointsY); y++) //DebugLines for SPRING SIMULATION
                {
                    for (int x = 0; x < (numPointsX); x++) //draws the lines between each dot to show where the springs are
                    {
                        if (x < (numPointsX - 1))
                        {
                            Debug.DrawLine(pointsVector4[x, y], pointsVector4[x + 1, y], Color.white, 500f);
                        }
                        if (y < (numPointsY - 1))
                        {
                            Debug.DrawLine(pointsVector4[x, y], pointsVector4[x, y + 1], Color.white, 500f);
                        }
                        if ((y < (numPointsY - 1)) && (x < (numPointsX - 1)))
                        {
                            Debug.DrawLine(pointsVector4[x, y], pointsVector4[x + 1, y + 1], new Vector4(0.1f, 0.1f, 0.1f, 1), 500f);
                        }
                    }
                }
            }
            if (showX8Grid && showResult)
            {
                for (int y = 0; y < (numPointsY); y++) //DebugLines for SPRING SIMULATION
                {
                    for (int x = 0; x < (numPointsX); x++) //draws the lines between each dot to show where the springs are
                    {
                        if (x < (numPointsX - 1))
                        {
                            Debug.DrawLine(pointsVector8[x, y], pointsVector8[x + 1, y], Color.white, 500f);
                        }
                        if (y < (numPointsY - 1))
                        {
                            Debug.DrawLine(pointsVector8[x, y], pointsVector8[x, y + 1], Color.white, 500f);
                        }
                        if ((y < (numPointsY - 1)) && (x < (numPointsX - 1)))
                        {
                            Debug.DrawLine(pointsVector8[x, y], pointsVector8[x + 1, y + 1], new Vector4(0.1f, 0.1f, 0.1f, 1), 500f);
                        }
                    }
                }
            }

            hasConvertedPoints = true;
            Debug.Log("Points have been converted");
            initialisingText.SetActive(false);
            toolTip.SetActive(true);
        }

        if (frameCount == framesToSimulate && ScalingValuesPerTechnique.springScalingActivate == true)
        {
            //Scaling variables:

            float hX = HeadSet.position.x - RigPos.transform.position.x;
            float hY = RigPos.transform.position.y;
            float hZ = HeadSet.position.z - RigPos.transform.position.z;

            float playableSpacingX = (playableSecondX - playableFirstX) / (numPointsX - 1);
            float playableSpacingY = (playableSecondY - playableFirstY) / (numPointsY - 1);



            switch (ScalingValuesPerTechnique.springNavigationScale)
            {
                case 2:
                    tempPointsVector = pointsVector;
                    break;
                case 4:
                    tempPointsVector = pointsVector4;
                    break;
                case 8:
                    tempPointsVector = pointsVector8;
                    break;
            }

            //Calculate index for position of headset
            if (hX > playableFirstX && hX < playableSecondX && hZ > playableFirstY && hZ < playableSecondY)
            {
                float relativeX = (hX - playableFirstX) / playableSpacingX;
                int indexX = (int)Mathf.Round(relativeX - 0.5f);
                //Debug.Log(indexX);
                float relativeY = (hZ - playableFirstY) / playableSpacingY;
                int indexY = (int)Mathf.Round(relativeY - 0.5f);

                bool whichTriangle = false; // splitting the square into two triangles to calculate position translation.
                if ((relativeX - indexX) > (relativeY - indexY))
                {
                    whichTriangle = true;
                }
                else
                {
                    whichTriangle = false;
                }
                //Debug.Log(indexX + " ___ " + indexY + " " + whichTriangle);

                //Matrix equation used: https://stackoverflow.com/questions/18844000/transfer-coordinates-from-one-triangle-to-another-triangle

                //    | xa1 xa2 xa3 |
                // A =| ya1 ya2 ya3 |
                //    |  1   1   1  |

                //    | xb1 xb2 xb3 |
                // B =| yb1 yb2 yb3 |
                //    |  1   1   1  |

                //     M is the transformation matrix
                //     M * A = B
                //     M * A * Inv(A) = B * Inv(A)
                //     M = B * Inv(A)

                // Multiply M by the column matrix of the point to get 

                float xa1 = playableFirstX + (playableSpacingX * indexX);
                float xa2;
                float xa3 = playableFirstX + (playableSpacingX * (indexX + 1));

                float ya1 = playableFirstY + (playableSpacingY * indexY);
                float ya2;
                float ya3 = playableFirstY + (playableSpacingY * (indexY + 1));

                float xb1 = tempPointsVector[indexX, indexY].x;
                float xb2;
                float xb3 = tempPointsVector[(indexX + 1), (indexY + 1)].x;

                float yb1 = tempPointsVector[indexX, indexY].z;
                float yb2;
                float yb3 = tempPointsVector[(indexX + 1), (indexY + 1)].z;

                if (whichTriangle == false)
                {
                    xa2 = playableFirstX + (playableSpacingX * indexX);
                    ya2 = playableFirstY + (playableSpacingY * (indexY + 1));
                    xb2 = tempPointsVector[indexX, (indexY + 1)].x;
                    yb2 = tempPointsVector[indexX, (indexY + 1)].z;

                }
                else
                {
                    xa2 = playableFirstX + (playableSpacingX * (indexX + 1));
                    ya2 = playableFirstY + (playableSpacingY * indexY);
                    xb2 = tempPointsVector[(indexX + 1), indexY].x;
                    yb2 = tempPointsVector[(indexX + 1), indexY].z;

                }

                Matrix4x4 matrixA = new Matrix4x4();
                Matrix4x4 matrixB = new Matrix4x4();

                matrixA.SetRow(0, new Vector4(xa1, xa2, xa3, 0));
                matrixA.SetRow(1, new Vector4(ya1, ya2, ya3, 0));
                matrixA.SetRow(2, new Vector4(1, 1, 1, 0));
                matrixA.SetRow(3, new Vector4(0, 0, 0, 1));
                //Debug.Log(matrixA);

                matrixB.SetRow(0, new Vector4(xb1, xb2, xb3, 0));
                matrixB.SetRow(1, new Vector4(yb1, yb2, yb3, 0));
                matrixB.SetRow(2, new Vector4(1, 1, 1, 0));
                matrixB.SetRow(3, new Vector4(0, 0, 0, 1));

                Matrix4x4 matrixInverseA = matrixA.inverse;
                Matrix4x4 matrixM = matrixB * matrixInverseA;

                Vector4 sourcePosVector = new Vector4(hX, hZ, 1, 0);
                Vector4 NewPosVector = matrixM * sourcePosVector;
                //Debug.Log(NewPosVector);

                RigPos.transform.position = new Vector4(NewPosVector.x - hX, hY, NewPosVector.y - hZ);

                //Debug.Log(new Vector4(NewPosVector.x - hX, hY, NewPosVector.y - hZ));

            }




        }




        if (!_experimentReady)
        {
            TaskManager.instance().setupExperiment(UserId);
            _experimentReady = true;
        }
        if (!registeredVRTKListeners)
        {
            VRTK_ControllerEvents ce = GameObject.FindObjectOfType<VRTK_ControllerEvents>();
            if(ce)  ce.AliasGrabOn += new ControllerInteractionEventHandler(DoTriggerPressed);
            if(ce)  ce.AliasGrabOff += new ControllerInteractionEventHandler(DoTriggerReleased);
            registeredVRTKListeners = (ce!=null);
        }
        //A. Update current and last position (constrained to floor). Compute displacement
        lastHeadPosInLocal = curHeadPosInLocal;
        curHeadPosInLocal = headsetTransform.localPosition; 
        curHeadPosInLocal.y = 0;

        lastHeadPosInWorld = curHeadPosInWorld;
        curHeadPosInWorld = headsetTransform.position;
        curHeadPosInWorld.y = 0;

        //bookmark

        Vector3 displacement = curHeadPosInLocal - lastHeadPosInLocal;
        Vector3 handPosition = getCurHandPosition();
        AreaOfInterest candidateAOI = selectCurrentAOI(headsetTransform.position);
        float k = candidateAOI.getKValue(headsetTransform.position);
        k = GetKMod2(lastHeadPosInLocal, curHeadPosInLocal, lastHeadPosInWorld, curHeadPosInWorld);
        float curTime = UnityEngine.Time.realtimeSinceStartup;
        if (ScalingValuesPerTechnique.springScalingActivate == false)
        {
            updateUserPositionDrift(k, displacement);
        }
        Vector3 driftVector = ComputeDrift(curHeadPosInWorld, curHeadPosInLocal);
        float driftMagnitude = driftVector.magnitude;
        Vector3 virtualCenter = playAreaTransform.position;
        virtualCenter.y = 0;
        float PGCMag = virtualCenter.magnitude;
        Vector3 headToVR = headsetTransform.position; //headToVR.y = 0;
        Vector3 headToTracking = headsetTransform.localPosition; //headToTracking.y = 0;
        TaskManager.instance().onHeadUpdate(headsetTransform.localPosition, displacement, headsetTransform.position, k * displacement, curTime, k, handPosition);
        //B. Select candidate AOI, based on current position IN WORLD

        //C. Update head position based on the selected AOI. 
        //EnvironmentManager.instance().centralText("Head " + curHeadPosInLocal + "\n VHead"+ headsetTransform.position);
    }

    public Vector3 getCurHandPosition() {
        if (this.leftController.activeSelf)
            return leftController.transform.position;
        else
            return rightController.transform.position;
    }

    //Singleton method: It retrieves the UNIQUE instance of this class in the system.
    public static NavigationControl instance() {
        return GameObject.FindObjectOfType<NavigationControl>();
    }

    public void setTechnique(NavigationTechnique technique) {
        //Start Demo
        //bool use_Navifield = false;

        //if (Demo)
        //{
        //    NavigationTechnique t = (technique_2_use == 0 ? NavigationTechnique.NAVIFIELD : NavigationTechnique.HOMOGENEOUS);

        //    //AOIlist.GetLength(0);
        //    foreach (AreaOfInterest aoi in AOIlist)
        //        aoi.NAVIFIELD_ENABLED = use_Navifield;
        //}
        //else
        //{
           bool use_Navifield = (technique == NavigationTechnique.NAVIFIELD);
            //AOIlist.GetLength(0);
            foreach (AreaOfInterest aoi in AOIlist)
                aoi.NAVIFIELD_ENABLED = use_Navifield;
        //}
        //End Demo


    }

    public void setMFactor(M_FACTOR m) {
        //Start Demo
        //if (Demo)
        //{
        //    NavigationTechnique t = (technique_2_use == 0 ? NavigationTechnique.NAVIFIELD : NavigationTechnique.HOMOGENEOUS);
        //    m = (M_FACTOR) M_2_use;
        //    //AOIlist.GetLength(0);
        //    foreach (AreaOfInterest aoi in AOIlist)
        //        aoi.maxMultiplier = ScalingValuesPerTechnique.getScalingValue(t, m);
        //}
        //else
        //{
            //bool use_Navifield = (technique == NavigationTechnique.NAVIFIELD);
            NavigationTechnique t = (AOIlist[0].NAVIFIELD_ENABLED ? NavigationTechnique.NAVIFIELD : NavigationTechnique.HOMOGENEOUS);

            foreach (AreaOfInterest aoi in AOIlist)
                aoi.maxMultiplier = ScalingValuesPerTechnique.getScalingValue(t,m);
        //}

        //End Demo


    }



    public void resetDrift() {
        playAreaTransform.position = new Vector3(0,0,0);
    }

    public void setOuterRadius(float max_r) {
        foreach (AreaOfInterest aoi in AOIlist)
            aoi.maxRadius = max_r;

    }
    AreaOfInterest selectCurrentAOI(Vector3 headPositionInWorld) {
        float minK = 100;
        AreaOfInterest candidateAOI = null;
        //Check all AOI and get the one with minimum K
        foreach (AreaOfInterest aoi in AOIlist)
        {
            float k = aoi.getKValue(headPositionInWorld);
            if (k < minK)
            {
                minK = k;
                candidateAOI = aoi;
            }
        }
        return candidateAOI;
    }

    void updateUserPosition(float k, Vector3 displacement) {
        Vector3 appliedDisplacement=(k-1)*displacement;
        playAreaTransform.position += appliedDisplacement;

        x = playAreaTransform.position.x;
        y = playAreaTransform.position.y;
        z = playAreaTransform.position.z;
        this.k = k;
    }

    private Vector3 ComputeDrift(Vector3 userVPos, Vector3 userRPos)
    {
        Vector3 oldUserPosition = userVPos;             //User Virtual position in simulation
        Vector3 newUserPosition = userVPos;
        Vector3 drift = userVPos - userRPos; ;          //Current drift. Initially set to VirtUserPos(t=now)- RealUserPos(t=now) => Equivalent to our "Gain" vector
        Vector3 endPosition = new Vector3(0, 0, 0);     //Target position --> We simulate steps in direction towards the center (0,0,0)
        float displacement = 0.0025f;                     //Assume the user will move 1cm per frame.
                                                          // 2.2 Variables to detect simulation end.
        float lastDist = userVPos.x * userVPos.x + userVPos.z * userVPos.z; //Squared distance
        float dist = lastDist;

        //3. Simulation Steps
        //3.1. Create a displacement vector of towards the centre (size=displacement variable)
        Vector3 delta_userPos = (endPosition - userVPos);   //Vector in direction to centre
        delta_userPos.Normalize();                              //Make it unitary
        delta_userPos *= displacement;                          //make it "displacement units" long

        //3.2. Apply that displacement to current position, untill we reach the centre 
        while (dist <= lastDist && dist >= 0.05f)//&& dist > 0)//while we are approaching (we will come to a point when we overshoot --> dist>lastDist).
        {
            //Apply a navigation step
            oldUserPosition = newUserPosition;
            float k = GetK_Naive(oldUserPosition);
            newUserPosition += k * delta_userPos;
            //Correct distances to detects arrival: 
            lastDist = dist;
            dist = Mathf.Abs(newUserPosition.x * newUserPosition.x + newUserPosition.z * newUserPosition.z);

            if (dist <= lastDist)
            {
                //compute increment to drift vector
                drift += (k - 1) * delta_userPos;
            }
        }
        return drift;
    }

    float GetK_Naive(Vector3 Pv)
    {
        AreaOfInterest candidateAOI = selectCurrentAOI(Pv);
        return candidateAOI.getKValue(Pv);
    }

    void updateUserPositionDrift(float k, Vector3 displacement)
    {
        Vector3 appliedDisplacement = (k - 1) * displacement;
        playAreaTransform.position += appliedDisplacement;

        x = playAreaTransform.position.x;
        y = playAreaTransform.position.y;
        z = playAreaTransform.position.z;
        this.k = k;
    }

    public void addListener(InteractionListener l) {
        listeners.Add(l);
    }

    public void removeListener(InteractionListener l) {
            listeners.Remove(l);
    }

    public void OnInteractableObjectUngrab(ObjectInteractEventArgs e) {
        Debug.Log("Current released Object: "+ e);

    }


    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (frameCount == framesToSimulate)
        {
            Debug.Log("TRIGGER pressed down" + e);
            TaskManager.instance().onTriggerPressed(0);
        }
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        Debug.Log("TRIGGER released" + e);
    }
    /// ////////////////////////// ////////////////////////// ////////////////////////// ///////////////////////

    public Vector4 SLB_ComputeDisplacement(Vector3 realPos, Vector3 prevRealPos)
    {
        /*  D = d_u*U + d_v*V */
        Vector3 U = Vector3.zero;
        float d_u = 0;

        U = SLB_GetDir_Gaze();
        /*if (dispDirecitonRW.Count <= 90) {			
			U = SLB_GetDir_Gaze();
		} else {
			U = SLB_GetDir_Avg();
		}*/

        d_u = SLB_GetScaledDisplament(realPos, prevRealPos);
        return new Vector4(U.x, U.y, U.z, d_u);
        // to update user pos use: SLB_UpdateDisplacement (U, d_u);
    }

    private float SLB_GetSpeed(float mag, float deltaTme = -1.0f)
    {
        if (deltaTme < 0)
            return mag / Time.deltaTime;
        else
            return mag / deltaTme;
    }

    // virtual world coordinates
    private void SLB_UpdateDisplacement(Vector3 dir, float mag)
    {
        //Vector3 dir = new Vector3(dirAndMag.x, dirAndMag.y, dirAndMag.z);
        //float mag = dirAndMag.w;
        Vector3 displacement = mag * dir;
        playAreaTransform.position += displacement;

        x = playAreaTransform.position.x;
        y = playAreaTransform.position.y;
        z = playAreaTransform.position.z;
        //this.k = mag;
    }

    private Vector4 SLB_GetDir_Avg()
    {
        Vector3 avgDir = SLB_GetDir_Gaze();
        return new Vector4(avgDir.x, avgDir.y, avgDir.z, 1);
        /*Vector3 avgDir = Vector3.zero;
		float avgMag = 0;

		// 2 second with 60fps = 120fps (considering a minimum of 60fps)
		for (int i = 0; i < 90; i++) {
			Vector3 aux = dispDirecitonRW [dispDirecitonRW.Count - i - 1];			 
			aux.y = 0;
			avgDir += aux;
			avgMag += dispMagnitudRW[dispMagnitudRW.Count - i - 1];	
		}

		avgDir = (avgDir / 90).normalized;
		avgMag = avgMag / 90;
		return new Vector4 (avgDir.x,avgDir.y, avgDir.z, avgMag);*/
    }

    private Vector3 SLB_GetDir_Gaze()
    {
        Vector3 gazeDir = headsetTransform.forward;
        gazeDir.y = 0;
        return gazeDir.normalized;
    }

    private float SLB_GetScaledDisplament(Vector3 realPos, Vector3 prevPos)
    {
        // average human walking speed 1.4 m/s

        float displacementReal = (realPos - prevPos).magnitude;
        float speed = SLB_GetSpeed(displacementReal);
        return SLB_GetScale(speed) * displacementReal;
    }

    private float SLB_GetScale(float curVelocity)
    {
        if (float.IsNaN(curVelocity))
            curVelocity = 0.001f;//  approx 0.28m/s
        /// compute the walking speed in a second considering 90fps
        //curVelocity *= 1/Time.deltaTime;
        // if we consider average human velocity as 1.4 m/s and about 20% of the range from 0 m/s to 1.4 m/s
        // to keep small movements with scale 1, and bigger displacemets will e scaled exponentially to the user velocity

        // In(8) = 2.07944154 (maxScale) , Vo = 0.28m/s  ,  maxVelocity  = 1.4m/s  , C = In(8)/(maxVelocity - Vo) , then e^(velocity - Vo)*C
        float scale = Mathf.Exp((curVelocity - 0.2f) * 1.62f); //2.9155f

        if (float.IsNaN(scale) || scale < 1 || float.IsNegativeInfinity(scale))
            scale = 1;
        else if (float.IsPositiveInfinity(scale) || scale > 7)
            scale = 7;
        return scale;
    }

    float epsilon = 0.25f;
    float alpha = 0.25f;

    float SLB_GetKMod2(Vector3 realPosOld, Vector3 realPosCur, Vector3 virtPosOld, Vector3 virtPosCur, float scalingF, float curVelocity)// Vector3 userRPos,
    {
        //1. Get uncorrected K
        float K_nf = scalingF;

        //2. Get drift associated with (potentially) next position 
        Vector3 Drift_t2 = SLB_ComputeDrift(virtPosCur, realPosCur, curVelocity);

        //driftList.Add(Drift_t2);
        //3. Get drift associated with older position 

        Vector3 displacementVec = realPosCur - realPosOld;
        Vector3 normalizeDrift;
        //if (Drift_t2.magnitude > 0.05f)//If drift is large, I only keep direction
        normalizeDrift = Drift_t2.normalized;
        //else //If it is below my threshold (5cm), correction is weighed within the rang (0, 0.05)->(0,1)
        //	normalizeDrift = Drift_t2 / 0.05f;
        Vector3 normalizeDisp = displacementVec.normalized;
        float dotprod = Vector3.Dot(normalizeDrift, normalizeDisp);

        //4. Compute explression according to "correction" formula: 
        float correction = -dotprod * alpha;

        if (correction < -epsilon)
            correction = 1 - epsilon;
        else if (correction > epsilon)
            correction = 1 + epsilon;
        else
            correction = 1 + correction;
        return K_nf * correction;
    }

    private Vector3 SLB_ComputeDrift(Vector3 userVPos, Vector3 userRPos, float curVelocity)
    {
        Vector3 origin = userVPos;
        float v0 = curVelocity;
        Vector3 oldUserPosition = userVPos;             //User Virtual position in simulation
        Vector3 newUserPosition = userVPos;
        Vector3 drift = userVPos - userRPos; ;          //Current drift. Initially set to VirtUserPos(t=now)- RealUserPos(t=now) => Equivalent to our "Gain" vector
        Vector3 endPosition = new Vector3(0, 0, 0);     //Target position --> We simulate steps in direction towards the center (0,0,0)

        // to comput the scaling factor at eachtime t we need:
        // a. initializing variables
        float deltaTime = 0.0111f;                      // time with 90fps = 1 / 90

        // 2.2 Variables to detect simulation end.
        float lastDist = userVPos.x * userVPos.x + userVPos.z * userVPos.z; //Squared distance
        float dist = lastDist;

        //3. Simulation Steps
        //3.1. Create a displacement vector of towards the centre (size=displacement variable)
        Vector3 delta_userPos = (endPosition - userVPos);   //Vector in direction to centre
        delta_userPos.Normalize();                          //Make it unitary

        //3.2. Apply that displacement to current position, untill we reach the centre 
        while (dist <= lastDist && dist >= 0.0025f)//&& dist > 0)//while we are approaching (we will come to a point when we overshoot --> dist>lastDist).
        {
            //Apply a navigation step
            oldUserPosition = newUserPosition;
            float velocity = quadraticSpeed(origin, endPosition, newUserPosition, v0 < 0.05f ? 0.05f : v0);
            //Debug.Log("Velocity "+ curVelocity+"\n");//Checked it varies between 0.2 and 1.4m/s

            // compute curScaling factor
            float SLB_k = SLB_GetScale(velocity);

            //float k = GetK_Naive(oldUserPosition);
            newUserPosition += SLB_k * delta_userPos * velocity * deltaTime;

            //Correct distances to detects arrival: 
            lastDist = dist;
            dist = newUserPosition.x * newUserPosition.x + newUserPosition.z * newUserPosition.z;//squaredDist

            if (dist <= lastDist)// or t >= t_2?
            {
                //compute increment to drift vector
                drift += (SLB_k - 1) * delta_userPos * velocity * deltaTime;
            }
        }
        return drift;
    }

    static public float quadraticSpeed(Vector3 originVW, Vector3 targetVW, Vector3 curPos, float v0 = 0.05f, float maxV = 1.4f)
    {
        float L = (targetVW - originVW).magnitude;
        float l = (curPos - originVW).magnitude;
        float a = (2 * maxV - v0) / L;

        if (l < L / 2)
            return v0 + a * l;
        else if (l < L)
            return maxV - a * (l - L / 2);
        else
            return 0.05f;

    }


    private Vector3 GetDrift(Vector3 userPosition, Vector3 playAreaTransformAux, float currK)
    {
        //if (currK == 1)
        //    return oldVectorDrift;

        //A. Initial drift
        Vector3 drift = playAreaTransformAux; //This implicitly contains VirtUserPos(t=now)- RealUserPos(t=now)

        Vector3 oldUserPosition = userPosition;
        Vector3 newUserPosition = userPosition;

        //B simulate steos in direction towards the center (0,0,0)
        Vector3 endPosition = new Vector3(0, 0, 0);
        float displacement = 0.01f; //Assume the user will move 1cm per frame.
        //B.1. Compute a displacement vector of "displacement units" towards the centre
        Vector3 delta_userPos = (endPosition - userPosition);   //Vector in direction to centre
        delta_userPos.Normalize();                              //Make it unitary
        delta_userPos *= displacement;                          //make it "displacement units" long

        //B.2. Take delta_userPos steps towards the center (returning to 0,0,0)
        //Detect we arrive to target
        float dist, lastDist;
        lastDist = newUserPosition.x * userPosition.x + newUserPosition.z * userPosition.z;
        dist = lastDist;

        while (dist <= lastDist && dist >= 0.05f)//&& dist > 0)//while we are approaching (we will come to a point when we overshoot --> dist>lastDist).
        {
            //Apply a navigation step
            oldUserPosition = newUserPosition;
            AreaOfInterest candidateAOI = selectCurrentAOI(oldUserPosition);
            float k = candidateAOI.getKValue(oldUserPosition);
            newUserPosition += k * delta_userPos;
            //Correct distances to detects arrival: 
            lastDist = dist;
            dist = Mathf.Abs(newUserPosition.x * userPosition.x + newUserPosition.z * userPosition.z);
            if (dist <= lastDist)// && dist > 0)
            {
                //compute increment to drift vector
                drift += (k - 1) * delta_userPos;
            }
        }

        //Compute drift from that simulation:
        //oldVectorDrift = drift;
        return drift;
    }

    private float GetCorrectK(float k, Vector3 curHeadPosInWorld, Vector3 lastHeadPosInWorld, Vector3 playAreaTransform)
    {
        Vector3 displacement = curHeadPosInWorld - lastHeadPosInWorld;

        //A Calcular drift con NaviFuield sin correccion
        //A.1. Desplazamiento aplicado al mundo (por el K-1 del Vive)
        Vector3 playAreaTransformAux = new Vector3(playAreaTransform.x, playAreaTransform.y, playAreaTransform.z);
        Vector3 displacementInRealWorld = displacement / k;
        //float prevDrift = GetDrift(lastHeadPosInWorld, playAreaTransformAux, k).magnitude;
        //Calcular cuanto se movera el entorno (k-1)*realDisplacement+playAreaTransform
        playAreaTransformAux += (k - 1) * displacementInRealWorld;                                //DIEGO: Used this straightaway

        //Drift final sin correcciones:      
        Vector3 currDriftVect = GetDrift(curHeadPosInWorld, playAreaTransformAux, k);
        float finalDrift = currDriftVect.magnitude;
        //driftList.Add(currDriftVect);


        /*if (prevDrift != 0)
        {
            //float TargetDrift = 0.5f;
            float deltaTime = 0.0111f; // time with 90fps = 1 / 90

            Vector3 derivDrift = 0.1f * (currDriftVect - prevDriftVec) / deltaTime;
            //float driftRatio = finalDrift / TargetDrift;
            float correction = derivDrift.magnitude;//derivDrift.magnitude * driftRatio;

            if (correction > 0.3f)
                correction = 0.3f;
            if (correction < -0.25f)
                correction = -0.25f;

            k = k + k * correction;
        }
        #endregion

        prevDriftVec = currDriftVect;
        prevDrift = finalDrift;
            
        */

        return k; // * weighFactor / delta_t// Are we using this at all?
    }

    /*Returns K, corrected to reduce drift. Requires current and previour positions in real and virtual.*/
    float GetKMod2(Vector3 realPosOld, Vector3 realPosCur, Vector3 virtPosOld, Vector3 virtPosCur)// Vector3 userRPos,
    {
        //1. Get uncorrected K
        float K_nf = GetK_Naive(virtPosOld);

        //2. Get drift associated with (potentially) next position (Pv_t2, Pr_t2)
        Vector3 Drift_t2 = ComputeDrift(virtPosCur, realPosCur);

        //driftList.Add(Drift_t2);
        //3. Get drift associated with current position (Pv_t1, Pr_t1)

        Vector3 displacementVec = virtPosCur - virtPosOld;
        Vector3 normalizeDrift = Drift_t2.normalized;
        Vector3 normalizeDisp = displacementVec.normalized;
        float dotprod = Vector3.Dot(normalizeDrift, normalizeDisp);

        //4. Compute explression according to "correction" formula: 
        float correction = -dotprod * alpha;

        if (correction < -epsilon)
            correction = 1 - epsilon;
        else if (correction > epsilon)
            correction = 1 + epsilon;
        else
            correction = 1 + correction;
        //scalingFactorNavi = correction;
        return K_nf * correction;
        /*
        float correction = 0;
        correction = Remap(dotprod, -1f, 1f, -0.25f, 0.25f);

        return K_nf * (1 - correction); // * weighFactor / delta_t  K_nf * (1 + correction)     */
    }

    static public float thresholdTravelDistance() { return 0.3f; }//30 cm from the centre?
}

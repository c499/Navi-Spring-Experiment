using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Valve.VR;

public class EventManager : MonoBehaviour
{
    #region Fields

    public static EventManager current;


    [Header("Settings")]
    public int maxStages = 8;

    public bool destroyAll = false;
    public bool recording = false;

    public Vector3 currentLocation;
   
    public bool end = false;

    [Header("Sequence Settings")]
    public bool randomiseSequence = false;

    public int randomNumber;
    public string sequenceText = "";
    public int[] currentSequence;
    public int currentMultiplier;
    public bool multiplierActive = false;

    [Header("Participant Settings")]
    public bool isParticipant;

    public int participantNumber;

    public bool readFromFiles = false;
    public bool readSequenceFromFile = false;
    public TextAsset dataFile;
    public bool fixedNumberOfStages = false;

    [Range(1, 100)]
    public int numberOfStages = 0;

    public bool fixedNumberOfSequences = false;

    [Range(1, 100)]
    public int numberOfSequences = 0;

    public bool sequencesRepeat = false;

    //public GameObject[] runObject;
    //public int[] stageNumber;
    //public int[] multiplierNumber;

    public MultiplierNumber[] multiplierNumber;

    [Header("Other Settings")]

    public Vector3 transformParent;
    public Vector3[] recordedVector;


    //[HideInInspector]
    public float triggerCount = 0, correctCount = 0, maxDistance;

    //[HideInInspector]
    public int countedSequenceNumber = 0, currentRunNumber = 0, currentStageNumber = 0, currentMultiplierNumber = 0, totalRuns;


    public Transform circleSmall;
    public float directionMultiplier;

    [HideInInspector]
    public Vector3 center, realCenter, north, south, east, west, northEast, southEast, northWest, southWest, circle, custom;

    [HideInInspector]
    public Quaternion defaultRot;

    protected SteamVR_PlayArea steamVR_PlayArea;
    protected Transform cameraRig;

    private HmdQuad_t steamVrBounds;
    private SteamVR_PlayArea.Size lastSize;
    private Vector3[] vertices;

    #endregion Fields

    #region Methods

    public void Start()
    {
        steamVR_PlayArea = GameObject.FindObjectOfType<SteamVR_PlayArea>();
        cameraRig = GameObject.FindObjectOfType<SteamVR_PlayArea>().gameObject.transform;

        if (steamVR_PlayArea)
            Debug.Log("steamVR_PlayArea  object found: " + steamVR_PlayArea.name);
        else
            Debug.Log("No steamVR_PlayArea  object could be found");

        if (vertices == null || vertices.Length == 0)
        {
            bool success = SteamVR_PlayArea.GetBounds(steamVR_PlayArea.size, ref steamVrBounds);
            if (success)
            {
                lastSize = steamVR_PlayArea.size;
            }
            else
            {
                Debug.Log("Could not get the chaperon size. This may happen if you use the calibrated size.");
            }
        }

        if (isParticipant)
        {
            dataFile = multiplierNumber[currentMultiplier].stageNumber[currentStageNumber].sequence[currentRunNumber];
            currentMultiplier = multiplierNumber[0].setMultiplier;
        }

        SetBounds();

        currentRunNumber = 0;
        currentStageNumber = 0;
        currentMultiplierNumber = 0;
        totalRuns = 0;

        randomNumber = UnityEngine.Random.Range(1, (DateTime.Now.Minute * DateTime.Now.Second));
        readFile();

        circleSmall = transform.GetChild(0);

  


 
    }

    public void Update()
    {

        realCenter = cameraRig.position;

        circleSmall.GetComponent<Projector>().orthographicSize = (maxDistance * (directionMultiplier*6f) ) ;
        circleSmall.transform.position = Vector3.zero;

    }

    public void updateStatus()
    {
        currentRunNumber++;
        totalRuns++;

        if (currentRunNumber == 2)
        {
            currentStageNumber++;
            currentRunNumber = 0;
        }

        if (currentStageNumber == 2)
        {
            currentMultiplierNumber++;
            currentStageNumber = 0;

        }

        if (currentMultiplierNumber == 2)
        {
            currentMultiplierNumber = 1;
  
        }
        dataFile = multiplierNumber[currentMultiplierNumber].stageNumber[currentStageNumber].sequence[currentRunNumber];

        if (totalRuns == 8)
        {
            dataFile = multiplierNumber[0].stageNumber[0].sequence[0];
            end = true;
        }

        //Debug.Log("CurrentMultiplierIndex: " + multiplierNumber[currentMultiplierNumber]);
        //Debug.Log("CurrentStageIndex: " + multiplierNumber[currentMultiplierNumber].stageNumber[currentStageNumber]);
        //Debug.Log("CurrentSequenceIndex: " + multiplierNumber[currentMultiplierNumber].stageNumber[currentStageNumber].sequence[currentRunNumber]);

        //dataFile = multiplierNumber[currentMultiplierNumber].stageNumber[currentStageNumber].sequence[currentRunNumber];
        currentMultiplier = multiplierNumber[currentMultiplierNumber].setMultiplier;
        readFile();
    }

    public void RandomiseSequence()
    {
        randomNumber = UnityEngine.Random.Range(1, (DateTime.Now.Minute * DateTime.Now.Second));
        for (int i = 0; i < currentSequence.Length; i++)

        {
            currentSequence[i] = (currentSequence[i] + 4) % 8;
            if (currentSequence[i] == 0)
            {
                currentSequence[i] = 8;
            }
        }
    }

    public void readFile()
    {
        sequenceText = "";
        string[] data = dataFile.text.Split('\n', ',', ' ');
        int[] dataValue = Array.ConvertAll(data, s => int.Parse(s));

        //Debug.Log ("Size of Array:" + data.Length);
        Array.Resize(ref currentSequence, data.Length);

       

        for (int i = 0; i < currentSequence.Length; i++)

        {
            if (randomiseSequence)
            {
                dataValue[i] = (dataValue[i] + randomNumber) % 8;

                if (dataValue[i] == 0)
                {
                    dataValue[i] = 8;
                }

                currentSequence[i] = dataValue[i];
                sequenceText += currentSequence[i].ToString() + ",";

                //Debug.Log("Sequence Number" + (sequence[i]));
            }
            else
            {
                currentSequence[i] = dataValue[i];
                sequenceText += currentSequence[i].ToString() + ",";

                //Debug.Log("Sequence Number" + (sequence[i]));
            }
        }
    }

    public void GetBounds(ref HmdQuad_t pRect)
    {
        CheckAndUpdateBounds();

        if (steamVR_PlayArea.size == SteamVR_PlayArea.Size.Calibrated)
        {
            pRect.vCorners0.v2 = steamVrBounds.vCorners0.v2;
            pRect.vCorners0.v2 = pRect.vCorners0.v2 > steamVrBounds.vCorners0.v2 ? steamVrBounds.vCorners0.v2 : pRect.vCorners0.v2;
            pRect.vCorners1.v0 = steamVrBounds.vCorners1.v0;
            pRect.vCorners1.v0 = pRect.vCorners1.v0 > steamVrBounds.vCorners1.v0 ? steamVrBounds.vCorners1.v0 : pRect.vCorners1.v0;
            pRect.vCorners2.v2 = steamVrBounds.vCorners2.v2;
            pRect.vCorners2.v2 = pRect.vCorners2.v2 < steamVrBounds.vCorners2.v2 ? steamVrBounds.vCorners2.v2 : pRect.vCorners2.v2;
            pRect.vCorners3.v0 = steamVrBounds.vCorners3.v0;
            pRect.vCorners3.v0 = pRect.vCorners3.v0 < steamVrBounds.vCorners3.v0 ? steamVrBounds.vCorners3.v0 : pRect.vCorners3.v0;
        }
        else
        {
            pRect.vCorners0.v2 = steamVrBounds.vCorners0.v2;
            pRect.vCorners0.v2 = pRect.vCorners0.v2 < steamVrBounds.vCorners0.v2 ? steamVrBounds.vCorners0.v2 : pRect.vCorners0.v2;
            pRect.vCorners1.v0 = steamVrBounds.vCorners1.v0;
            pRect.vCorners1.v0 = pRect.vCorners1.v0 < steamVrBounds.vCorners1.v0 ? steamVrBounds.vCorners1.v0 : pRect.vCorners1.v0;
            pRect.vCorners2.v2 = steamVrBounds.vCorners2.v2;
            pRect.vCorners2.v2 = pRect.vCorners2.v2 > steamVrBounds.vCorners2.v2 ? steamVrBounds.vCorners2.v2 : pRect.vCorners2.v2;
            pRect.vCorners3.v0 = steamVrBounds.vCorners3.v0;
            pRect.vCorners3.v0 = pRect.vCorners3.v0 > steamVrBounds.vCorners3.v0 ? steamVrBounds.vCorners3.v0 : pRect.vCorners3.v0;
        }
        pRect.vCorners0.v1 = 0;
        pRect.vCorners1.v1 = 0;
        pRect.vCorners2.v1 = 0;
        pRect.vCorners3.v1 = 0;
        pRect.vCorners0.v0 = steamVrBounds.vCorners0.v0;
        pRect.vCorners0.v0 = pRect.vCorners0.v0 < steamVrBounds.vCorners0.v0 ? steamVrBounds.vCorners0.v0 : pRect.vCorners0.v0;
        pRect.vCorners1.v2 = steamVrBounds.vCorners1.v2;
        pRect.vCorners1.v2 = pRect.vCorners1.v2 > steamVrBounds.vCorners1.v2 ? steamVrBounds.vCorners1.v2 : pRect.vCorners1.v2;
        pRect.vCorners2.v0 = steamVrBounds.vCorners2.v0;
        pRect.vCorners2.v0 = pRect.vCorners2.v0 > steamVrBounds.vCorners2.v0 ? steamVrBounds.vCorners2.v0 : pRect.vCorners2.v0;
        pRect.vCorners3.v2 = steamVrBounds.vCorners3.v2;
        pRect.vCorners3.v2 = pRect.vCorners3.v2 < steamVrBounds.vCorners3.v2 ? steamVrBounds.vCorners3.v2 : pRect.vCorners3.v2;
    }

    public void CheckAndUpdateBounds()
    {
        if (lastSize != steamVR_PlayArea.size)
        {
            bool success = SteamVR_PlayArea.GetBounds(steamVR_PlayArea.size, ref steamVrBounds);
            if (success)
            {
                lastSize = steamVR_PlayArea.size;
            }
            else
            {
                Debug.LogWarning("Could not get the Play Area bounds. " + steamVR_PlayArea.size);
            }
        }
    }


    /**
        Computes the maximum distance between the current location of the camera rig and the limits of the PlayArea (using eight points N,E,S,W, 
        THIS IS WRONG! Limits of play area are always relative to camera rig!!
         */
    public void SetBounds()
    {
        center = transform.position;
        center.y = 0f;

        realCenter = cameraRig.position;
        defaultRot = Quaternion.Euler(0, 0, 0);

        northEast = new Vector3(steamVrBounds.vCorners0.v0, 0, steamVrBounds.vCorners0.v2);
        southEast = new Vector3(steamVrBounds.vCorners1.v0, 0, steamVrBounds.vCorners1.v2);
        northWest = new Vector3(steamVrBounds.vCorners3.v0, 0, steamVrBounds.vCorners3.v2);
        southWest = new Vector3(steamVrBounds.vCorners2.v0, 0, steamVrBounds.vCorners2.v2);

        north = new Vector3(0, 0, steamVrBounds.vCorners0.v2);
        south = new Vector3(0, 0, steamVrBounds.vCorners1.v2);
        east = new Vector3(steamVrBounds.vCorners0.v0, 0, 0);
        west = new Vector3(steamVrBounds.vCorners2.v0, 0, 0);

        float maxDistance1 = Vector3.Distance(realCenter, northEast);
        float maxDistance2 = Vector3.Distance(realCenter, southEast);
        float maxDistance3 = Vector3.Distance(realCenter, northWest);
        float maxDistance4 = Vector3.Distance(realCenter, southWest);
        float maxDistance5 = Vector3.Distance(realCenter, north);
        float maxDistance6 = Vector3.Distance(realCenter, south);
        float maxDistance7 = Vector3.Distance(realCenter, east);
        float maxDistance8 = Vector3.Distance(realCenter, west);

        float[] distances = { maxDistance1, maxDistance2, maxDistance3, maxDistance4, maxDistance5, maxDistance6, maxDistance7, maxDistance8 };

        maxDistance = distances.Min();
    }

    public void ReadFileName()
    {
        string myPath = "Assets/Created Assets/TextFile/";
        string searchString = "*.txt?";

        DirectoryInfo dir = new DirectoryInfo(myPath);
        FileInfo[] info = dir.GetFiles(searchString, SearchOption.AllDirectories);
        foreach (FileInfo f in info)
        {
            if (multiplierNumber.Length != 0)
            {
                for (int k = 0; k < multiplierNumber.Length; k++)
                {
                    for (int i = 0; i < multiplierNumber[k].stageNumber.Length; i++)
                    {
                        for (int j = 0; j < multiplierNumber[k].stageNumber[i].sequence.Length; j++)
                        {
                            if (f.Name.Contains(multiplierNumber[k].stageNumber[i].sequenceLength.ToString()) && f.Name.Contains("Short"))
                            {
                                multiplierNumber[k].stageNumber[i].sequence[0] = (TextAsset)AssetDatabase.LoadAssetAtPath(myPath + f.Name, typeof(TextAsset));
                            }

                            if (f.Name.Contains(multiplierNumber[k].stageNumber[i].sequenceLength.ToString()) && f.Name.Contains("Long"))
                            {
                                multiplierNumber[k].stageNumber[i].sequence[1] = (TextAsset)AssetDatabase.LoadAssetAtPath(myPath + f.Name, typeof(TextAsset));
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnValidate()
    {
        if (fixedNumberOfStages)
        {
            fixedNumberOfSequences = false;
            FixStageNumber();
        }

        if (fixedNumberOfSequences)
        {
            fixedNumberOfStages = false;
            FixSequenceNumber();
        }

        if (fixedNumberOfSequences && fixedNumberOfStages)
        {
            fixedNumberOfStages = false;
            fixedNumberOfSequences = false;
        }

        if (sequencesRepeat)
        {
            FixRepeat();
        }

        if (readFromFiles)
        {
            ReadFileName();
        }

        if (readSequenceFromFile)
        {
            readFile();
        }

        if (isParticipant)
        {
            ParticipantNumber();
            ReadFileName();
        }
    }

    private void FixSequenceNumber()
    {
        for (int i = 0; i < multiplierNumber.Length; i++)
        {
            for (int j = 0; j < multiplierNumber[0].stageNumber.Length; j++)
            {
                if (multiplierNumber[i].stageNumber[j].sequence != null)
                {
                    Array.Resize(ref multiplierNumber[i].stageNumber[j].sequence, numberOfSequences);
                }
            }
        }
    }

    private void FixStageNumber()
    {
        for (int i = 0; i < multiplierNumber.Length; i++)
        {
            Array.Resize(ref multiplierNumber[i].stageNumber, numberOfStages);
        }
    }

    private void FixRepeat()
    {
        if (multiplierNumber.Length != 0)
        {
            for (int k = 0; k < multiplierNumber.Length; k++)
            {
                for (int i = 0; i < multiplierNumber[k].stageNumber.Length; i++)
                {
                    for (int j = 0; j < multiplierNumber[k].stageNumber[i].sequence.Length; j++)
                    {
                        multiplierNumber[k].stageNumber[i].sequence[j] = multiplierNumber[k].stageNumber[i].sequenceText;
                    }
                }
            }
        }
    }

    private void ParticipantNumber()
    {
        Array.Resize(ref multiplierNumber, 2);
        for (int i = 0; i < multiplierNumber.Length; i++)
        {
            Array.Resize(ref multiplierNumber[i].stageNumber, 2);
        }

        for (int i = 0; i < multiplierNumber.Length; i++)
        {
            for (int j = 0; j < multiplierNumber[0].stageNumber.Length; j++)
            {
                if (multiplierNumber[i].stageNumber[j].sequence != null)
                {
                    Array.Resize(ref multiplierNumber[i].stageNumber[j].sequence, 2);
                }
            }
        }



        if (participantNumber == 0)
        {
            multiplierActive = false;
            multiplierNumber[0].setMultiplier = 1;
            multiplierNumber[1].setMultiplier = 1;

            multiplierNumber[0].stageNumber[0].sequenceLength = 1;
            multiplierNumber[0].stageNumber[1].sequenceLength = 1;

            multiplierNumber[1].stageNumber[0].sequenceLength = 1;
            multiplierNumber[1].stageNumber[1].sequenceLength = 1;

        }


        if (participantNumber == 10)
        {
            multiplierActive = true;
            multiplierNumber[0].setMultiplier = 1;
            multiplierNumber[1].setMultiplier = 7;

            multiplierNumber[0].stageNumber[0].sequenceLength = 1;
            multiplierNumber[0].stageNumber[1].sequenceLength = 5;

            multiplierNumber[1].stageNumber[0].sequenceLength = 1;
            multiplierNumber[1].stageNumber[1].sequenceLength = 5;

        }


        if (participantNumber == 11)
        {
            multiplierActive = false;
            multiplierNumber[0].setMultiplier = 1;
            multiplierNumber[1].setMultiplier = 7;
 
            multiplierNumber[0].stageNumber[0].sequenceLength = 1;
            multiplierNumber[0].stageNumber[1].sequenceLength = 5;

            multiplierNumber[1].stageNumber[0].sequenceLength = 1;
            multiplierNumber[1].stageNumber[1].sequenceLength = 5;
  
        }

        if (participantNumber == 20)
        {
            multiplierActive = true;
            multiplierNumber[0].setMultiplier = 1;
            multiplierNumber[1].setMultiplier = 7;

            multiplierNumber[0].stageNumber[0].sequenceLength = 5;
            multiplierNumber[0].stageNumber[1].sequenceLength = 1;

            multiplierNumber[1].stageNumber[0].sequenceLength = 5;
            multiplierNumber[1].stageNumber[1].sequenceLength = 1;

        }

        if (participantNumber == 21)
        {
            multiplierActive = false;
            multiplierNumber[0].setMultiplier = 1;
            multiplierNumber[1].setMultiplier = 7;

            multiplierNumber[0].stageNumber[0].sequenceLength = 5;
            multiplierNumber[0].stageNumber[1].sequenceLength = 1;

            multiplierNumber[1].stageNumber[0].sequenceLength = 5;
            multiplierNumber[1].stageNumber[1].sequenceLength = 1;

        }

        if (participantNumber == 30)
        {
            multiplierActive = true;
            multiplierNumber[0].setMultiplier = 7;
            multiplierNumber[1].setMultiplier = 1;

            multiplierNumber[0].stageNumber[0].sequenceLength = 1;
            multiplierNumber[0].stageNumber[1].sequenceLength = 5;

            multiplierNumber[1].stageNumber[0].sequenceLength = 1;
            multiplierNumber[1].stageNumber[1].sequenceLength = 5;
        }

        if (participantNumber == 31)
        {
            multiplierActive = false;
            multiplierNumber[0].setMultiplier = 7;
            multiplierNumber[1].setMultiplier = 1;

            multiplierNumber[0].stageNumber[0].sequenceLength = 1;
            multiplierNumber[0].stageNumber[1].sequenceLength = 5;

            multiplierNumber[1].stageNumber[0].sequenceLength = 1;
            multiplierNumber[1].stageNumber[1].sequenceLength = 5;
        }

        if (participantNumber == 40)
        {
            multiplierActive = true;
            multiplierNumber[0].setMultiplier = 7;
            multiplierNumber[1].setMultiplier = 1;

            multiplierNumber[0].stageNumber[0].sequenceLength = 5;
            multiplierNumber[0].stageNumber[1].sequenceLength = 1;

            multiplierNumber[1].stageNumber[0].sequenceLength = 5;
            multiplierNumber[1].stageNumber[1].sequenceLength = 1;
        }

        if (participantNumber == 41)
        {
            multiplierActive = false;
            multiplierNumber[0].setMultiplier = 7;
            multiplierNumber[1].setMultiplier = 1;

            multiplierNumber[0].stageNumber[0].sequenceLength = 5;
            multiplierNumber[0].stageNumber[1].sequenceLength = 1;

            multiplierNumber[1].stageNumber[0].sequenceLength = 5;
            multiplierNumber[1].stageNumber[1].sequenceLength = 1;
        }

        if (participantNumber == 51)
        {
            multiplierActive = true;
            multiplierNumber[0].setMultiplier = 1;
            multiplierNumber[1].setMultiplier = 7;

            multiplierNumber[0].stageNumber[0].sequenceLength = 1;
            multiplierNumber[0].stageNumber[1].sequenceLength = 5;

            multiplierNumber[1].stageNumber[0].sequenceLength = 1;
            multiplierNumber[1].stageNumber[1].sequenceLength = 5;

        }


        if (participantNumber == 50)
        {
            multiplierActive = false;
            multiplierNumber[0].setMultiplier = 1;
            multiplierNumber[1].setMultiplier = 7;

            multiplierNumber[0].stageNumber[0].sequenceLength = 1;
            multiplierNumber[0].stageNumber[1].sequenceLength = 5;

            multiplierNumber[1].stageNumber[0].sequenceLength = 1;
            multiplierNumber[1].stageNumber[1].sequenceLength = 5;

        }

        if (participantNumber == 61)
        {
            multiplierActive = true;
            multiplierNumber[0].setMultiplier = 1;
            multiplierNumber[1].setMultiplier = 7;

            multiplierNumber[0].stageNumber[0].sequenceLength = 5;
            multiplierNumber[0].stageNumber[1].sequenceLength = 1;

            multiplierNumber[1].stageNumber[0].sequenceLength = 5;
            multiplierNumber[1].stageNumber[1].sequenceLength = 1;

        }

        if (participantNumber == 60)
        {
            multiplierActive = false;
            multiplierNumber[0].setMultiplier = 1;
            multiplierNumber[1].setMultiplier = 7;

            multiplierNumber[0].stageNumber[0].sequenceLength = 5;
            multiplierNumber[0].stageNumber[1].sequenceLength = 1;

            multiplierNumber[1].stageNumber[0].sequenceLength = 5;
            multiplierNumber[1].stageNumber[1].sequenceLength = 1;

        }

        if (participantNumber == 71)
        {
            multiplierActive = true;
            multiplierNumber[0].setMultiplier = 7;
            multiplierNumber[1].setMultiplier = 1;

            multiplierNumber[0].stageNumber[0].sequenceLength = 1;
            multiplierNumber[0].stageNumber[1].sequenceLength = 5;

            multiplierNumber[1].stageNumber[0].sequenceLength = 1;
            multiplierNumber[1].stageNumber[1].sequenceLength = 5;
        }

        if (participantNumber == 70)
        {
            multiplierActive = false;
            multiplierNumber[0].setMultiplier = 7;
            multiplierNumber[1].setMultiplier = 1;

            multiplierNumber[0].stageNumber[0].sequenceLength = 1;
            multiplierNumber[0].stageNumber[1].sequenceLength = 5;

            multiplierNumber[1].stageNumber[0].sequenceLength = 1;
            multiplierNumber[1].stageNumber[1].sequenceLength = 5;
        }

        if (participantNumber == 81)
        {
            multiplierActive = true;
            multiplierNumber[0].setMultiplier = 7;
            multiplierNumber[1].setMultiplier = 1;

            multiplierNumber[0].stageNumber[0].sequenceLength = 5;
            multiplierNumber[0].stageNumber[1].sequenceLength = 1;

            multiplierNumber[1].stageNumber[0].sequenceLength = 5;
            multiplierNumber[1].stageNumber[1].sequenceLength = 1;
        }

        if (participantNumber == 80)
        {
            multiplierActive = false;
            multiplierNumber[0].setMultiplier = 7;
            multiplierNumber[1].setMultiplier = 1;

            multiplierNumber[0].stageNumber[0].sequenceLength = 5;
            multiplierNumber[0].stageNumber[1].sequenceLength = 1;

            multiplierNumber[1].stageNumber[0].sequenceLength = 5;
            multiplierNumber[1].stageNumber[1].sequenceLength = 1;
        }




        //if (participantNumber > 18)
        //{
        // participantNumber = 1;
        //}
    }

    private void Awake()
    {
        current = this;
    }

    [System.Serializable]
    public class MultiplierNumber
    {
        #region Fields

        public int setMultiplier;
        public StageNumber[] stageNumber;

        #endregion Fields

        #region Classes

        [System.Serializable]
        public class StageNumber
        {
            #region Fields

            public int sequenceLength;
            public TextAsset sequenceText;

            public TextAsset[] sequence;

            #endregion Fields
        }

        #endregion Classes
    }

    #endregion Methods
}
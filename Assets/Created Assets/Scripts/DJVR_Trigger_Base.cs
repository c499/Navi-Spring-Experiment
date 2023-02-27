namespace VRTK
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class DJVR_Trigger_Base : MonoBehaviour
    {
        #region Fields

        [Header("-Object Spawn")]
        [Header("Object Settings")]
        public bool placeObject = true;

        public bool placeOriginalObject = false;

        public bool startOfTest;
        public bool endOfTest;

        public int numberOfObjects = 0;

        [Header("-Position")]
        public bool centerParent = true;

        public bool centerThisObject = false;

        public bool centerRealWorld = false;
        public bool centerGameWorld = false;

        public bool centerTrigger = false;
        public GameObject[] placedObject;
        public PlacedDirection[] placedDirections;
        public Vector3[] placedVector;

        [Range(1, 10)]
        public float[] directionMultiplier;

        [Range(0, 100)]
        public float[] heightValue;

        [Header("-Position - Circle")]
        public bool useMaxRadius = false;

        public float circleRadius;
        public int[] angle;
        public bool factorInMultiplier = false;

        [Header("-Colour")]
        public bool changeColour = false;

        public bool changeColourLight = false;
        public Color[] containerColor;

        [Header("-Number")]
        public bool numberObject = false;

        [Header("-Audio")]
        [Header("Trigger Settings")]
        public bool playAudio = true;

        public AudioClip correctClip;
        public AudioClip incorrectClip;
        public AudioClip winClip;

        [Header("-Multiplier")]
        public bool affectAdditionalMovement = false;

        public float setAdditionalMovement;

        [Header("-Destroy")]
        public bool destroyGameObject = true;

        public float destroyTimeInSeconds = 5;
        public bool onDestroyDestroyAll = false;

        [Header("-Timer")]
        public bool setTimer = false;

        public float triggerTime = 0;

        [Header("-Recording")]
        public bool beginRecording = false;

        [Header("-Misc")]
        public float maximumTriggers = 1;

        public bool moveToWorldCenter = false;
        public bool setAOIToCenter = false;
        public bool recordTriggerPosition = false;
        public bool placeTriggerPosition = false;
        public float distanceFromCenter;

        public Material[] objMaterial;

        [Header("Sequence Settings")]
        public bool adoptCurrentSequence = true;

        public bool adoptCurrentSequenceText = false;
        public bool adoptMultiplier = false;
        public string sequenceText = "";
        public int[] sequence;

        [Header("Misc Settings")]
        public float distance;

        [HideInInspector]
        public int objectNumber, sequenceNumber = 0, correctCount = 0;

        [HideInInspector]
        public float numberOfTriggers = 0, time = 0, resetCount = 0;

        protected Transform cameraRig;
        protected Transform HeadsetTransform;
        protected DJVR_Navigation navigationTechnique;
        protected EventManager eventManager;
        protected GameObject sequenceCanvas;

        private static int[] input = { -1, -1, -1, -1, -1, -1, -1, -1 };
        private GameObject test;
        private GameObject gameObject1 = null;
        private PlacedDirection placedDirection;

        private float maxDistance;
        private Vector3 center;
        private Vector3 realCenter;
        private Quaternion defaultRot;
        private Vector3 north;
        private Vector3 south;
        private Vector3 east;
        private Vector3 west;
        private Vector3 northEast;
        private Vector3 southEast;
        private Vector3 northWest;
        private Vector3 southWest;
        private Vector3 circle;
        private Vector3 custom;

        #endregion Fields

        #region Enums

        public enum PlacedDirection
        {
            north,
            northEast,
            east,
            southEast,
            south,
            southWest,
            west,
            northWest,
            circle,
            angle,
            custom
        };

        #endregion Enums

        #region Methods

        public void Start()
        {
            navigationTechnique = DJVR_Navigation.current;
            eventManager = EventManager.current;
            sequenceCanvas = GameObject.Find("SequenceCanvas");
            cameraRig = GameObject.FindObjectOfType<SteamVR_PlayArea>().gameObject.transform;
            HeadsetTransform = VRTK.DeviceFinder.HeadsetTransform();
     

            DestroyClone();

            sequenceNumber = eventManager.countedSequenceNumber;

            if (adoptCurrentSequenceText)
            {
                if (eventManager.totalRuns > 0)
                {
                    this.transform.GetComponentInChildren<DJVR_ObjectTooltip>().containerSize = new Vector2(220f, 95f);

                    this.transform.GetComponentInChildren<DJVR_ObjectTooltip>().displayText = "Congratulations! On to the next sequence.\r\nPlease stand in the innermost circle\r\nand pull a trigger to start.\r\n\r\nNext sequence: ";
                }

                if (eventManager.totalRuns == 8)
                {
                    adoptCurrentSequenceText = false;

                    transform.GetComponent<DJVR_Trigger_>().onlyTriggerInAreaClose = false;

                    this.transform.GetComponentInChildren<DJVR_ObjectTooltip>().containerSize = new Vector2(220f, 95f);

                    this.transform.GetComponentInChildren<DJVR_ObjectTooltip>().displayText = "Congratulations! Experiment Complete!\r\nYou may now remove the headset";
                }
            }

            AdoptSequenceDetails();

            if (placeTriggerPosition)
            {
                Vector3 currentLocationY = eventManager.currentLocation;
                currentLocationY.y = 0;

                transform.position = currentLocationY;
            }

            eventManager.transformParent = transform.parent.position;

            if (startOfTest)
            {
                ResetCameraRig();
            }
        }

        // Update is called once per frame
        public void Update()
        {
            sequenceNumber = eventManager.countedSequenceNumber;
            if (moveToWorldCenter)
            {
                Vector3 cameraRigY = cameraRig.localPosition;
                cameraRigY.y = 0f;

                Quaternion cameraRigYRotation = cameraRig.rotation;

                transform.parent.position = cameraRigY;
                transform.parent.rotation = cameraRigYRotation;
            }

            if (Input.GetKeyDown(KeyCode.A))
                AllCorrect();
        }

        public void AdoptSequenceDetails()
        {
            if (sequence != null && eventManager != null)
            {
                if (adoptCurrentSequence)
                {
                    Array.Resize(ref sequence, eventManager.currentSequence.Length);

                    for (int i = 0; i < sequence.Length; i++)

                    {
                        sequence[i] = eventManager.currentSequence[i];
                        sequenceText = eventManager.sequenceText;
                        if (adoptCurrentSequenceText)
                        {
                            this.transform.GetComponentInChildren<DJVR_ObjectTooltip>().displayText += sequence[i].ToString() + ",";
                        }
                    }
                }

                if (adoptMultiplier)
                {
                    setAdditionalMovement = eventManager.currentMultiplier;
                }

                if (factorInMultiplier)
                {
                    for (int i = 0; i < directionMultiplier.Length; i++)
                        directionMultiplier[i] = (1 + (setAdditionalMovement + 1f)) / 2;
                }

                if (eventManager != null)
                {
                    if (transform.tag != "NotSequence" && transform.GetComponentInParent<DJVR_Trigger_AOI>() != null)
                    {
                        transform.GetComponentInParent<DJVR_Trigger_AOI>().active = eventManager.multiplierActive;
                    }
                }
            }
        }

        public void ResetCameraRig()
        {
            resetCount++;
            cameraRig.position = Vector3.zero;
            eventManager.GetComponent<InputVCR>().SyncProperty("Reset Count", resetCount.ToString());
        }

        public void TriggerEvent()
        {
            numberOfTriggers += 1;
            eventManager.triggerCount += 1;
            Debug.Log("TriggerCount: " + eventManager.triggerCount);
            Debug.Log("TRIGGERED: " + this.transform.name);

            if (recordTriggerPosition)
            {
                eventManager.currentLocation = HeadsetTransform.position;
            }

            if (affectAdditionalMovement)
            {
                //Debug.Log("DefaultSet to: " + navigationTechnique.defaultAdditionalMovementMultiplier);

                navigationTechnique.additionalMovementMultiplier = setAdditionalMovement;
                navigationTechnique.defaultAdditionalMovementMultiplier = setAdditionalMovement;

                navigationTechnique.additionalMovementMultiplier = navigationTechnique.defaultAdditionalMovementMultiplier;
                //Debug.Log("NormalSet to: " + navigationTechnique.additionalMovementMultiplier);
            }
            if (playAudio && objectNumber == 0)
            {
                AudioSource.PlayClipAtPoint(correctClip, transform.position);
            }

            if (placeObject)
            {
                SetBounds();
                PlaceObjects();
                Debug.Log("This Object Spawned Objects: " + this.transform.name);
            }
            if (destroyGameObject)
            {
                transform.parent.Recycle();
            }

            if (beginRecording && eventManager.recording == false)
            {
                if (eventManager.isParticipant)
                {
                    eventManager.GetComponentInChildren<InputVCR>().jsonRecordingName = eventManager.participantNumber.ToString();
                    eventManager.GetComponentInChildren<InputVCR>().currentRun = eventManager.currentRunNumber.ToString();
                    eventManager.GetComponentInChildren<InputVCR>().currentStage = eventManager.currentStageNumber.ToString();
                    eventManager.GetComponentInChildren<InputVCR>().currentMultiplier = eventManager.currentMultiplierNumber.ToString();
                }

                RecordStart();

                for (int i = 0; i < eventManager.recordedVector.Length; i++)
                {
                  

                    eventManager.GetComponent<InputVCR>().SyncProperty(i.ToString() + " PositionX", eventManager.recordedVector[i].x.ToString());
                    eventManager.GetComponent<InputVCR>().SyncProperty(i.ToString() + " PositionZ", eventManager.recordedVector[i].z.ToString());
                    eventManager.GetComponent<InputVCR>().SyncProperty(i.ToString() + " PositionY", eventManager.recordedVector[i].y.ToString());
                }

                eventManager.GetComponent<InputVCR>().SyncProperty("Current Sequence", eventManager.sequenceText);

                eventManager.recording = true;
            }
            else if (beginRecording && eventManager.recording == true)
            {
                RecordStart();
                eventManager.recording = false;
            }
            if (onDestroyDestroyAll && eventManager.destroyAll == false)
            {
                eventManager.destroyAll = true;
            }
            else if (onDestroyDestroyAll == false && eventManager.destroyAll == true)
            {
                eventManager.destroyAll = false;
            }
            else
            {
                eventManager.destroyAll = false;
            }
        }

        public void ConfirmSequence()
        {
            if (this.gameObject.tag != "NotSequence")
            {
                SequenceCheck();
            }
        }

        public void SetBounds()
        {
            if (centerRealWorld)
            {
                center = cameraRig.localPosition;
                center.y = 0f;
            }

            if (centerGameWorld)
            {
                center = Vector3.zero;
                center.y = 0f;
            }

            if (centerThisObject)
            {
                center = transform.position;
                center.y = 0f;
            }

            if (centerParent)
            {
                center = transform.root.position;
                center.y = 0f;
            }

            if (centerTrigger)
            {
                center = eventManager.currentLocation;
                center.y = 0f;

                Debug.Log("Center: " + center);
            }

            maxDistance = eventManager.maxDistance;

            realCenter = eventManager.realCenter;
            defaultRot = eventManager.defaultRot;

            northEast = eventManager.northEast;
            southEast = eventManager.southEast;
            northWest = eventManager.northWest;
            southWest = eventManager.southWest;

            north = eventManager.north;
            south = eventManager.south;
            east = eventManager.east;
            west = eventManager.west;

            if (useMaxRadius)
            {
                circleRadius = maxDistance;
            }
        }

        public void PlaceObjects()
        {
            int c = UnityEngine.Random.Range(1, 8);

            for (int i = 0; i < placedObject.Length; i++)
            {
                GameObject obj = ObjectPoolerScript.current.GetPooledObject();

                float a = i * (360f / placedObject.Length);

                float b = (a + (c * (360f / placedObject.Length)));

                switch (placedDirections[i])
                {
                    case PlacedDirection.north:
                        placedVector[i] = north * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];
                        break;

                    case PlacedDirection.south:
                        placedVector[i] = south * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];
                        break;

                    case PlacedDirection.east:
                        placedVector[i] = east * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];
                        break;

                    case PlacedDirection.west:
                        placedVector[i] = west * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];
                        break;

                    case PlacedDirection.northWest:
                        placedVector[i] = northWest * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];
                        break;

                    case PlacedDirection.southWest:
                        placedVector[i] = southWest * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];
                        break;

                    case PlacedDirection.northEast:
                        placedVector[i] = northEast * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];
                        break;

                    case PlacedDirection.southEast:
                        placedVector[i] = southEast * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];
                        break;

                    case PlacedDirection.circle:
                        placedVector[i] = RandomCircle(center, circleRadius, b) * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];

                        //Debug.Log("CenterX: " + center.x + " CenterY: " + center.y + " CenterZ: " + center.z);
                        //Debug.Log("CircleRadius: " + circleRadius);
                        //Debug.Log("B: " + b);
                        //Debug.Log("directionMultiplier[i] " + directionMultiplier[i]);
                        //Debug.Log("placedVector[i] " + placedVector[i]);
                        //Debug.Log("DistanceFromCenter: " + Vector3.Distance(placedVector[i], center));

                        break;

                    case PlacedDirection.angle:
                        placedVector[i] = RandomCircle(center, circleRadius, angle[i]) * directionMultiplier[i];
                        placedVector[i].y = placedVector[i].y + heightValue[i];
                        break;

                    case PlacedDirection.custom:
                        placedVector[i] = placedVector[i] * directionMultiplier[i];
                        break;

                    default:
                        break;
                }

                if (i == 7)
                {
                    gameObject1 = Instantiate(placedObject[i], placedVector[i], Quaternion.Euler(0, b, 0)) as GameObject;
                    eventManager.recordedVector[i] = placedVector[i];
                }

                if (placeOriginalObject)
                {
                    if (changeColour)
                    {
                        placedObject[i].GetComponentInChildren<Renderer>().sharedMaterial = objMaterial[i];
                        //placedObject[i].GetComponentInChildren<Renderer>().sharedMaterial = new Material(Shader.Find("Valve/vr_standard"));
                        placedObject[i].GetComponentInChildren<Renderer>().sharedMaterial.color = containerColor[i];

                        if (changeColourLight)
                        {
                            placedObject[i].GetComponentInChildren<Renderer>().sharedMaterial.EnableKeyword("_EMISSION");
                            placedObject[i].GetComponentInChildren<Renderer>().sharedMaterial.SetColor("_EmissionColor", containerColor[i]);
                        }
                    }

                    string indexNumber = (i + 1).ToString();
                    Debug.Log("Object number: " + indexNumber);

                    if (numberObject)
                    {
                        obj.transform.GetComponentInChildren<DJVR_ObjectTooltip>().displayText = indexNumber;

                        obj.transform.GetComponentInChildren<DJVR_Trigger_Base>().objectNumber = i + 1;
                    }

                    gameObject1 = Instantiate(placedObject[i], placedVector[i], defaultRot) as GameObject;
                }
                else
                {
                    if (i != 7)
                    {
                        if (changeColour)
                        {
                            //obj.GetComponentInChildren<Renderer>().sharedMaterial.shader = Shader.Find("Valve/vr_standard");

                            obj.GetComponentInChildren<Renderer>().material = objMaterial[i];
                            obj.GetComponentInChildren<Renderer>().material.color = containerColor[i];

                            if (changeColourLight)
                            {
                                obj.GetComponentInChildren<Renderer>().sharedMaterial.EnableKeyword("_EMISSION");
                                obj.GetComponentInChildren<Renderer>().sharedMaterial.SetColor("_EmissionColor", containerColor[i]);
                            }
                        }

                        string indexNumberObj = (((i + eventManager.randomNumber) % 8) + 1).ToString();
                        //string indexNumberObj = (i + 1).ToString();
                        //Debug.Log("Object number: " + indexNumberObj);

                        if (numberObject)
                        {
                            obj.transform.GetComponentInChildren<DJVR_ObjectTooltip>().displayText = indexNumberObj;

                            obj.transform.GetComponentInChildren<DJVR_Trigger_Base>().objectNumber = ((i + eventManager.randomNumber) % 8) + 1;
                            // obj.transform.GetComponentInChildren<DJVR_Trigger_Base>().objectNumber = i + 1;
                        }

                        //gameObject1 = Instantiate(placedObject[i], placedVector[i], defaultRot) as GameObject;

                        eventManager.recordedVector[i] = placedVector[i];

                        obj.transform.position = placedVector[i];
                        obj.transform.rotation = Quaternion.Euler(0, b, 0);
                        obj.SetActive(true);
                    }
                }
            }
        }

        public void SequenceCheck()
        {
            input[0] = input[1];
            input[1] = input[2];
            input[2] = input[3];
            input[3] = input[4];
            input[4] = input[5];
            input[5] = input[6];
            input[6] = input[7];
            input[7] = objectNumber;

            if (sequence[sequenceNumber] == objectNumber)
            {
                Debug.Log("Correct Answer");
                if (playAudio)
                {
                    AudioSource.PlayClipAtPoint(correctClip, transform.position);
                }
                sequenceCanvas.GetComponentInChildren<CanvasRenderer>().SetAlpha(1f);
                sequenceCanvas.GetComponentInChildren<Text>().text = "Correct Answer!";
                sequenceCanvas.GetComponentInChildren<Text>().CrossFadeAlpha(0f, 0.75f, false);
                WinCheck();
                //Debug.Log("Number Correct: " + eventManager.correctCount);
                eventManager.GetComponent<InputVCR>().SyncProperty("Touched correct object", objectNumber.ToString());
            }

            if (sequence[0] == input[0] && sequence[1] == input[1] && sequence[2] == input[2] && sequence[3] == input[3] && sequence[4] == input[4] && sequence[5] == input[5] && sequence[6] == input[6] && sequence[7] == input[7])
            {
                AllCorrect();
            }

            if (sequence[sequenceNumber] != objectNumber)
            {
                Debug.Log("Wrong Answer");
                {
                    AudioSource.PlayClipAtPoint(incorrectClip, transform.position);
                }
                sequenceCanvas.GetComponentInChildren<CanvasRenderer>().SetAlpha(1f);
                sequenceCanvas.GetComponentInChildren<Text>().text = "Wrong Answer!\nTry Again";
                sequenceCanvas.GetComponentInChildren<Text>().CrossFadeAlpha(0f, 0.75f, false);

                eventManager.GetComponent<InputVCR>().SyncProperty("Touched incorrect object", objectNumber.ToString());
            }

            if (eventManager.correctCount == sequence.Length)
            {
                AllCorrect();
            }
        }

        public void AllCorrect()
        {
            Debug.Log("All answers correct");
            eventManager.updateStatus();

            sequenceCanvas.GetComponentInChildren<CanvasRenderer>().SetAlpha(1f);
            sequenceCanvas.GetComponentInChildren<Text>().text = "All Correct!\nPlease return to the center for the next stage";
            sequenceCanvas.GetComponentInChildren<Text>().CrossFadeAlpha(0f, 20f, false);

            AudioSource.PlayClipAtPoint(winClip, transform.position);
            PlaceObjects();

            eventManager.countedSequenceNumber = 0;
            eventManager.correctCount = 0;
            eventManager.GetComponent<InputVCR>().SyncProperty("All answers correct", "");

            AdoptSequenceDetails();
        }

        public void FlashSequence()
        {
            Debug.Log("Flashed Sequence");

            sequenceCanvas.GetComponentInChildren<CanvasRenderer>().SetAlpha(1f);

            sequenceCanvas.GetComponentInChildren<Text>().text = sequenceText;
            sequenceCanvas.GetComponentInChildren<Text>().CrossFadeAlpha(0f, 1f, false);

            //Debug.Log("Number Correct: " + eventManager.correctCount);
            eventManager.GetComponent<InputVCR>().SyncProperty("Flashed Sequence", "Flashed Sequence");
        }

        public void WinCheck()
        {
            eventManager.countedSequenceNumber += 1;
            eventManager.correctCount += 1;
        }

        public Vector3 RandomCircle(Vector3 center, float radius, float a)
        {
            //Debug.Log(a);
            float ang = a;
            Vector3 pos;
            pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
            pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
            pos.y = center.y;
            return pos;
        }

        public void DestroyClone()
        {
            if (this.gameObject.tag == "NotSequence")
            {
                GameObject killDuplicate = GameObject.FindGameObjectWithTag("NotSequence");
                if (this.gameObject.GetInstanceID() < killDuplicate.GetInstanceID() && this.gameObject.name == killDuplicate.name)
                {
                    gameObject.Recycle();
                }
            }
        }

        public void RecordStart()
        {
            eventManager.GetComponentInChildren<RecordButton>().Record();
        }

        private void OnDisable()
        {
            AdoptSequenceDetails();
            sequenceText = null;
            if (eventManager != null)
            {
                sequenceText = eventManager.sequenceText;
            }
        }

        private void OnValidate()
        {
            if (placedObject.Length != numberOfObjects)
            {
                Debug.LogWarning("Don't resize the 'ints' field's array size!");
                Array.Resize(ref placedObject, numberOfObjects);
            }

            if (placedDirections.Length != numberOfObjects)
            {
                Debug.LogWarning("Don't resize the 'ints' field's array size!");
                Array.Resize(ref placedDirections, numberOfObjects);
            }

            if (placedVector.Length != numberOfObjects)
            {
                Debug.LogWarning("Don't resize the 'ints' field's array size!");
                Array.Resize(ref placedVector, numberOfObjects);
            }

            if (directionMultiplier.Length != numberOfObjects)
            {
                Debug.LogWarning("Don't resize the 'ints' field's array size!");
                Array.Resize(ref directionMultiplier, numberOfObjects);
            }

            if (heightValue.Length != numberOfObjects)
            {
                Debug.LogWarning("Don't resize the 'ints' field's array size!");
                Array.Resize(ref heightValue, numberOfObjects);
            }

            if (containerColor.Length != numberOfObjects)
            {
                Debug.LogWarning("Don't resize the 'ints' field's array size!");
                Array.Resize(ref containerColor, numberOfObjects);
            }

            if (angle.Length != numberOfObjects)
            {
                Debug.LogWarning("Don't resize the 'ints' field's array size!");
                Array.Resize(ref angle, numberOfObjects);
            }

            for (int i = 0; i < directionMultiplier.Length; i++)

            {
                if (directionMultiplier[i] == 0 && numberOfObjects != 0)
                {
                    Debug.LogWarning("Direction Multiplier is 0, will center object! For: " + this.gameObject.transform.parent.name);
                }
            }

            if (centerParent && centerThisObject)
            {
                centerParent = false;
                centerThisObject = true;
            }

            //AdoptSequenceDetails();
        }

        #endregion Methods
    }
}
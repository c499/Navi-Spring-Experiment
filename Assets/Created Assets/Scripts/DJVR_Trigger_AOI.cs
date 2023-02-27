using System;
using VRTK;
using UnityEngine;

/**
 It only computes K factor if collider is inside? Does this happen anytime? Should it be done in Update?
     
     */
public class DJVR_Trigger_AOI : MonoBehaviour
{
    #region Fields

    [Header("Area Of Interest Settings")]
    public bool active = true;

    [Header("Multiplier Distance Settings")]
    public float closeAreaEffect = 1.5F;
    public float mediumAreaEffect = 2.5F;
    public bool insideClose = false, insideMedium = false;

    [Header("Multiplier Settings")]
    public float closeMultiplier = 0;
    public bool multiplierEnabled; //This means this is the AOI that we are using at the minute (min dist or min K)
    public float currentMultiplier;
    public float distance;

    [Header("Gizmo Settings")]
    public bool adoptColourFromObject = true;
    public Color closeColour = Color.red;
    public Color mediumColour = Color.yellow;
    public bool drawWireframeWhenSelectedOnly = false;
    public bool showInGame = false;
    public GameObject visibleGizmo;

    [Header("-Object Spawn")]
    [Header("Child Object Settings")]
    public bool placeChild = false;

    public int numberOfObjects = 0;

    [Header("-Position")]
    public int[] childIndex;

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

    [Header("Misc Settings")]
    public float distanceHeading;
    public Vector3 direction;

    protected DJVR_Navigation navigationTechnique;
    protected Transform headsetTransform;
    protected GameObject targetHandLeft;
    protected GameObject targetHandRight;
    protected SteamVR_PlayArea steamVR_PlayArea;
    protected EventManager eventManager;

    private Color ObjectCloseColour = Color.black;
    private Color ObjectMediumColour = Color.black;
    private float defaultMultiplier = 0;
    private Vector3 distanceToTarget;
    private bool headsetTransformFound = false;

    private GameObject gizmoClose = null;
    private GameObject gizmoMedium = null;
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
    private Vector3 heading;
    private Vector3 headingNorm;
    private Vector3 custom;

    private CapsuleCollider capsuleCollider;

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
        heading,
        custom
    };

    #endregion Enums

    #region Methods

    // Use this for initialization
    private void Start()
    {
        transform.tag = "PointOfInterest";
        steamVR_PlayArea = GameObject.FindObjectOfType<SteamVR_PlayArea>();
        eventManager = FindObjectOfType<EventManager>();
        headsetTransform = VRTK.DeviceFinder.HeadsetTransform();

        if (headsetTransform)
        {
            headsetTransformFound = true;
        }
        else
        {
            Debug.Log("target object could not be found");
        }

        navigationTechnique = FindObjectOfType<DJVR_Navigation>();
        capsuleCollider = this.gameObject.GetComponent<CapsuleCollider>();
        capsuleCollider.isTrigger = true;
        capsuleCollider.radius = mediumAreaEffect;
        defaultMultiplier = navigationTechnique.defaultAdditionalMovementMultiplier;

        ShowGizmoInGame();

        if (placeChild)
        {
            SetBounds();
            PlaceChild();
        }

        if (adoptColourFromObject)
        {
            AdoptColours();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        DestroyAll();
        defaultMultiplier = navigationTechnique.defaultAdditionalMovementMultiplier;
    }

    private void CalculateMultiplier()
    {
        // Debug.Log("Target Position: " + headsetTransform.transform.position);

        distanceToTarget = headsetTransform.transform.position - transform.position;
        distanceToTarget.y = 0;
        distance = distanceToTarget.magnitude;

        //Debug.Log("Distance:" + distance + ": " + transform.name);

        float gradient = 0;
        float multiplierAdjusted = 0;

        if (distance <= closeAreaEffect)
        {
            insideClose = true;
            insideMedium = false;
            if (active)
            {
                if (multiplierEnabled)
                {
                    navigationTechnique.additionalMovementMultiplier = closeMultiplier;
                }
            }
        }
        else if (distance <= mediumAreaEffect)
        {
            insideMedium = true;
            insideClose = false;
            if (active)
            {
                gradient = (defaultMultiplier - closeMultiplier) / (mediumAreaEffect - closeAreaEffect);
                multiplierAdjusted = (gradient * (distance - mediumAreaEffect)) + defaultMultiplier;

                currentMultiplier = multiplierAdjusted;
                //If this AOI is the best candidate (min Dist or min K), update navigation technique with it. 
                if (multiplierEnabled)
                {
                    navigationTechnique.additionalMovementMultiplier = multiplierAdjusted;
                }
            }
        }
        else if (mediumAreaEffect < distance)
        {
            insideMedium = false;
            insideClose = false;

            currentMultiplier = 100;
            
        }
        //Debug.Log("Distance: " + distance);
        //Debug.Log("Multiplier Adjusted: " + multiplierAdjusted2);
    }

    private void OnTriggerStay(Collider other)
    {
        if (headsetTransformFound)
        {
            CalculateMultiplier();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (active)
        {
            navigationTechnique.additionalMovementMultiplier = defaultMultiplier;
        }
    }
    private void SetBounds()
    {
        center = transform.position;
        center.y = 0f;

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

        heading = transform.position - (realCenter);
        heading.y = 0f;

        distanceHeading = heading.magnitude;

        direction = heading / distanceHeading;

        direction.y = 0f;


    }

    private void PlaceChild()
    {
        for (int i = 0; i < childIndex.Length; i++)
        {
            int a = i * (360 / childIndex.Length);

        
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
                    placedVector[i] = RandomCircle(center, circleRadius, a) * directionMultiplier[i];
                    placedVector[i].y = placedVector[i].y + heightValue[i];
                    break;

                case PlacedDirection.angle:
                    placedVector[i] = RandomCircle(center, circleRadius, angle[i]) * directionMultiplier[i];
                    placedVector[i].y = placedVector[i].y + heightValue[i];
                    break;

                case PlacedDirection.heading:
                    placedVector[i] = heading * directionMultiplier[i];
                    placedVector[i] = placedVector[i] + direction / 2f;
                    placedVector[i].y = placedVector[i].y + heightValue[i];
                    break;

                case PlacedDirection.custom:
                    placedVector[i] = placedVector[i] * directionMultiplier[i];
                    break;

                default:
                    break;
            }

 
            
           

            if (transform.GetChild(childIndex[i]).tag != "NotSequence")
            {
                transform.GetChild(childIndex[i]).rotation = Quaternion.Euler(-90, 0, 0);
            }


            transform.GetChild(childIndex[i]).position = placedVector[i];


            if (i == 0)
            {
                eventManager.directionMultiplier = directionMultiplier[i];

            }
        }
    }

    private Vector3 RandomCircle(Vector3 center, float radius, int a)
    {
        //Debug.Log(a);
        float ang = a;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.y = center.y;
        return pos;
    }
    private void OnDestroy()
    {
        if (navigationTechnique != null)
        {
            navigationTechnique.additionalMovementMultiplier = navigationTechnique.defaultAdditionalMovementMultiplier;
            //Debug.Log("Does this work? YES IT DOES!! ");
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawWireframeWhenSelectedOnly)
        {
            DrawGizmos();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (drawWireframeWhenSelectedOnly)
        {
            DrawGizmos();
        }
    }

    private void DrawGizmos()
    {
        if (adoptColourFromObject)
        {
            Gizmos.color = ObjectCloseColour;
            //Gizmos.DrawWireSphere(transform.position, closeAreaEffect);
            DebugExtension.DrawCylinder(new Vector3(this.transform.position.x, 0, this.transform.position.z), new Vector3(this.transform.position.x, mediumAreaEffect * 1.5f, this.transform.position.z), ObjectCloseColour, closeAreaEffect);

            Gizmos.color = ObjectMediumColour;
            //Gizmos.DrawWireSphere(transform.position, mediumAreaEffect);
            DebugExtension.DrawCylinder(new Vector3(this.transform.position.x, 0, this.transform.position.z), new Vector3(this.transform.position.x, mediumAreaEffect * 1.5f, this.transform.position.z), ObjectMediumColour, mediumAreaEffect);
        }
        else
        {
            Gizmos.color = closeColour;
            //Gizmos.DrawWireSphere(transform.position, closeAreaEffect);
            DebugExtension.DrawCylinder(new Vector3(this.transform.position.x, 0, this.transform.position.z), new Vector3(this.transform.position.x, mediumAreaEffect * 1.5f, this.transform.position.z), closeColour, closeAreaEffect);
            Gizmos.color = mediumColour;
            //Gizmos.DrawWireSphere(transform.position, mediumAreaEffect);
            DebugExtension.DrawCylinder(new Vector3(this.transform.position.x, 0, this.transform.position.z), new Vector3(this.transform.position.x, mediumAreaEffect * 1.5f, this.transform.position.z), mediumColour, mediumAreaEffect);
        }
    }
    private void ShowGizmoInGame()
    {
        if (showInGame)
        {
            ObjectCloseColour = transform.GetComponentInChildren<Renderer>().material.color;
            gizmoClose = Instantiate(visibleGizmo, transform.position, Quaternion.Euler(0, 0, 0)) as GameObject;
            gizmoClose.transform.localScale = new Vector3(closeAreaEffect * 2, closeAreaEffect * 2, closeAreaEffect * 2);
            gizmoClose.GetComponent<Renderer>().material.color = ObjectCloseColour;
            gizmoClose.transform.parent = this.transform;

            ObjectMediumColour = transform.GetComponentInChildren<Renderer>().material.color;
            ObjectMediumColour.r = ObjectMediumColour.r - 0.5f;
            ObjectMediumColour.g = ObjectMediumColour.g - 0.5f;
            ObjectMediumColour.b = ObjectMediumColour.b - 0.5f;
            gizmoMedium = Instantiate(visibleGizmo, transform.position, Quaternion.Euler(0, 0, 0)) as GameObject;
            gizmoMedium.transform.localScale = new Vector3(mediumAreaEffect * 2, mediumAreaEffect * 2, mediumAreaEffect * 2);
            gizmoMedium.GetComponent<Renderer>().material.color = ObjectMediumColour;
            gizmoMedium.transform.parent = this.transform;
        }
    }

    private void AdoptColours()
    {
        ObjectCloseColour = transform.GetComponentInChildren<Renderer>().material.color;
        closeColour = ObjectCloseColour;

        ObjectMediumColour = transform.GetComponentInChildren<Renderer>().material.color;
        ObjectMediumColour.r = ObjectMediumColour.r - 0.5f;
        ObjectMediumColour.g = ObjectMediumColour.g - 0.5f;
        ObjectMediumColour.b = ObjectMediumColour.b - 0.5f;
        mediumColour = ObjectMediumColour;
    }
    private void DestroyAll()
    {
        if (eventManager.destroyAll && transform.GetChild(0).tag != "NotSequence")
        {
            gameObject.SetActive(false);
        }
    }

    private void OnValidate()
    {
        transform.tag = "PointOfInterest";

        if (childIndex.Length != numberOfObjects)
        {
            Debug.LogWarning("Don't resize the 'ints' field's array size!");
            Array.Resize(ref childIndex, numberOfObjects);
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

        if (angle.Length != numberOfObjects)
        {
            Debug.LogWarning("Don't resize the 'ints' field's array size!");
            Array.Resize(ref angle, numberOfObjects);
        }

        for (int i = 0; i < directionMultiplier.Length; i++)

        {
            if (directionMultiplier[i] == 0 && numberOfObjects != 0)
            {
                Debug.LogWarning("Direction Multiplier is 0, will center object! For: " + this.gameObject.transform.name);
            }
        }
    }

    #endregion Methods
}
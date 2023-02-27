using UnityEngine;
using Valve.VR;

public class DJVR_TriggerStart : MonoBehaviour
{
    #region Fields

    public GameObject[] placedObject;
    public Vector3[] placedVector;
    public PlacedDirection[] placedDirections;

    [Range(1, 10)]
    public float[] directionMultiplier;

    [Range(0, 100)]
    public float[] heightValue;

    public bool changeColour = false;
    public Color[] containerColor;

    protected SteamVR_PlayArea steamVR_PlayArea;

    private GameObject gameObject1 = null;
    private PlacedDirection placedDirection;
    private Vector3[] vertices;
    private HmdQuad_t steamVrBounds;
    private SteamVR_PlayArea.Size lastSize;

    private Vector3 north;
    private Vector3 south;
    private Vector3 east;
    private Vector3 west;
    private Vector3 northEast;
    private Vector3 southEast;
    private Vector3 northWest;
    private Vector3 southWest;
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
        custom
    };

    #endregion Enums

    #region Methods

    // Use this for initialization
    private void Start()
    {
        steamVR_PlayArea = GameObject.FindObjectOfType<SteamVR_PlayArea>();
        if (steamVR_PlayArea)
            Debug.Log("Base steamVR_PlayArea  object found: " + steamVR_PlayArea.name);
        else
            Debug.Log("No steamVR_PlayArea  object could be found");

        if (vertices == null || vertices.Length == 0)
        {
            bool success = SteamVR_PlayArea.GetBounds(steamVR_PlayArea.size, ref steamVrBounds);
            if (success)
            {
                lastSize = steamVR_PlayArea.size;
                StartTest();
            }
            else
            {
                Debug.LogWarning("Could not get the chaperon size. This may happen if you use the calibrated size.");
            }
        }
        //Debug.Log(north);
    }

    // Update is called once per frame
    private void Update()
    {
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
    private void CheckAndUpdateBounds()
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

    private void StartTest()
    {
        northEast = new Vector3(steamVrBounds.vCorners0.v0, 0, steamVrBounds.vCorners0.v2);
        southEast = new Vector3(steamVrBounds.vCorners1.v0, 0, steamVrBounds.vCorners1.v2);
        northWest = new Vector3(steamVrBounds.vCorners3.v0, 0, steamVrBounds.vCorners3.v2);
        southWest = new Vector3(steamVrBounds.vCorners2.v0, 0, steamVrBounds.vCorners2.v2);

        north = new Vector3(0, 0, steamVrBounds.vCorners0.v2);
        south = new Vector3(0, 0, steamVrBounds.vCorners1.v2);
        east = new Vector3(steamVrBounds.vCorners0.v0, 0, 0);
        west = new Vector3(steamVrBounds.vCorners2.v0, 0, 0);

        //Debug.Log(northEast);

        for (int i = 0; i < placedObject.Length; i++)
        {
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

                case PlacedDirection.custom:
                    placedVector[i] = placedVector[i] * directionMultiplier[i];
                    break;

                default:
                    break;
            }

            if (changeColour)
            {
                placedObject[i].GetComponent<Renderer>().material = new Material(Shader.Find("Valve/vr_standard"));
                placedObject[i].GetComponent<Renderer>().sharedMaterial.color = containerColor[i];
            }

            gameObject1 = Instantiate(placedObject[i], placedVector[i], Quaternion.Euler(0, 0, 0)) as GameObject;
        }
    }

    #endregion Methods
}
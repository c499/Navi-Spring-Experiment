using UnityEngine;
using System;

public class AdoptColour : MonoBehaviour
{
    #region Fields

    public bool showClose = true;
    public bool showMedium = false;

    public bool highlightClose = false;
    public bool highlightMedium = false;

    public Material projMaterial;
    public Shader projShader;

    public Transform circleSmall;
    public Transform circleMedium;

    public Transform cylinderSmall;
    public Transform cylinderMedium;

    private Vector3 parentPos;
    private Vector3 parentPosY;
    private Color closeColour;
    private Color mediumColour;

    private float ParentCloseZone;
    private float ParentMediumZone;

    #endregion Fields

    #region Methods

    // Use this for initialization
    private void Start()
    {
        getColour();
        circleSmall = transform.GetChild(0);
        circleMedium = transform.GetChild(1);
        cylinderSmall = transform.GetChild(2);
        cylinderMedium = transform.GetChild(3);

        projShader = Shader.Find("Projector/AdditiveTint");


        HighlightCloseCylinder();
        HighlightMediumCylinder();

        parentPosY = transform.root.position;
        parentPosY.y = 0.2f;

        DrawRadar circles = transform.GetComponent<DrawRadar>();

        if (showClose)
        {
            circleSmall.GetComponent<Projector>().material = new Material(Shader.Find("Projector/AdditiveTint"));

            circleSmall.GetComponent<Projector>().material.color = closeColour;

            circleSmall.GetComponent<Projector>().orthographicSize = ParentCloseZone * 1.25f;
            circleSmall.transform.position = parentPosY;
        }

        if (showMedium)
        {
            circleMedium.GetComponent<Projector>().material = new Material(Shader.Find("Projector/AdditiveTint"));
            circleMedium.GetComponent<Projector>().material.color = mediumColour;

            circleMedium.GetComponent<Projector>().orthographicSize = ParentMediumZone * 1.25f;
            circleMedium.transform.position = parentPosY;
        }
        //Debug.Log("Parent Colour: " + ParentColour);
    }

    // Update is called once per frame
    private void Update()
    {
        HighlightCloseCylinder();
        HighlightMediumCylinder();

        parentPos = transform.root.position;
        parentPos.y = 0.2f;
    }

    private void getColour()
    {
        closeColour = transform.GetComponentInParent<DJVR_Trigger_AOI>().closeColour;
        mediumColour = transform.GetComponentInParent<DJVR_Trigger_AOI>().mediumColour;

        ParentCloseZone = transform.GetComponentInParent<DJVR_Trigger_AOI>().closeAreaEffect;
        ParentMediumZone = transform.GetComponentInParent<DJVR_Trigger_AOI>().mediumAreaEffect;
    }

    private void HighlightCloseCylinder()
    {
        if (highlightClose && transform.GetComponentInParent<DJVR_Trigger_AOI>().insideClose)
        {
            cylinderSmall.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Valve/vr_standard");
            //cylinderSmall.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Valve/vr_standard"));
            cylinderSmall.GetComponent<Renderer>().sharedMaterial.color = closeColour;
            cylinderSmall.GetComponent<Renderer>().sharedMaterial.EnableKeyword("_EMISSION");
            cylinderSmall.GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", closeColour);

            Vector3 radiusToScale = new Vector3(ParentCloseZone * 8f, 0.1f, ParentCloseZone * 8f);
            cylinderSmall.GetComponent<Transform>().localScale = radiusToScale;
            parentPos.y = 0.25f;
            cylinderSmall.transform.position = parentPos;
            
        }
        else
        {
            Vector3 radiusToScale = new Vector3(0, 0, 0);
            cylinderSmall.GetComponent<Transform>().localScale = radiusToScale;
            parentPos.y = 0.25f;
            cylinderSmall.transform.position = parentPos;
        }
    }

    private void HighlightMediumCylinder()
    {
        if (highlightMedium && transform.GetComponentInParent<DJVR_Trigger_AOI>().insideMedium)
        {
            cylinderMedium.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Valve/vr_standard");
            //cylinderMedium.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Valve/vr_standard"));
            cylinderMedium.GetComponent<Renderer>().sharedMaterial.color = mediumColour;
            cylinderMedium.GetComponent<Renderer>().sharedMaterial.EnableKeyword("_EMISSION");
            cylinderMedium.GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", mediumColour);

            Vector3 radiusToScale = new Vector3(ParentMediumZone * 8f, 0.1f, ParentMediumZone * 8f);
            cylinderMedium.GetComponent<Transform>().localScale = radiusToScale;
            parentPos.y = 0.25f;
            cylinderMedium.transform.position = parentPos;
        }
        else
        {
            Vector3 radiusToScale = new Vector3(0, 0, 0);
            cylinderMedium.GetComponent<Transform>().localScale = radiusToScale;
            parentPos.y = 0.25f;
            cylinderMedium.transform.position = parentPos;
        }
    }

    #endregion Methods
}
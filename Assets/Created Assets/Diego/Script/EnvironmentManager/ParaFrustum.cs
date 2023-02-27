using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Created_Assets.Diego.Script;
using Assets.Created_Assets.Diego.Script.TaskManager.ManeuvreData;

public class ParaFrustum : MonoBehaviour {
    [Header("Parafrustum parameters")]
    public float posTolerance = 0.1f;
    public float angleTolerance = 20.0f;

    [Header("Prefab References")]
    public GameObject ParaFrustumBase;
    public GameObject TailPlane;
    public GameObject HeadCentre;
    public GameObject outPlane;
    public GameObject posPlane;
    public GameObject inPlane;
    public GameObject headCursor; //Little pointer at the centre of the user's sight to help aimming,
    public GameObject userHead; //Temporary (debugging)

    public enum PARAFRUSTUM_AREA { OUTSIDE, INSIDE_HEAD, INSIDE_BOTH};
    PARAFRUSTUM_AREA state;
    protected float cosAngleTolerance;
    // Use this for initialization
    void Start() {
        cosAngleTolerance = Mathf.Cos(angleTolerance * Mathf.Deg2Rad);
        setupParaFrustumFromParameters();
        headCursor.SetActive(false);
    }

    // Update is called once per frame
    // static int curPos = 0;
	void Update () {
        //LET's test the behaviour of our parafrustum
        float pErr = 0, aErr = 0;
        //checkInside(ref pErr, ref aErr);
        //EnvironmentManager.instance().setParafrustum(PrecisionLevel.COARSE, PrecisionLevel.COARSE, (curPos++)%6);//Quick test on the orientations achieved..
        //_updateRadius(0.19f);
        //_updateVisuals(PARAFRUSTUM_AREA.INSIDE_BOTH);

    }

    public void setupParaFrustumFromParameters()
    {
        //Set to visualize from the outside 
        state = PARAFRUSTUM_AREA.OUTSIDE;
        //Radius assuming observer at the centre of the head zone (0.5m):
        float radius = Mathf.Tan(Mathf.Deg2Rad * angleTolerance) * (0.5f);
        cosAngleTolerance = Mathf.Cos(angleTolerance * Mathf.Deg2Rad);
        _updateRadius(radius);
        _updateVisuals(state);
        // Setup eye sphere sizes according to posTolerance;
        Transform[] children = HeadCentre.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
            if (child.name == "SphereLeft" || child.name == "SphereRight" )
                child.transform.localScale = new Vector3(posTolerance,posTolerance,posTolerance);

    }

    protected void _updateVisuals(PARAFRUSTUM_AREA area ) {
        switch (area) {
            case PARAFRUSTUM_AREA.OUTSIDE:
                outPlane.SetActive(true);
                posPlane.SetActive(false);
                inPlane.SetActive(false);
                break;
            case PARAFRUSTUM_AREA.INSIDE_HEAD:
                outPlane.SetActive(false);
                posPlane.SetActive(true);
                inPlane.SetActive(false);
                break;
            case PARAFRUSTUM_AREA.INSIDE_BOTH:
                outPlane.SetActive(false);
                posPlane.SetActive(false);
                inPlane.SetActive(true);
                break;
        }
    }

    protected void _updateRadius(float radiusInMeters)
    {
        float sizeInPlaneUnits = 2*radiusInMeters / 10; //Planes in unity are 10x10meters. Scale down to Plane's units. (Size=2*radius)
        outPlane.transform.localScale= new Vector3(sizeInPlaneUnits, sizeInPlaneUnits, sizeInPlaneUnits);
        posPlane.transform.localScale = new Vector3(sizeInPlaneUnits, sizeInPlaneUnits, sizeInPlaneUnits);
        inPlane.transform.localScale = new Vector3(sizeInPlaneUnits, sizeInPlaneUnits, sizeInPlaneUnits);
    }

    public void showHeadCursor(bool show) {
        headCursor.SetActive(show);
    }

    public bool checkInside(ref float posError, ref float angError) {
        Transform headVirtual= userHead.transform;

        Matrix4x4 fromPlayerHeadToParafrustumHead=HeadCentre.transform.worldToLocalMatrix*headVirtual.localToWorldMatrix;
        //A. Compute position and orientation error. 
        Vector4 position = fromPlayerHeadToParafrustumHead * new Vector4(0, 0, 0, 1);
        float distanceSquare = position.x * position.x + position.y * position.y + position.z * position.z;
        posError = Mathf.Sqrt(distanceSquare);
        //Now let's get the orientation error
        Vector3 targetToUserHeadInVRCoords = headVirtual.position - TailPlane.transform.position;
        float targetToUserDistance = targetToUserHeadInVRCoords.magnitude; //I need it later (size of ring and move head cursor)
        Vector3 userLookAtVectorInVRCoords = headVirtual.localToWorldMatrix * (new Vector3(0, 0, -1));
        userLookAtVectorInVRCoords.Normalize();
        targetToUserHeadInVRCoords.Normalize();
        float cosAngle = Vector3.Dot(userLookAtVectorInVRCoords, targetToUserHeadInVRCoords);
        if (cosAngle >= 1.0f)//Believe it or not, it was failing when cosAngle=1.
            cosAngle = 0.999f;
        angError = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;

        /*if (float.IsNaN(angError))
            checkInside(ref posError, ref angError);*/
        //A. Check if user's head is inside the positioning area.
        //EnvironmentManager.instance().centralText("DISTANCE:" + Mathf.Sqrt(distanceSquare));
        if (posError < posTolerance/2)//posTolerance is the diameter of the sphere-> dist should be smaller than radius (posTolerance/2)
        {           
            //EnvironmentManager.instance().centralText("INSIDE POS:" + posError);
            state = PARAFRUSTUM_AREA.INSIDE_HEAD;
            _updateVisuals(state);
            headCursor.SetActive(true);
        }
        else
        {
            //EnvironmentManager.instance().centralText("OUTSIDE POS:" + posError);
            state = PARAFRUSTUM_AREA.OUTSIDE;
            _updateVisuals(state);
            headCursor.SetActive(false);
            return false;
        }
        //B. Recompute the size of the head planes:
        //B.1. Orient the plane towards the user.
        TailPlane.transform.LookAt(headVirtual);
        //B.2. Recompute radius according to position
        headCursor.transform.localPosition=new Vector3(0,0, targetToUserDistance);
        float radius = Mathf.Tan(Mathf.Deg2Rad*angleTolerance)* (targetToUserDistance);
        _updateRadius(radius);
        //C. Check if orientation is inside the range
        /*EnvironmentManager.instance().centralText("IN_POS: D=" + Mathf.Sqrt(distanceSquare)
                                               + "\n radius=" + radius.ToString("F4")
                                               + "\n cos="+cosAngle+"; tolerance=" + cosAngleTolerance
                                               +"\n    A="+angError);*/
        if (cosAngle >= cosAngleTolerance) {
            //C.1. We are inside! Let's update visual feedback and return true!
            state = PARAFRUSTUM_AREA.INSIDE_BOTH;
            _updateVisuals(state);
            /*EnvironmentManager.instance().centralText("GOOD!: D=" + posError
                                              + "\n radius=" + radius.ToString("F4")
                                              + "\n cos=" + cosAngle + "; tolerance=" + cosAngleTolerance
                                              + "\n    A=" + angError);*/
            return true;
        }
        return false;

    }
}

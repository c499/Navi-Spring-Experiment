using UnityEngine;
/**
    This class searches for the AOI matching the selected criteria (min Dist or min K) and marks it as selected (multiplierEnabled)
     
     */
public class DJVR_Trigger_AOI_Scanner : MonoBehaviour
{
    #region Fields

    [Header("Multiplier Calculation Settings")]
    public bool multiplierMinDistance = false;
    public bool multiplierMinK = true;
    private float minDist = 5;
    private GameObject[] AreasOfInterest;
    private GameObject AOIActive;

    #endregion Fields

    #region Methods

    // Use this for initialization
    private void Start()
    {
        if (multiplierMinDistance && multiplierMinK)
        {
            Debug.LogError("Both multiplierMinDistance and multiplierMinK are selected, please select one ");
            multiplierMinDistance = false;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        CheckDistance();
    }

    private void CheckDistance()
    {
        AreasOfInterest = GameObject.FindGameObjectsWithTag("PointOfInterest");
        //Search all areas of interest and keep the one matching criteria (minDist or minK)
        foreach (GameObject AOI in AreasOfInterest)
        {
            //If there is no candidate AOI yet, take this (the first)
            if (!AOIActive)
            {
                AOIActive = AOI;
            }

            if (multiplierMinDistance)
            {
                if (Vector3.Distance(transform.position, AOI.transform.position) <= Vector3.Distance(transform.position, AOIActive.transform.position))
                {
                    AOIActive = AOI;
                    AOIActive.GetComponent<DJVR_Trigger_AOI>().multiplierEnabled = true;
                }
                else
                {
                    AOI.GetComponent<DJVR_Trigger_AOI>().multiplierEnabled = false;
                }
            }
            //Criteria proposed in paper:
            if (multiplierMinK)
            {
                if (AOI.GetComponent<DJVR_Trigger_AOI>().currentMultiplier <= AOIActive.GetComponent<DJVR_Trigger_AOI>().currentMultiplier)
                {   //Take this as candidate and mark it as the currently selected one. 
                    AOIActive = AOI;
                    AOIActive.GetComponent<DJVR_Trigger_AOI>().multiplierEnabled = true;
                }
                else
                {
                    AOI.GetComponent<DJVR_Trigger_AOI>().multiplierEnabled = false;
                }
            }
        }
    }

    #endregion Methods
}
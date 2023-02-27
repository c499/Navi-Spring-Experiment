using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaOfInterest : MonoBehaviour {
    [Header("Multiplier Distance Settings")]
    public bool NAVIFIELD_ENABLED = true;
    public float innerRadius = 0.5F;
    public float maxRadius = 1.0F;
    public float innerMultiplier = 1;
    public float maxMultiplier = 1;
    [Header("DEBUG: Real Time Values")]
    public float distance = 0;
    public float k = 0;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //Returns the M value to use, according to head position.
   public float getKValue(Vector3 headPositionInWorld) {
        if (!NAVIFIELD_ENABLED)
            return maxMultiplier; // We are using homogeneous technique. We return the constant scaling factor

        //NAVIFIELD_ENABLED
        //A. Compute distance to local (0,0,0).
        headPositionInWorld.y = 0;
        Vector3 floorPosition = this.transform.position; floorPosition.y = 0;
        distance = (floorPosition - headPositionInWorld).magnitude;//headsetPosition.position;
                                                                   //distance = (this.transform.position - headPositionInWorld).magnitude;//headsetPosition.position;
        //B. Compute K factor
        if (distance < innerRadius)
            return (k=innerMultiplier);
        else if (distance < maxRadius) {
            return (k = innerMultiplier +(maxMultiplier-innerMultiplier)*(distance - innerRadius) / (maxRadius - innerRadius));
        }
        return (k = maxMultiplier); 
    }
}

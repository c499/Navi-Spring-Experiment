using UnityEngine;
using System.Collections;

public class BallReturn : MonoBehaviour {

    public GameObject obj;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
         obj = ObjectPoolerScriptDupe.current.GetPooledObject();

        Vector3 above = new Vector3(transform.position.x, transform.position.y + 0.8f, transform.position.z);


        if (obj == null)
        {
            return;
        }

        obj.transform.position = above;
        obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        obj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        obj.transform.rotation = transform.rotation;
        obj.SetActive(true);



    }
}

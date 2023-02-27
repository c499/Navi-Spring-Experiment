using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour {
    [Header("Central Text Areas")]
    public Text frontText;
    public Text backText;
    // Use this for initialization
    void Start () {
        frontText.text="try...";
        backText.text = "try...";
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

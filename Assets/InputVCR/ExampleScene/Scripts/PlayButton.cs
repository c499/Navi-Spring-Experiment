/* PlayButton.cs
 * Copyright Eddie Cameron 2012 (See readme for licence)
 * ----------------------------
 */ 

using UnityEngine;
using System.Collections;

public class PlayButton : MonoBehaviour 
{
	public InputVCR playerVCR;
	
	public RecordButton recordButton;
	
	private bool isRecording;
	private Vector3 recordingStartPos;
	private Quaternion recordingStartRot;
	
	private bool isPlaying;
	private InputVCR curPlayer;

	
	void Awake()
	{
		
	}
	
	public void StartRecording()
	{
		if ( isRecording )
			playerVCR.Stop ();
		else
		{
			//recordingStartPos = playerVCR.transform.position;
			//recordingStartRot = playerVCR.transform.rotation;
			playerVCR.NewRecording ();
		}
		
		isRecording = !isRecording;
	}
	
	void Update()
	{
		if ( Input.GetKeyDown ( KeyCode.P ) )
			StartPlay ();
	}
	
	
	private void StartPlay()	
	{
		if ( isPlaying )
		{
			// pause
			isPlaying = false;
	
		}
		else if ( curPlayer != null )
		{
			// unpause


			isPlaying = true;
		}
		else
		{
			// try to start new playback
			if ( isRecording )
				recordButton.Record ();

			
		
		}
	}
	
	
}

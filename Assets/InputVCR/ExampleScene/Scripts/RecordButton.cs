/* RecordButton.cs
 * Copyright Eddie Cameron 2012 (See readme for licence)
 * ----------------------------
 */ 

using UnityEngine;
using System.Collections;

public class RecordButton : MonoBehaviour 
{
	public PlayButton playButton;
	
	void Update()
	{
		if ( Input.GetKeyDown ( KeyCode.R ) )
			Record ();
	}
	
	public void Record()
	{
		playButton.StartRecording();
        
	}
}

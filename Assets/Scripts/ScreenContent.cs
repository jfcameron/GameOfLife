//
// Name: ScreenContent
// Description: A quick fix to objects not updating on the controller screen
//
using UnityEngine;
using System.Collections;

public class ScreenContent : MonoBehaviour 
{
	void Start () 
	{
		gameObject.SetActive (false);
		gameObject.SetActive (true);
	
	}

}

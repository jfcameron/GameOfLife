//
// Name: PauseState
// Description: GUI element that presents the pause state
//
using UnityEngine;
using System.Collections;

public class PauseState : MonoBehaviour
{
	//**********
	// Constants
	//**********
	private static string c_ResumeName = "[RESUME]";
	private static string c_PauseName  = "[PAUSE]" ;

	//*************
	// Data members
	//*************
	[SerializeField] private GameOfLife m_GameOfLife = null;

	private TextMesh m_TextMesh = null;

	//
	// Unity interface
	//
	private void Awake()
	{
		m_TextMesh = GetComponent<TextMesh> ();

	}

	private void FixedUpdate()
	{
		if (m_GameOfLife.a_SimulationOn)
			m_TextMesh.text = c_PauseName;
		else
			m_TextMesh.text = c_ResumeName;

	}

}

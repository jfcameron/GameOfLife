//
// Name: StepSize
// Description: GUI element that presents the step size of game of life sim
//
using UnityEngine;
using System.Collections;

public class StepSize : MonoBehaviour 
{
	//
	//
	//
	private static string c_FastName   = "Fast";
	private static string c_MediumName = "Medium";
	private static string c_SlowName   = "Slow";


	//
	//
	//
	[SerializeField] private GameOfLife m_GameOfLife = null;
	private TextMesh m_TextMesh = null;

	//****************
	// Unity interface
	//****************
	private void Awake()
	{
		m_TextMesh = GetComponent<TextMesh> ();

	}

	private void FixedUpdate()
	{
		if (m_GameOfLife.a_SimulationStepInterval == 0)
			m_TextMesh.text = c_FastName;
		else if (m_GameOfLife.a_SimulationStepInterval == 15)
			m_TextMesh.text = c_MediumName;
		else
			m_TextMesh.text = c_SlowName;

	}

}







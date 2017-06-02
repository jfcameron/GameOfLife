//
// Name: GameOfLife
// Description: Manages compute shader program of the same name
//
using UnityEngine;
using System.Collections;

public class GameOfLife : MonoBehaviour 
{
	//**********
	// Constants
	//**********
	//Uniform names
	private static string c_InputTextureUniformName  = "_Input";
	private static string c_OutputTextureUniformName = "_Output";
	private static string c_TexelCoordinate          = "_TexelCoordinate";
	private static string c_InputDown                = "_InputDown";

	//Kernel names
	private static string c_MainKernelName            = "Main";
	private static string c_UserInputKernelName       = "UserInput";
	private static string c_CopyColorBufferKernelName = "CopyColorBuffer";
	private static string c_ClearBuffersKernelName    = "ClearBuffers";
	private static string c_GenGosperKernelName       = "GenGosper";

	//Dispatch sizes for certain kernel dispatch calls
	private static int    c_DispatchSizeX = 38;
	private static int    c_DispatchSizeY = 38;

	//Gameobject names
	private static string c_ComputerScreenName       = "ComputerScreen";
	private static string c_MainScreenName           = "MainScreen";
	private static string c_ControllerComputerScreen = "ControllerComputerScreen";

	//Simulation speeds
	private static int c_FastSim   = 0;
	private static int c_MediumSim = 15;
	private static int c_SlowSim   = 30;
	
	//Game FPS
	private static float c_60FPS = 1 / 60f;

	//*************
	// Data members
	//*************
	//Data initialized from the inspector
	[SerializeField] private Camera        m_Camera;
	[SerializeField] private ComputeShader m_GameOfLifeProgram;
	[SerializeField] private RenderTexture m_RenderTexture1;
	[SerializeField] private RenderTexture m_RenderTexture2;

	private bool m_BufferFlop          = false;
	private bool m_SimulationOn        = false;
	private int  m_SimulationStepTimer = 0;
	private int  m_SimulationInterval  = c_FastSim;

	//**********
	// Accessors
	//**********
	public bool a_SimulationOn {get{return m_SimulationOn;} set{m_SimulationOn = value;}}
	public int  a_SimulationStepInterval { get { return m_SimulationInterval; } set { m_SimulationInterval = value; } }

	//****************
	// Unity interface
	//****************
	private void Start()
	{
		//Set interval
		Time.fixedDeltaTime = c_60FPS;

		//Init render textures
		m_RenderTexture2.Release();
		m_RenderTexture2.enableRandomWrite = true;
		m_RenderTexture2.format = RenderTextureFormat.RInt;
		m_RenderTexture2.Create();

		m_RenderTexture1.Release();
		m_RenderTexture1.enableRandomWrite = true;
		m_RenderTexture1.format = RenderTextureFormat.RInt;
		m_RenderTexture1.Create();

		//Init main kernel uniforms
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_MainKernelName),c_InputTextureUniformName ,m_RenderTexture1);
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_MainKernelName),c_OutputTextureUniformName,m_RenderTexture2);

		//Init userinput kernel uniforms
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_UserInputKernelName),c_InputTextureUniformName ,m_RenderTexture1);
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_UserInputKernelName),c_OutputTextureUniformName,m_RenderTexture2);

		//Init color
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_CopyColorBufferKernelName),c_InputTextureUniformName ,m_RenderTexture1);
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_CopyColorBufferKernelName),c_OutputTextureUniformName,m_RenderTexture2);

		//Clear buffers
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_ClearBuffersKernelName),c_InputTextureUniformName ,m_RenderTexture1);
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_ClearBuffersKernelName),c_OutputTextureUniformName,m_RenderTexture2);

		//Gosper
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_GenGosperKernelName),c_InputTextureUniformName ,m_RenderTexture1);
		m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_GenGosperKernelName),c_OutputTextureUniformName,m_RenderTexture2);

	}

	private void FixedUpdate()
	{
		handleUserInput();
		dispatchComputeKernel();

	}

	//
	//
	//
	private void handleUserInput()
	{
		if (Input.GetKeyDown(KeyCode.Return))
			m_SimulationOn = !m_SimulationOn;

		//Check for button down
		if (!Input.GetMouseButton(0))
		{
			m_GameOfLifeProgram.SetInt(c_InputDown,0);
			return;

		}

		//Check for ray collision
		RaycastHit hit;
		Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
		
		if (!Physics.Raycast(ray, out hit)) 
			return;

		Vector3 localPoint = hit.transform.worldToLocalMatrix.MultiplyPoint(hit.point);
		localPoint += new Vector3(0.5f,0.5f,0);//Moves 0,0 to bottom left from center

		//Simulation case
		if (hit.transform.name == c_ComputerScreenName || hit.transform.name == c_MainScreenName)
		{
			//Convert to UV coordinate
			Vector2 uv = (Vector2)localPoint;
			
			//Convert to thread coordinate
			uv *= 38f;
			
			//pass uv to compute shader
			m_GameOfLifeProgram.SetVector(c_TexelCoordinate,uv);
			m_GameOfLifeProgram.SetInt(c_InputDown,1);

		}
		//Controller case
		else if (hit.transform.name == c_ControllerComputerScreen)
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (localPoint.y >= 0.9f)
					m_SimulationOn = !m_SimulationOn;
				else if (localPoint.y >= 0.7f)
					m_GameOfLifeProgram.Dispatch  (m_GameOfLifeProgram.FindKernel(c_ClearBuffersKernelName),c_DispatchSizeX,c_DispatchSizeY,1);
				else if (localPoint.y >= 0.5f)
				{
					if (m_SimulationInterval == c_FastSim)
						m_SimulationInterval = c_MediumSim;

					else if (m_SimulationInterval == c_MediumSim)
						m_SimulationInterval = c_SlowSim;

					else if (m_SimulationInterval == c_SlowSim)
						m_SimulationInterval = c_FastSim;

				}
				else if (localPoint.y >= 0.2f)
				{
					m_GameOfLifeProgram.Dispatch  (m_GameOfLifeProgram.FindKernel(c_GenGosperKernelName),c_DispatchSizeX,c_DispatchSizeY,1);

				}

			}



		}

	}

	private void dispatchComputeKernel()
	{
		if (m_BufferFlop == true)
		{
			//Init main kernel uniforms
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_MainKernelName),c_InputTextureUniformName ,m_RenderTexture1);
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_MainKernelName),c_OutputTextureUniformName,m_RenderTexture2);
			
			//Init userinput kernel uniforms
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_UserInputKernelName),c_InputTextureUniformName ,m_RenderTexture1);
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_UserInputKernelName),c_OutputTextureUniformName,m_RenderTexture2);

			//Init color
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_CopyColorBufferKernelName),c_InputTextureUniformName ,m_RenderTexture1);
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_CopyColorBufferKernelName),c_OutputTextureUniformName,m_RenderTexture2);

		}
		else
		{
			//Init main kernel uniforms
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_MainKernelName),c_InputTextureUniformName ,m_RenderTexture2);
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_MainKernelName),c_OutputTextureUniformName,m_RenderTexture1);
			
			//Init userinput kernel uniforms
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_UserInputKernelName),c_InputTextureUniformName ,m_RenderTexture2);
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_UserInputKernelName),c_OutputTextureUniformName,m_RenderTexture1);

			//Init color
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_CopyColorBufferKernelName),c_InputTextureUniformName ,m_RenderTexture2);
			m_GameOfLifeProgram.SetTexture(m_GameOfLifeProgram.FindKernel(c_CopyColorBufferKernelName),c_OutputTextureUniformName,m_RenderTexture1);
			
		}

		m_BufferFlop = !m_BufferFlop;

		if (m_SimulationOn == true)
			if (m_SimulationStepTimer++ > m_SimulationInterval)
			{
				m_GameOfLifeProgram.Dispatch  (m_GameOfLifeProgram.FindKernel(c_MainKernelName),c_DispatchSizeX,c_DispatchSizeY,1);
				m_SimulationStepTimer = 0;
			}

		m_GameOfLifeProgram.Dispatch  (m_GameOfLifeProgram.FindKernel(c_UserInputKernelName),c_DispatchSizeX,c_DispatchSizeY,1);
		m_GameOfLifeProgram.Dispatch  (m_GameOfLifeProgram.FindKernel(c_CopyColorBufferKernelName),c_DispatchSizeX,c_DispatchSizeY,1);

	}

}



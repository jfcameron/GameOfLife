//
// Name: PlayerControl
// Description: Basic first person perspective controls
//
using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour 
{
	//**********
	// Constants
	//**********
	private static string c_HorizontalAxisName = "Horizontal";
	private static string c_VerticalAxisName = "Vertical";
	private static float c_Speed = 0.2f;


	//****************
	// Unity interface
	//****************
	private void FixedUpdate()
	{
		if (Input.GetKey(KeyCode.W))
			transform.parent.Translate(Vector3.forward*c_Speed,Space.Self);

		if (Input.GetKey(KeyCode.S))
			transform.parent.Translate(-Vector3.forward*c_Speed,Space.Self);

		if (Input.GetKey(KeyCode.A))
			transform.parent.Translate(-Vector3.right*c_Speed,Space.Self);

		if (Input.GetKey(KeyCode.D))
			transform.parent.Translate(Vector3.right*c_Speed,Space.Self);

		if (Input.GetMouseButton(1))
		{
			transform.parent.Rotate(Vector3.up*Input.GetAxis(c_HorizontalAxisName)*c_Speed,Space.Self);

			transform.Rotate(Vector3.left*Input.GetAxis(c_VerticalAxisName)*c_Speed,Space.Self);
			//transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y,0));

		}

	}

}





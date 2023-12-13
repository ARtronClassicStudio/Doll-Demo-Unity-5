using UnityEngine;
using System.Collections;
using System;

public class InputManager  : MonoBehaviour
{
	// A delegate type for hooking up change notifications.
	public delegate void InputEventHandler(object sender, EventArgs e);

	// An event that clients can use to be notified whenever the elements of the list change.
	public static event InputEventHandler Tapped;
	public static event InputEventHandler DoubleTapped;
	public static event InputEventHandler Pinched;

	public float lastMouseButtonTime = -1;
	bool tapping = false;
	int taps = 0;
	float firstMagnitude = 0;
	float testPinch = 1f;
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();

		testPinch += Input.GetAxis("Mouse ScrollWheel") * 0.1f;
		testPinch = Mathf.Clamp(testPinch, 0, 2);
		if (!Mathf.Approximately(testPinch, 1.0f))
			Pinched(this,new InputEventArgs(testPinch));

		if(Input.touchCount >= 2)
		{
			Touch touch1 = Input.GetTouch(0);
			Touch touch2 = Input.GetTouch(1);
			float magnitude = (touch1.position - touch2.position).magnitude;
			if(touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
			{
				firstMagnitude = magnitude;
			}
			else
			{
				Pinched(this,new InputEventArgs(magnitude / firstMagnitude));
			}
		}
		else
		{
			if (Input.GetMouseButtonDown (0))
			{
				if(!tapping)
				{
					lastMouseButtonTime = Time.time;
					tapping = true;
					taps = 0;
				}
			}
			if (tapping && Input.GetMouseButtonUp (0))
			{
				taps ++;
			}
			if(tapping && (Time.time - lastMouseButtonTime > 0.2f))
			{
				if (taps == 1)
					if(Tapped != null)
						Tapped(this, EventArgs.Empty);
				if(taps > 1)
					if(DoubleTapped != null)
						DoubleTapped(this, EventArgs.Empty);
				tapping = false;
			}
		}
	}
}
public class InputEventArgs : EventArgs
{
	public InputEventArgs(float delta)
	{
		pinchDelta = delta;
	}
	public float pinchDelta { get; set; }
}


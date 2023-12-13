using UnityEngine;
using System.Collections;
using System;

public class Orbit : MonoBehaviour {

	public float xSpeed = .3f;
	public float xEaseIn = .9f;
	public float xEaseOut = .1f;
	public float targetWidth = 2f;
	public float targetOffset = 2f;

	float x = 0.0f;
	float y = 0.0f;
	float origX;

	// Use this for initialization
	void Start () {
		Input.simulateMouseWithTouches = true;
		Vector3 angles = transform.eulerAngles;
		x = angles.y; origX = x;
		y = angles.x;
		
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;

		//InputManager.DoubleTapped += ZoomSwap;
		InputManager.Pinched += PinchZoom;

	}
	float EvalMaxFOV()
	{
		var d = Environment.GetActiveCamera().transform.position - transform.position;
		d.y = 0;
		var matchedView = Mathf.Atan2(targetWidth, d.magnitude);
		return Mathf.Rad2Deg * matchedView;
	}

	void PinchZoom(object ssender, EventArgs ea)
	{
		InputEventArgs e = (InputEventArgs)ea;
		Environment.GetActiveCamera().fieldOfView /= e.pinchDelta;
		Environment.GetActiveCamera().fieldOfView = Mathf.Clamp(Environment.GetActiveCamera().fieldOfView, EvalMaxFOV(), 60);
	}
	void ZoomSwap(object sender, EventArgs e)
	{

		if (Environment.GetActiveCamera().fieldOfView > 50)
			Environment.GetActiveCamera().fieldOfView /= 2;
		else
			Environment.GetActiveCamera().fieldOfView *= 2;
	}
	
	Vector2 lastMousePosition;
	public Transform target;
	void LateUpdate ()
	{
		var zoomFactor = Mathf.InverseLerp(EvalMaxFOV(), 60, Environment.GetActiveCamera().fieldOfView);
		var d = Environment.GetActiveCamera().transform.position - transform.position;
		d.y = -targetOffset ;
		Vector3 x = Environment.GetActiveCamera().transform.position + Vector3.Lerp(-d, Environment.GetActiveCamera().transform.forward, zoomFactor);
		Environment.GetActiveCamera().transform.LookAt(x);
	}
	bool dragging = false;

	void FixedUpdate ()
	{
		if(Input.touchCount >= 2)
		{
			dragging = false;
			return;
		}

		Vector2 position = Input.mousePosition * 2.0f;
		foreach (var t in Input.touches)
		{
			if (t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled)
				position = t.position;
		}

		if (Input.GetMouseButtonDown (0) || (Input.GetMouseButton(0) && !dragging))
		{
			dragging = true;
			lastMousePosition = position;
		}
		else if (Input.GetMouseButton (0) && dragging)
		{
			var delta = position - lastMousePosition;

			x = Mathf.Lerp (x, x + delta.x * (-xSpeed), xEaseIn);
		}
		else 
		{
			dragging = false;
			if (x - origX > 180F)
				x -= 360F;
			if (x - origX < -180F)
				x += 360F;
			x = Mathf.Lerp (x, origX, xEaseOut);		}

		x %= 360.0f;
		Quaternion rotation = Quaternion.Euler(y, x, 0);
		transform.rotation = rotation;

		lastMousePosition = position;
	}
}

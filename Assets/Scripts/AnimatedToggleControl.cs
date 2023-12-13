using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (Toggle))]
public class AnimatedToggleControl : MonoBehaviour {

	public string isOnBoolName = "Selected";
	private int isOnBoolId;
	private bool previousIsOn;
	private Animator animator;
	private Toggle toggle;

	void Start () {
		isOnBoolId = Animator.StringToHash (isOnBoolName);
		animator = GetComponent<Animator> ();
		toggle = GetComponent<Toggle> ();
		UpdateState ();
	}

	void Update () {
		if (toggle.isOn != previousIsOn)
			UpdateState();
	}

	void UpdateState()
	{
		previousIsOn = toggle.isOn;
		animator.SetBool (isOnBoolId, toggle.isOn);
	}
}

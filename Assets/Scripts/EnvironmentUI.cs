using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (ToggleGroup))]
public class EnvironmentUI : MonoBehaviour {

	public Toggle environmentTogglePrefab;
	public GameObject environmentsParent;

	void Start () {
		Environment[] environments = environmentsParent.GetComponentsInChildren<Environment> ();
		ToggleGroup group = GetComponent<ToggleGroup> ();

		for (int i = 0; i < environments.Length; i++)
		{
			Environment environment = environments[i];

			Toggle t = Instantiate<Toggle>(environmentTogglePrefab);
			t.transform.parent = transform;
			t.transform.localPosition = Vector3.zero;
			t.transform.localScale = Vector3.one;
			t.transform.localRotation = Quaternion.identity;
			t.isOn = environment.activateOnAwake;
			t.onValueChanged.AddListener((b)=> {if (b) environment.Activate();});
			t.group = group;

			Material material = t.GetComponentInChildren<MeshRenderer>().material;
			material.SetTexture ("_Cube", environment.specCube);
			material.SetFloat("_Bias", -2.5f);
		}
	}
}
using UnityEngine;
using UnityEditor;
using System;
[CustomEditor(typeof(Environment))]
public class EnvironmentInspector : Editor
{
	public void OnEnable() 
	{
	}
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();
		
		Environment env = target as Environment;

		env.Activate();
	}	
}

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[ExecuteInEditMode]
public class Environment : MonoBehaviour
{
	public bool activateOnAwake = false;
	public Cubemap skyCube = null;
	public Cubemap specCube = null;
	public float camHDRExposure = 1.0f;
	
	public static Environment main = null;
	
	//	Skybox material, allocated only if requested.
	private Material _skyboxMaterial = null;
	private Material skyboxMaterial
	{
		get
		{
			if( _skyboxMaterial == null )
			{		
				Shader shader = Shader.Find( "Skybox/Cubemap" );
				if( shader )
				{
					_skyboxMaterial = new Material( shader );
					_skyboxMaterial.name = "Skybox";
				}
				else
					Debug.LogError( "Couldn't find " + shader.name + " shader" );
			}
			return _skyboxMaterial;
		}
	}

	void Awake()
	{
		if (activateOnAwake)
			Activate();
	}
	
	private void SetLights( bool enabled )
	{
		Light[] lights = GetComponentsInChildren<Light>();
		foreach( Light light in lights )
			light.enabled = enabled;
	}
	
	public void Deactivate()
	{
		SetLights( false );
	}
	
	private Environment[] CollectAllEnvironments()
	{
		return GameObject.FindObjectsOfType<Environment>();
	}

	void Update ()
	{
		if (main != this)
			return;
	}
	
	public void Activate()
	{
		#if UNITY_EDITOR
		var selectedGO = UnityEditor.Selection.activeGameObject;
		if (selectedGO)
		{
			var selectedEnvironment = selectedGO.GetComponent<Environment>();
			if (selectedEnvironment != null && selectedEnvironment != this)
				return;
		}
		#endif
		
		var envs = CollectAllEnvironments();
		foreach (var env in envs)
			if (env != this)
				env.Deactivate();

		SetupGraphicsParameters ();
	}

	public static Camera GetActiveCamera()
	{
		Camera[] cameras = FindObjectsOfType<Camera>();
		foreach(Camera camera in cameras)
		{
			if(camera.enabled)
			{
				return camera;
			}
		}
		return null;
	}
	private void SetupGraphicsParameters ()
	{
		Camera cam = GetActiveCamera();
		if (cam)
		{
			var toneMap = cam.GetComponent<Tonemapping> ();
			if (toneMap && toneMap.enabled)
				toneMap.exposureAdjustment = camHDRExposure;
		}

		RenderSettings.skybox = skyboxMaterial;
		if (skyCube)
		{
			skyboxMaterial.SetTexture ("_Tex", skyCube);
		}
		
		if (specCube)
		{
			RenderSettings.customReflection = specCube;
			RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;

			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
			RenderSettings.ambientProbe = ProjectCubeIntoSH3 (specCube, 4 /*mip level*/);
		}

		SetLights( true );

		main = this;
		
	}

	public UnityEngine.Rendering.SphericalHarmonics3 ProjectCubeIntoSH3 (Cubemap src, int miplevel) 
	{
		// algorithm from Stupid Spherical Harmonics Tricks
		// http://www.ppsloan.org/publications/StupidSH36.pdf
		
		Vector3[] kCubemapOrthoBases = new Vector3[] {
			new Vector3( 0, 0,-1), new Vector3( 0,-1, 0), new Vector3(-1, 0, 0),
			new Vector3( 0, 0, 1), new Vector3( 0,-1, 0), new Vector3( 1, 0, 0),
			new Vector3( 1, 0, 0), new Vector3( 0, 0, 1), new Vector3( 0,-1, 0),
			new Vector3( 1, 0, 0), new Vector3( 0, 0,-1), new Vector3( 0, 1, 0),
			new Vector3( 1, 0, 0), new Vector3( 0,-1, 0), new Vector3( 0, 0,-1),
			new Vector3(-1, 0, 0), new Vector3( 0,-1, 0), new Vector3( 0, 0, 1),
		};
		
		float weightSum = 0.0f;

		UnityEngine.Rendering.SphericalHarmonics3 sh = new UnityEngine.Rendering.SphericalHarmonics3 ();
		sh.Clear ();

		// go over all pixels of a cubemap face
		for (int i = 0; i < 6; ++i)
		{
			Vector3 basisX =  kCubemapOrthoBases[i*3+0];
			Vector3 basisY = -kCubemapOrthoBases[i*3+1];
			Vector3 basisZ = -kCubemapOrthoBases[i*3+2];
			
			Color[] pixels = src.GetPixels((CubemapFace)i, miplevel);
			int size = src.width >> miplevel;
			if (size < 1) size = 1;
			
			// We'll need pixel center coordinates in -1..1 space, so
			// basically (-1 + 1/size) to (1 - 1/size) when integer goes from
			// 0 to size-1.
			float coordBias = -1.0f + 1.0f / (float)size;
			float coordScale = 2.0f * (1.0f - 1.0f / (float)size) / ((float)size - 1.0f);
			
			for (int y = 0; y < size; ++y)
			{
				float fy = y * coordScale + coordBias;
				for (int x = 0; x < size; ++x)
				{
					Color rgbm = pixels[x + y * size];
					float fx = x * coordScale + coordBias;
					
					// fx, fy are pixel coordinates in -1..+1 range
					float ftmp = 1.0f + fx*fx + fy*fy;
					float weight = 4.0f / (Mathf.Sqrt(ftmp) * ftmp);
					
					// evaluate SH in pixel's direction, weight by solid angle
					// and overall exposure
					Vector3 dir = basisZ + basisX * fx + basisY * fy;
					dir.Normalize ();

					// accumulate into overall SH
					Color color = rgbm * rgbm.a * 8.0f;
					sh.AddDirectionalLight (-dir,
						(QualitySettings.activeColorSpace == ColorSpace.Linear) ? color.linear: color,
						weight * 0.5f /* "normal" light intensity in Unity is 0.5 */);
					weightSum += weight;
				}
			}
		}
		
		// normalize SH and add to output.
		float normWeight = 4.0f / weightSum;
		sh *= normWeight;

		return sh;
	}
}

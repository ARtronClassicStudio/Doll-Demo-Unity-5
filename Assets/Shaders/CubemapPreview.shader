Shader "Custom/Cubemap Preview" {
Properties {
	_Cube ("Reflection Cubemap", Cube) = "black"
	_ReflectionProbeType ("Type", Float) = 0
}
SubShader {
	Tags { "ForceSupported" = "True" }

	CGPROGRAM
	#pragma surface surf Lambert nofog
	#include "UnityCG.cginc"

	samplerCUBE _Cube;
	half4 _Cube_HDR;
	half _Bias;

	struct Input {
		float3 worldNormal;
	};

	void surf (Input IN, inout SurfaceOutput o)
	{
		o.Albedo = 0;
		float3 vertexNormal = mul(_World2Object, float4(IN.worldNormal,0));
		fixed4 cubeTex = texCUBEbias (_Cube, float4(vertexNormal.rgb, _Bias));
		half3 reflColor = DecodeHDR (cubeTex, _Cube_HDR);
		o.Emission = reflColor;
		o.Alpha = 1.0f;
	}	
	ENDCG
	}
}

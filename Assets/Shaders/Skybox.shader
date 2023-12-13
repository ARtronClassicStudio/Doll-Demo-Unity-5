Shader "Hidden/Skybox" {
Properties {
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" }
	Cull Off ZWrite Off Fog { Mode Off }

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		
		#include "UnityCG.cginc"
		

		samplerCUBE _SkyCube;
		half4		_SkyCubeParams;
		
		struct VertexIn {
			float4 vertex : POSITION;
			float4 texcoord : TEXCOORD0;
		};

		struct VertexOut {
			float4 vertex : POSITION;
			float3 texcoord : TEXCOORD0;
		};
		
		VertexOut vert (VertexIn v)
		{
			VertexOut o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.texcoord.xyz = v.texcoord.xyz;
			return o;
		}

		half3 DecodeRGBM (half4 c) 
		{
			return c.rgb * pow(c.a * _SkyCubeParams.x, _SkyCubeParams.y) * _SkyCubeParams.z;
		}

		half4 frag (VertexOut i) : COLOR
		{
			half4 col = texCUBE(_SkyCube, i.texcoord);
			col.rgb = DecodeRGBM (col);
			col.a = 1.0;
			return col;
		}
		ENDCG 
	}
} 	


SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" }
	Cull Off ZWrite Off Fog { Mode Off }
	Color [_Tint]
	Pass {
		SetTexture [_Tex] { combine texture +- primary, texture * primary }
	}
}

Fallback Off

}

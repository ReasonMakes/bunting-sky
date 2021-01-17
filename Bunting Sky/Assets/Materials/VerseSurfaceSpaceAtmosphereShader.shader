//Kyle Halladay, thank you for lighting- er... SHADING the way
//http://kylehalladay.com/blog/tutorial/bestof/2014/05/16/Coloured-Shadows-In-Unity.html

Shader "Custom/VerseSurfaceSpaceAtmosphereShader"
{
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_ShadowColor("Shadow Color", Color) = (0.094,0.094,0.286,1)
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

		CGPROGRAM
		#pragma surface surf CSLambert

		sampler2D _MainTex;
		fixed4 _Color;
		fixed4 _ShadowColor;

		struct Input {
			float2 uv_MainTex;
		};

		half4 LightingCSLambert(SurfaceOutput s, half3 lightDir, half atten) {

			fixed diff = max(0, dot(s.Normal, lightDir));

			fixed4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);

			c.rgb += _ShadowColor.xyz * atten;

			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "VertexLit"
}
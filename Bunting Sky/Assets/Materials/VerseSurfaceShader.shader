Shader "Custom/VerseSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _TintColor("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        fixed4 _TintColor;

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float3 normal: NORMAL;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
            half3 worldNormal: NORMAL;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        
        float Shading(float3 normal, float3 lightDir)
        {
            float NdotL = max(0.0, dot(normalize(normal), normalize(lightDir)));

            return NdotL;
        }

        /*
        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            //Have to comment this out for some reason
            //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
            return o;
        }
        */

        //This function is never run
        fixed4 frag(v2f i) : SV_Target
        {
            fixed4 col = tex2D(_MainTex, i.uv);

            col *= Shading(i.worldNormal, _WorldSpaceLightPos0.xyz);
            col += _TintColor * (1 - Shading(i.worldNormal, _WorldSpaceLightPos0.xyz));

            return col;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //MEMEMEME
            //_Color *= Shading(i.worldNormal, _WorldSpaceLightPos0.xyz);
            //_Color += _TintColor * (1 - Shading(i.worldNormal, _WorldSpaceLightPos0.xyz));

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

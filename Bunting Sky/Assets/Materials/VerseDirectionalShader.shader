Shader "Unlit/VerseShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _TintColor;

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float Shading(float3 normal, float3 lightDir)
            {
                float NdotL = max(0.0, dot(normalize(normal), normalize(lightDir)));

                return NdotL;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                //col *= Shading(i.worldNormal, _WorldSpaceLightPos0.xyz);

                //float4 skyboxCol = (0.09411764705882352941176470588235, 0.09411764705882352941176470588235, 0.29019607843137254901960784313725, 1.0);
                //float4 skyboxCol = (0.094, 0.094, 0.29, 1.0);

                //col += skyboxCol * (Shading(i.worldNormal, _WorldSpaceLightPos0.xyz));

                //col = (1 - Shading(i.worldNormal, _WorldSpaceLightPos0.xyz)) * skyboxCol;

                //col = _TintColor;

                col *= Shading(i.worldNormal, _WorldSpaceLightPos0.xyz);
                col += _TintColor * (1 - Shading(i.worldNormal, _WorldSpaceLightPos0.xyz));

                return col;
            }
            ENDCG
        }
    }
}

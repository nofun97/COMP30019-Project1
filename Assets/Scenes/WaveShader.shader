// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WaveShader"
{
    Properties
    {
			_MainTex ("Albedo Texture", 2D) = "white" {}
			_TintColor("Tint Color", Color) = (0.0, 1.0, 0.99, 0.37)
			_Transparency("Transparency", Range(0.0,0.5)) = 0.25
			_CutoutThresh("Cutout Threshold", Range(0.0,1.0)) = 0.2
			_PointLightColor("Point Light Color", Color) =(0, 0, 0)
			_PointLightPosition("Point Light Position", Vector) = (0.0, 0.0, 0.0)
			_AmbientReflectionConstant("Ambient Reflection Constant", Range(0.0, 1.0)) = 1.0
			_SpecularReflectivity("Specular Reflectivity", Range(0.0, 1.0)) = 0.75
			_DiffuseReflectivity("Diffuse Reflectivity", Range(0.0, 1.0)) = 1.0
			_HighlightsTightness("Highlights Tightness", Float) = 1.0
			_AttenuationFactor("Attenuation Factor", Float) = 1.0
    }
    SubShader
    {
			Tags {"Queue"="Transparent" "RenderType"="Transparent" }
			LOD 100

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _TintColor;
				float _Transparency;
				float _CutoutThresh;
				uniform float3 _PointLightColor;
				uniform float3 _PointLightPosition;
				float _AmbientReflectionConstant;
				float _SpecularReflectivity;
				float _DiffuseReflectivity;
				float _HighlightsTightness;
				float _AttenuationFactor;

				struct vertIn
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				struct vertOut
				{
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color: COLOR;
					float4 worldVertex : TEXCOORD1;
					float3 worldNormal : TEXCOORD2;
				};

				// Implementation of the vertex shader
				vertOut vert(vertIn v)
				{
					// Displace the original vertex in model space
					float4 displacement = float4(0.0f, sin(v.vertex.x + _Time.y), 0.0f, 0.0f);
					v.vertex += displacement;

					vertOut o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					// o.color = float4(0.0f, 1.0f, 0.99f, 0.37f);
					o.worldVertex = mul(unity_ObjectToWorld, v.vertex);
					o.worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), float3(0.0, 1.0, 0.0)));
					return o;
				}

				// Implementation of the fragment shader
				fixed4 frag(vertOut v) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, v.uv) + _TintColor;
					col.a = _Transparency;
					clip(col.r - _CutoutThresh);

					// Our interpolated normal might not be of length 1
					float3 interpNormal = normalize(v.worldNormal);

					// Calculate ambient RGB intensities
					float Ka = _AmbientReflectionConstant;
					float3 amb = col.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * Ka;

					// Calculate diffuse RBG reflections, we save the results of L.N because we will use it again
					// (when calculating the reflected ray in our specular component)
					float fAtt = _AttenuationFactor;
					float Kd = _DiffuseReflectivity;
					float3 L = normalize(_PointLightPosition - v.worldVertex.xyz);
					float LdotN = dot(L, interpNormal);
					float3 dif = fAtt * _PointLightColor.rgb * Kd * col.rgb * saturate(LdotN);

					// Calculate specular reflections
					float Ks = _SpecularReflectivity;
					float specN = _HighlightsTightness; // Values>>1 give tighter highlights
					float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);
					// Using classic reflection calculation:
					float3 R = normalize((2.0 * LdotN * interpNormal) - L);
					float3 spe = fAtt * _PointLightColor.rgb * Ks * pow(saturate(dot(V, R)), specN);

					// Combine Phong illumination model components
					float4 returnColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
					returnColor.rgb = amb.rgb + dif.rgb + spe.rgb;
					returnColor.a = col.a;

					return returnColor;
				}
				ENDCG
			}
    }
    FallBack "Diffuse"
}

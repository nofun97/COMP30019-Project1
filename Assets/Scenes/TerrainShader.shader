// Original Cg/HLSL code stub copyright (c) 2010-2012 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Adapted for COMP30019 by Jeremy Nicholson, 10 Sep 2012
// Adapted further by Chris Ewin, 23 Sep 2013
// Adapted further (again) by Alex Zable (port to Unity), 19 Aug 2016
// Adapted even further by Patrick Setiawan, Michelle Anggana, Novan Allanadi 09 Sept 2019

//UNITY_SHADER_NO_UPGRADE

Shader "Unlit/PhongShader"
{
	Properties
	{
		_PointLightColor("Point Light Color", Color) =(0, 0, 0)
		_PointLightPosition("Point Light Position", Vector) = (0.0, 0.0, 0.0)
		_MinimumMountainHeight("Minimum Mountain Height", Float) = 10.0
		_MaxSeaLevel("Max Sea Level", Float) = 5.0
		_AmbientReflectionConstant("Ambient Reflection Constant", Range(0.0, 1.0)) = 1.0
		_SpecularReflectivity("Specular Reflectivity", Range(0.0, 1.0)) = 0.75
		_DiffuseReflectivity("Diffuse Reflectivity", Range(0.0, 1.0)) = 1.0
		_HighlightsTightness("Highlights Tightness", Float) = 1.0
		_AttenuationFactor("Attenuation Factor", Float) = 1.0
		_PeakColor("Peak Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_GroundColor("Ground Color", Color) = (0.376, 0.502, 0.22, 1.0)
		_SeaFloorColor("Sea Floor Color", Color) = (0.34, 0.23, 0.04, 1.0)
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float3 _PointLightColor;
			uniform float3 _PointLightPosition;
			float _MinimumMountainHeight;
			float _MaxSeaLevel;
			float _AmbientReflectionConstant;
			float _SpecularReflectivity;
			float _DiffuseReflectivity;
			float _HighlightsTightness;
			float _AttenuationFactor;
			float4 _PeakColor;
			float4 _GroundColor;
			float4 _SeaFloorColor;

			struct vertIn
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 color : COLOR;
			};

			struct vertOut
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 worldVertex : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
			};

			// Implementation of the vertex shader
			vertOut vert(vertIn v)
			{
				vertOut o;

				// Convert Vertex position and corresponding normal into world coords.
				// Note that we have to multiply the normal by the transposed inverse of the world
				// transformation matrix (for cases where we have non-uniform scaling; we also don't
				// care about the "fourth" dimension, because translations don't affect the normal)
				float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
				float3 worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), v.normal.xyz));

				// Transform vertex in world coordinates to camera coordinates, and pass colour
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				if (v.vertex.y <= _MaxSeaLevel) {
					o.color = _SeaFloorColor;
				} else if (v.vertex.y >= _MinimumMountainHeight) {
					o.color = _PeakColor;
				} else {
					o.color = _GroundColor;
				}

				// Pass out the world vertex position and world normal to be interpolated
				// in the fragment shader (and utilised)
				o.worldVertex = worldVertex;
				o.worldNormal = worldNormal;

				return o;
			}

			// Implementation of the fragment shader
			fixed4 frag(vertOut v) : SV_Target
			{
				// Our interpolated normal might not be of length 1
				float3 interpNormal = normalize(v.worldNormal);

				// Calculate ambient RGB intensities
				float Ka = _AmbientReflectionConstant;
				float3 amb = v.color.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * Ka;

				// Calculate diffuse RBG reflections, we save the results of L.N because we will use it again
				// (when calculating the reflected ray in our specular component)
				float fAtt = _AttenuationFactor;
				float Kd = _DiffuseReflectivity;
				float3 L = normalize(_PointLightPosition - v.worldVertex.xyz);
				float LdotN = dot(L, interpNormal);
				float3 dif = fAtt * _PointLightColor.rgb * Kd * v.color.rgb * saturate(LdotN);

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
				returnColor.a = v.color.a;

				return returnColor;
			}
			ENDCG
		}
	}
}

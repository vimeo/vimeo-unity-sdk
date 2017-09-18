Shader "360/Equirectangular"
{
	Properties
	{
		_MainTex ("Diffuse (RGB) Alpha (A)", 2D) = "gray" {}
	}

	SubShader
	{
		Pass
		{
			Tags {"LightMode" = "Always"}

			Cull Front

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 normal : TEXCOORD0;
			};

			v2f vert(float4 vertex : POSITION, float3 normal : NORMAL)
			{
				v2f outCoords = { UnityObjectToClipPos(vertex), normal };
				return outCoords;
			}

			#define ONE_OVER_PI .31830988618379067154F

			inline float2 ToRadialCoords(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * ONE_OVER_PI;
				return float2(0.5F - sphereCoords.x * 0.5F, 1.0F - sphereCoords.y);
			}

			sampler2D _MainTex;

			float4 frag(float3 coords : TEXCOORD0) : SV_Target
			{
				float2 equirectangularUV = ToRadialCoords(coords);
				return tex2D(_MainTex, equirectangularUV);
			}
			ENDCG
		}
	}
	FallBack "VertexLit"
}
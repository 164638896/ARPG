
Shader "Shaders/NormalMap"
{
	Properties
	{
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		//_BumpScale ("Bump Scale", Float) = 1.0
		//_Specular ("Specular", Color) = (1, 1, 1, 1)
		//_Gloss ("Gloss", Range(8.0, 256)) = 20
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)

		[Toggle(NORMAL_MAP)] _EnableNormal("Enable Normal", Float) = 0
		[Toggle(RIM)] _EnableRIM("Enable Rim", Float) = 0
	}

	SubShader 
	{
		Pass 
		{ 
			Tags { "LightMode"="ForwardBase" }
		
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma multi_compile __ NORMAL_MAP
			#pragma multi_compile __ RIM

			#include "Lighting.cginc"
			
			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			//float _BumpScale;
			//fixed4 _Specular;
			//float _Gloss;
			fixed4 _RimColor;

			struct a2v 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
			};
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float3 lightDir: TEXCOORD1;
				float3 viewDir : TEXCOORD2;
#ifndef NORMAL_MAP
				float3 normal : NORMAL;
#endif
			};

			v2f vert(a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				
				o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

				o.lightDir = WorldSpaceLightDir(v.vertex);
				o.viewDir = WorldSpaceViewDir(v.vertex);

#ifdef NORMAL_MAP
				o.uv.zw = v.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;

				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

				float3x3 worldToTangent = float3x3(worldTangent, worldBinormal, worldNormal);

				// Transform the light and view dir from world space to tangent space
				o.lightDir = mul(worldToTangent, o.lightDir);
				o.viewDir = mul(worldToTangent, o.viewDir);
#else 
				o.normal = UnityObjectToWorldNormal(v.normal);
#endif	
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{				
				fixed3 lightDir = normalize(i.lightDir);
				fixed3 viewDir = normalize(i.viewDir);
				
				fixed3 normal;
#ifdef NORMAL_MAP			
				// Get the texel in the normal map
				normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
#else
				normal = i.normal;
#endif	

				fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;

				fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(normal, lightDir));

				//fixed3 halfDir = normalize(lightDir + viewDir);
				//fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(normal, halfDir)), _Gloss);
#ifdef RIM
				fixed3 rimColor = _RimColor.rgb * (1.0 - dot(normal, viewDir));
				diffuse += rimColor;
#endif
				return fixed4(ambient + diffuse/* + specular*/, 1.0);
			}
			
			ENDCG
		}
	} 
	Fallback "Diffuse"
}


Shader "Shaders/MyStandard"
{
	Properties
	{
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		
		[Toggle(AMBIENT)] _EnableAmbient("Ambient", Float) = 0
		[Toggle(LIGHTCOLOR)] _EnableLightColor("Light Color", Float) = 0

		[Toggle(NORMAL_MAP)] _EnableNormal("Normal", Float) = 0
		_BumpMap("Normal Map", 2D) = "bump" {}

		[Toggle(SPECULAR_MAP)] _EnableSpecular("Specular", Float) = 0
		_SpecularMap("Specular Map", 2D) = "Specular" {}
		_Gloss("Gloss", Range(2.0, 256)) = 20

		[Toggle(RIM)] _EnableRIM("Enable Rim", Float) = 0
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
	}

	SubShader 
	{
		Pass 
		{ 
			Tags { "LightMode"="ForwardBase" }
		
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma multi_compile __ AMBIENT
			#pragma multi_compile __ LIGHTCOLOR
			#pragma multi_compile __ NORMAL_MAP
			#pragma multi_compile __ SPECULAR_MAP
			#pragma multi_compile __ RIM
			
			#include "Lighting.cginc"
			
			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			
			sampler2D _SpecularMap;
			float4 _SpecularMap_ST;
			float _Gloss;

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
				normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
#else
				normal = i.normal;
#endif	

				fixed3 ambient = fixed3(0,0,0);
				fixed3 diffuse = tex2D(_MainTex, i.uv).rgb * _Color.rgb;
#ifdef AMBIENT
				ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * diffuse;
#endif

#ifdef LIGHTCOLOR
				diffuse = _LightColor0.rgb * diffuse * max(0, dot(normal, lightDir));
#endif

#ifdef SPECULAR_MAP
				fixed3 halfDir = normalize(lightDir + viewDir);
				diffuse += _LightColor0.rgb * tex2D(_SpecularMap, i.uv).rgb * pow(max(0, dot(normal, halfDir)), _Gloss);
#endif

#ifdef RIM
				fixed3 rimColor = _RimColor.rgb * (1.0 - dot(normal, viewDir));
				diffuse += rimColor;
#endif
				return fixed4(ambient + diffuse, 1.0);
			}
			
			ENDCG
		}
	} 
	Fallback "Diffuse"
}

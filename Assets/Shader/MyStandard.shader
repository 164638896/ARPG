
Shader "Shaders/MyStandard"
{
	Properties
	{
		_Color("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex("Main Tex", 2D) = "white" {}

		[Toggle(AMBIENT)] _EnableAmbient("Ambient", Float) = 0
		[Toggle(LIGHTCOLOR)] _EnableLightColor("Light Color", Float) = 0

		[Toggle(NORMAL_MAP)] _EnableNormal("Normal", Float) = 0
		_BumpMap("Normal Map", 2D) = "white" {}

		[Toggle(SPECULAR_MAP)] _EnableSpecular("Specular", Float) = 0
		_Specular("Specular", Color) = (1, 1, 1, 1)
		 _Gloss("Gloss", Range(2.0, 256)) = 20

		[Toggle(CUBE)] _EnableCube("Enable Cube", Float) = 0
		_CubeMap("Reflection Cubemap", Cube) = "_Skybox" {}
		 _ReflectAmount("Reflect Amount", Range(0, 1)) = 0.5

		_MaskMap("Mask Map", 2D) = "white" {}

		[Toggle(RIM)] _EnableRIM("Enable Rim", Float) = 0
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)

		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dest Blend", Float) = 0
		[Enum(Off,0,On,1)] _ZWrite("ZWrite", Float) = 1
	}

	SubShader
	{
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			Cull[_Cull]

			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]

			CGPROGRAM

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ AMBIENT
			#pragma multi_compile __ LIGHTCOLOR
			#pragma multi_compile __ NORMAL_MAP
			#pragma multi_compile __ SPECULAR_MAP
			#pragma multi_compile __ CUBE			
			#pragma multi_compile __ RIM
			

			#include "Lighting.cginc"

			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _BumpMap;
			float4 _BumpMap_ST;

			sampler2D _MaskMap;
			float4 _MaskMap_ST;

			fixed4 _Specular;
			float _Gloss;

			samplerCUBE _CubeMap;
			fixed _ReflectAmount;

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

			#ifndef NORMAL_MAP
				float3 normal : NORMAL;
			#endif
			
			#ifdef CUBE
				float4 TtoW0 : TEXCOORD1;
				float4 TtoW1 : TEXCOORD2;
				float4 TtoW2 : TEXCOORD3;
			#else 
				float3 lightDir: TEXCOORD1;
				float3 viewDir : TEXCOORD2;
			#endif
			};

			v2f vert(a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

			#ifdef CUBE
				#ifdef NORMAL_MAP
					o.uv.zw = v.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;

					float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

					fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
					fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
					fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

					o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
					o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
					o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
				#else 
					o.normal = UnityObjectToWorldNormal(v.normal);
				#endif	
			#else

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
			#endif

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed3 normal;
				fixed3 ambient = fixed3(0, 0, 0);

				fixed4 mask = tex2D(_MaskMap, i.uv.xy);

			#ifdef CUBE
				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				#ifdef NORMAL_MAP			
					normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
					normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
				#else
					normal = i.normal;
				#endif	

					float3 worldRefl = reflect(-viewDir, normal);
					fixed3 reflection = texCUBE(_CubeMap, worldRefl).rgb;

					fixed4 diffuse = tex2D(_MainTex, i.uv) * _Color;
				#ifdef AMBIENT
					ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * diffuse.rgb;
				#endif

					diffuse.rgb *= max(0, dot(normal, lightDir));

				#ifdef LIGHTCOLOR
					diffuse.rgb *= _LightColor0.rgb;
				#endif

				#ifdef SPECULAR_MAP
					fixed3 halfDir = normalize(lightDir + viewDir);
					diffuse.rgb += _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(normal, halfDir)), _Gloss);
				#endif

				#ifdef RIM
					fixed3 rimColor = _RimColor.rgb * (1.0 - dot(normal, viewDir));
					diffuse.rgb += rimColor;
				#endif
					fixed3 color = ambient + diffuse.rgb;
					color = color * (1 - mask.g) + lerp(color * mask.g, 2 * reflection * mask.g, _ReflectAmount);
					return fixed4(color, diffuse.a);
			#else

				fixed3 lightDir = normalize(i.lightDir);
				fixed3 viewDir = normalize(i.viewDir);

				#ifdef NORMAL_MAP			
					normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
				#else
					normal = i.normal;
				#endif	

					fixed4 diffuse = tex2D(_MainTex, i.uv) * _Color;
				#ifdef AMBIENT
					ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * diffuse.rgb;
				#endif

					diffuse.rgb *= max(0, dot(normal, lightDir));

				#ifdef LIGHTCOLOR
					diffuse.rgb *= _LightColor0.rgb;
				#endif

				#ifdef SPECULAR_MAP
					fixed3 halfDir = normalize(lightDir + viewDir);
					diffuse.rgb += _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(normal, halfDir)), _Gloss);
				#endif

				#ifdef RIM
					fixed3 rimColor = _RimColor.rgb * (1.0 - dot(normal, viewDir));
					diffuse.rgb += rimColor;
				#endif
					return fixed4(ambient + diffuse.rgb, diffuse.a);
			#endif

			}

			ENDCG
		}
	}
	Fallback "Diffuse"
}

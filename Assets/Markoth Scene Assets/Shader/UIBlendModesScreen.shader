Shader "UI/BlendModes/Screen" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Vector) = (1,1,1,1)
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
	}

		SubShader
		{
			LOD 950
			Tags { "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			GrabPass {
			}
			Pass
			{
				LOD 950
			Tags { "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ColorMask 0 -1
			ZWrite Off
			Cull Off
			Stencil {
				ReadMask 0
				WriteMask 0
				Comp Always
				Pass Keep
				Fail Keep
				ZFail Keep
			}
			Fog {
				Mode Off
			}
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				float4 _Color;

				sampler2D _MainTex;
				sampler2D _GrabTexture;

				struct appdata_t
				{
					float4 vertex : POSITION;
					float4 color : COLOR0;
					float2 texcoord : TEXCOORD0;

				};

				struct v2f
				{
					float4 color : COLOR0;
					float2 texcoord0 : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 position : SV_POSITION;
					//float4 texcoord : TEXCOORD0;
				};


				v2f vert(appdata_t v)
				{
					v2f o;
					o.position = UnityObjectToClipPos(v.vertex);
					o.texcoord1 = o.position;
					o.color = v.color * _Color;
					o.texcoord0 = v.texcoord;
					/*float4 u_xlat1;
					float4 oldPos = o.position;

					oldPos.y = oldPos.y * _ProjectionParams.x;
					u_xlat1.xzw = oldPos.xwy * float3(0.5, 0.5, 0.5);
					o.texcoord.zw = oldPos.zw;
					o.texcoord.xy = u_xlat1.zz + u_xlat1.xw;*/
					return o;
				}


				fixed4 frag(v2f i) : SV_Target
				{
					float3 u_xlat0;
					float4 u_xlat10_0;
					float3 u_xlat1;
					float3 u_xlat16_1;
					float4 u_xlat10_1;
					bool u_xlatb1;
					float u_xlat6;
					fixed4 output;

					u_xlat10_0 = tex2D(_MainTex, i.texcoord0.xy);
					u_xlat1.x = u_xlat10_0.w * i.color.w + -0.00999999978;
					u_xlatb1 = u_xlat1.x < 0.0;
					if (((int(u_xlatb1) * int(0xffffffffu))) != 0) { discard; }
					u_xlat1.xy = i.texcoord1.xy / i.texcoord1.ww;
					u_xlat1.xy = u_xlat1.xy + float2(1.0, 1.0);
					u_xlat1.x = u_xlat1.x * 0.5;
					u_xlat1.z = (-u_xlat1.y) * 0.5 + 1.0;
					u_xlat10_1 = tex2D(_GrabTexture, u_xlat1.xz);
					u_xlat16_1.xyz = (-u_xlat10_1.xyz) + float3(1.0, 1.0, 1.0);
					u_xlat0.xyz = (-u_xlat10_0.xyz) * i.color.xyz + float3(1.0, 1.0, 1.0);
					u_xlat6 = u_xlat10_0.w * i.color.w;
					output.w = u_xlat6;
					output.xyz = (-u_xlat16_1.xyz) * u_xlat0.xyz + float3(1.0, 1.0, 1.0);
					return output;
					/*float2 samp = i.texcoord.xy / i.texcoord.ww;
					fixed4 c = tex2D(_MainTex, samp);
					return c;*/
				}
				ENDCG
			}
		}

	/*//DummyShader
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}*/
	Fallback "Sprites/Approximate Screen"
}
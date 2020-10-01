// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Sprites/Compartment"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_GradientTexture("Gradient Texture", 2D) = "white" {}
		_Color ("Sprite Tint", Color) = (1,1,1,1)
		_Blend("Blend", Range(0, 1)) = 0.5
		_Center("Center", Vector) = (1,1,1)
		//_Color2("Background", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _distance_mode("Dist", Float) = 0
		[PerRendererData] _Outline("Outline", Float) = 0
		[PerRendererData] _OutlineColor("Outline Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				//nointerpolation float D: FLOAT10;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				//nointerpolation float2 d		 : TEXCOORD1;
				nointerpolation float4 screenPos : TEXCOORD2;
			};
			
			float3 _Center;
			fixed4 _Color;
			//fixed4 _Color2;
			float _Outline;
			float _Blend;
			fixed4 _OutlineColor;
			float _distance_mode;

			float getLerPValue(float z) {
				if ((z <= 0.0004882813f) && (z >= 0.000))
					return 0.0f;
				if ((z > 0.0004882813f) && (z <= 0.1254883f))
					return 0.25f;
				if ((z <= 0.2504883f) && (z > 0.1254883f))
					return 0.25f;
				if (z > 0.2504883f)
					return 0.5f;
				else 
					return 0.5f;
			}

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float4 p = UnityObjectToClipPos(_Center);
				OUT.screenPos = p;
				OUT.screenPos.y *= _ProjectionParams.x;
				//float3 p = mul(unity_ObjectToWorld, IN.vertex);
				//float distance = length(p-_Center);
				float z = getLerPValue(mul(unity_ObjectToWorld, IN.vertex).z);//0.0004882813,0.1254883,0.2504883
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				//OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
				OUT.color = IN.color * lerp(_Color, unity_FogColor, z);
				//#ifdef PIXELSNAP_ON
				//OUT.vertex = UnityPixelSnap (OUT.vertex);
				//#endif
				//OUT.d = distance;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _GradientTexture;
			sampler2D _AlphaTex;
			float4 _MainTex_TexelSize;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

//#if ETC1_EXTERNAL_ALPHA
//				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
//				color.a = tex2D (_AlphaTex, uv).r;
//#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
                float2 TexCoord = IN.screenPos.xy / IN.screenPos.w;
                TexCoord = 0.5f * (float2(TexCoord.x,-TexCoord.y) + 1);
				float2 screenUV = IN.screenPos.xy / IN.screenPos.w * 0.5f + 0.5f;
                float2 pixelUV = IN.texcoord.xy;//  * 0.5f + 0.5f;
				float d = distance(TexCoord,pixelUV);
				fixed4 col = IN.color;
				//col.g = d;
				return col;
			}
		ENDCG
		}
	}
}

Shader "Custom/spritecolor" {
		// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
		Properties
		{
			[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
			_Color("Sprite Tint", Color) = (1,1,1,1)
			_Blend("Blend", Range(0, 1)) = 0.5
			//_Color2("Background", Color) = (1,1,1,1)
			[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		}

			SubShader
		{
			Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
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
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord  : TEXCOORD0;
			//float3 worldcoord :FLOAT30;
		};

		fixed4 _Color;
		//fixed4 _Color2;
		float _Blend;

		v2f vert(appdata_t IN)
		{
			v2f OUT;
			OUT.vertex = UnityObjectToClipPos(IN.vertex);
			OUT.texcoord = IN.texcoord;
			OUT.color = IN.color * _Color;
			return OUT;
		}

		sampler2D _MainTex;
		sampler2D _AlphaTex;

		float4 _MainTex_TexelSize;

		fixed4 SampleSpriteTexture(float2 uv)
		{
			fixed4 color = tex2D(_MainTex, uv);

			//#if ETC1_EXTERNAL_ALPHA
			// get the color from an external texture (usecase: Alpha support for ETC1 on android)
			//color.a = tex2D (_AlphaTex, uv).r;
			//#endif //ETC1_EXTERNAL_ALPHA

			return color;
		}

		fixed4 frag(v2f IN) : SV_Target
		{
			//fixed4 c = SampleSpriteTexture (IN.texcoord);
			//c.rgb = _Color.rgb;
			fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
			c.rgb = _Color.rgb*c.a;
			return c;
		}
			ENDCG
		}
		}
	}


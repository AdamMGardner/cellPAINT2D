// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Sprites/NucleicAcid"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Sprite Tint", Color) = (1,1,1,1)
		_Blend("Blend", Range(0, 1)) = 0.5
		//_Color2("Background", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

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
			#pragma target 3.5
			//#pragma target 2.0
			#pragma multi_compile _ PIXELSNAP_ON
			//#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
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
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 z : TEXCOORD1;
				//float3 worldcoord :FLOAT30;
			};
			
			fixed4 _Color;
			//fixed4 _Color2;
			float _Outline;
			float _Blend;
			fixed4 _OutlineColor;

			float getLerPValue(float z) {
				if ((z < 0.0004882813f))
					return 0.0f;
				if ((z >= 0.0004882813f) && (z <= 0.26f))
					return z*3.5f;
				else 
					return 0.95f;
			}

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float z = getLerPValue(mul(unity_ObjectToWorld, IN.vertex).z);//0.0004882813,0.1254883,0.2504883
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				//OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
				OUT.color = IN.color*  lerp(_Color, unity_FogColor, z);
				//OUT.color.rgb *= lerp(_Color, unity_FogColor, z);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				OUT.z = float2(z,z);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float4 _MainTex_TexelSize;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				//color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 BlurTexels(float2 uv, int kernelSize)
				{
					//Compute new fragment color value : Box Filter
					float weight = 1.0f ;/// 9.0f;
	
					fixed4 colorSum = fixed4(0.0f, 0.0f, 0.0f, 0.0f);
					int weightSum = 0;
	
					int dX, dY;
					float2 neighborCoord;
					[unroll(16)]
					for(dX = -kernelSize; dX <= kernelSize; dX++) {
						for(dY = -kernelSize; dY <= kernelSize; dY++) {
							neighborCoord = float2(uv.x+dX*_MainTex_TexelSize.x, uv.y+dY*_MainTex_TexelSize.y);
							colorSum += tex2D(_MainTex, neighborCoord)*weight;
							weightSum += weight;
						}
					}

					float4 newColor = colorSum  / weightSum;
					return newColor; //Result of blur
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				int scalep = (int) _Outline;
				//fixed4 c = SampleSpriteTexture (IN.texcoord)* IN.color;
				int kernelSize = 1;
				float z = IN.z.x;
				if (z == 0.0f) {kernelSize = 1;}
				if ((z > 0.0f)&&(z < 0.95f)) {kernelSize = 8;}
				if (z == 0.95f) {kernelSize = 12;}
				//to use the blur 
				fixed4 c = BlurTexels (IN.texcoord,kernelSize)* IN.color;
				//fixed4 c = SampleSpriteTexture (IN.texcoord)* IN.color;
				c.rgb *= c.a;

				// If outline is enabled and there is a pixel, try to draw an outline.
				if (_Outline > 0 && c.a != 0) {
					float totalAlpha = 1.0;
					[unroll(16)]
					for (int i = 1; i < scalep + 1; i++)
					{
						// Get the neighbouring four pixels.
						fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, _MainTex_TexelSize.y*i));
						fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, _MainTex_TexelSize.y * i));
						fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(_MainTex_TexelSize.x * i, 0));
						fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(_MainTex_TexelSize.x * i, 0));

						fixed4 pixelUpRight = tex2D(_MainTex, IN.texcoord + fixed2(_MainTex_TexelSize.x * i, _MainTex_TexelSize.y*i));
						fixed4 pixelDownRight = tex2D(_MainTex, IN.texcoord + fixed2(_MainTex_TexelSize.x * i, -_MainTex_TexelSize.y * i));
						fixed4 pixelDownLeft = tex2D(_MainTex, IN.texcoord + fixed2(-_MainTex_TexelSize.x * i, -_MainTex_TexelSize.y * i));
						fixed4 pixelUpLeft = tex2D(_MainTex, IN.texcoord - fixed2(-_MainTex_TexelSize.x * i, _MainTex_TexelSize.y*i));

						totalAlpha = totalAlpha * pixelUp.a * pixelDown.a * pixelRight.a * pixelLeft.a;

						// If one of the neighbouring pixels is invisible, we render an outline.
						//if (pixelUp.a * pixelDown.a * pixelRight.a * pixelLeft.a == 0) {
						//	c.rgba = fixed4(1, 1, 1, 1) * _OutlineColor;
						//}
					}
					if (totalAlpha < 0.2f)//&&totalAlpha > 0.005f)
					{
						c.rgba = fixed4(1, 1, 1, 1) * _OutlineColor;
						//c.a = totalAlpha;
					}
				}
				//c.rgb *= c.a;
				//c.rgb = fixed4(c.a, c.a, c.a, c.a);
				return c;
			}
		ENDCG
		}
	}
}

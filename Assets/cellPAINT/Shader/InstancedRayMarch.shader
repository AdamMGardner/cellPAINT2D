Shader "Instanced/InstancedRayMarch" {
		Properties{
			_MainTex("Albedo (RGB)", 2D) = "white" {}
			_MipLevel("Mip Level", Int) = 0
			_GridSize("Grid Size", Int) = 0
			_VolumeTex("Volume Texture", 3D) = "" {}
			_NumRayStepMax("_Num Ray Step Max", Int) = 32
			_Threshold("Intensity Threshold", Range(0, 1)) = 0.5
			_lowerBounds("lower smooth", Range(0, 1)) = 0.1
			_upperBounds("upper smooth", Range(0, 1)) = 0.4
			_NumShades("Number Of Shades", Range(1, 5)) = 1
			_BaseColor("Base Color", Color) = (1, 1, 1, 1)
		}
			SubShader{
			ZTest LEqual
			ZWrite On
			Cull Back

			Pass{

			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM

#pragma vertex vert
#pragma fragment frag_surf_opaque//frag
#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
#pragma target 4.5

#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"
#include "AutoLight.cginc"

			sampler2D _MainTex;

#if SHADER_TARGET >= 45
		StructuredBuffer<float4> positionBuffer;
#endif

		int _GridSize;
		int _MipLevel;
		int _NumRayStepMax;
		float _Threshold;
		sampler3D _VolumeTex;
		float _NumShades;
		float4 _BaseColor;
		float _lowerBounds;
		float _upperBounds;

		matrix Identity =
		{
			{ 1, 0, 0, 0 },
			{ 0, 1, 0, 0 },
			{ 0, 0, 1, 0 },
			{ 0, 0, 0, 1 }
		};

		uniform	StructuredBuffer<int> _CubeIndices;
		uniform	StructuredBuffer<float3> _CubeVertices;
		uniform	StructuredBuffer<float4x4> _CubeMatrices;
		
		struct v2f
		{
			centroid float4 pos : SV_POSITION;
			float2 uv_MainTex : TEXCOORD0;
			float3 ambient : TEXCOORD1;
			float3 diffuse : TEXCOORD2;
			float3 color : TEXCOORD3;
			float3 viewDir : C0LOR0;
			float3 objectPos : C0LOR1;
			float3 modelMatrix : world;
			SHADOW_COORDS(4)
		};


		void rotate2D(inout float2 v, float r)
		{
			float s, c;
			sincos(r, s, c);
			v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
		}

		v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
		{
#if SHADER_TARGET >= 45
			float4 data = positionBuffer[instanceID];
#else
			float4 data = 0;
#endif

			//float rotation = data.w * data.w * _Time.x * 0.5f;
			//rotate2D(data.xz, rotation);

			float3 localPosition = v.vertex.xyz * data.w;//w is the scale
			float3 worldPosition = data.xyz + localPosition;
			float3 worldNormal = v.normal;

			float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
			float3 ambient = _WorldSpaceLightPos0.xyz;// ShadeSH9(float4(worldNormal, 1.0f));
			float3 diffuse = (ndotl * _LightColor0.rgb);
			float3 color = v.color;
			
			v2f o;
			o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
			o.uv_MainTex = v.texcoord;
			o.ambient = ambient;
			o.diffuse = diffuse;
			o.color = color;
			o.objectPos = localPosition;
			o.viewDir = normalize(worldPosition - _WorldSpaceCameraPos.xyz);// (float4(-_WorldSpaceCameraPos + data.xyz, 1.0f)));//worldPosition - _WorldSpaceCameraPos;
			o.modelMatrix = data.xyz;
			TRANSFER_SHADOW(o)
			return o;
		}

		/*float2 getDimenstions(Texture2D textureObj)
		{
		uint width;
		uint height;
		textureObj.GetDimensions(width, height);
		return float2(width, height);
		}*/

		float sampleVolume(float3 p)
		{
			//return tex3Dlod(_VolumeTex, float4(p,0)).r;
			return tex3Dlod(_VolumeTex, float4(p, 0)).a;
		}

		float sampleVolumeMip(float3 p, int mipLevel)
		{
			//return tex3Dlod(_VolumeTex, float4(p,0)).r;
			return tex3Dlod(_VolumeTex, float4(p, mipLevel)).a;
		}

		float getDepth(float3 current_pos)
		{
			float4 pos = mul(UNITY_MATRIX_VP, float4(current_pos, 1));
			return (pos.z / pos.w);
		}

		float calcAO(float3 pos, float3 nor)
		{
			float occ = 0.0;
			float sca = 1.0;
			for (int i = 0; i<5; i++)
			{
				float hr = 0.01 + 0.12*float(i) / 4.0;
				float3 aopos = nor * hr + pos;
				//float dd = map(aopos).x; distance to closest ?
				float dd = sampleVolumeMip(aopos, _MipLevel);
				occ += -(dd - hr)*sca;
				sca *= 0.95;
			}
			return clamp(1.0 - 3.0*occ, 0.0, 1.0);
		}

		float getAttenuation(float3 position, float dataStep) {
			dataStep = 0.01;
			float intensityThreshold = _Threshold;
			float attenuation = 0.0f;
			float rangeCheck = 1.0f;
			float currentIntensity = sampleVolumeMip(position, _MipLevel);
			float dx = sampleVolumeMip(position + float3(dataStep, 0, 0), _MipLevel);
			float dxn = sampleVolumeMip(position + float3(-dataStep, 0, 0), _MipLevel);
			float dy = sampleVolumeMip(position + float3(0, dataStep, 0), _MipLevel);
			float dyn = sampleVolumeMip(position + float3(0, -dataStep, 0), _MipLevel);
			float dz = sampleVolumeMip(position + float3(0, 0, dataStep), _MipLevel);
			float dzn = sampleVolumeMip(position + float3(0, 0, -dataStep), _MipLevel);

			attenuation += (dx <= currentIntensity ? 1.0 : 0.0) * rangeCheck;
			attenuation += (dy <= currentIntensity ? 1.0 : 0.0) * rangeCheck;
			attenuation += (dz <= currentIntensity ? 1.0 : 0.0) * rangeCheck;
			attenuation += (dxn <= currentIntensity ? 1.0 : 0.0) * rangeCheck;
			attenuation += (dyn <= currentIntensity ? 1.0 : 0.0) * rangeCheck;
			attenuation += (dzn <= currentIntensity ? 1.0 : 0.0) * rangeCheck;
			attenuation /= 6.0f;

			return clamp(1.0 - attenuation, 0.0, 1.0);
		}

		
		float3 getNormal(float3 position, float dataStep)
		{
			float dx = sampleVolume(position + float3(dataStep, 0, 0)) - sampleVolume(position + float3(-dataStep, 0, 0));
			float dy = sampleVolume(position + float3(0, dataStep, 0)) - sampleVolume(position + float3(0, -dataStep, 0));
			float dz = sampleVolume(position + float3(0, 0, dataStep)) - sampleVolume(position + float3(0, 0, -dataStep));

			return normalize(float3(dx, dy, dz));
		}

		// calculate diffuse component of lighting
		float diffuseSimple(float3 L, float3 N) {
			return clamp(dot(L, N), 0.0, 1.0);
		}

		// calculate specular component of lighting
		float specularSimple(float3 L, float3 N, float3 H) {
			if (dot(N, L)>0) {
				return pow(clamp(dot(H, N), 0.0, 1.0), 64.0);
			}
			return 0.0;
		}

		float shadeintensity(float3 directionToCamera, float3 normal) {
			// calculate total intensity of lighting

			float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
			float3 halfVector = normalize(lightDirection + directionToCamera);
			float iambi = 0.1f;
			float idiff = diffuseSimple(lightDirection, normal);
			float ispec = specularSimple(lightDirection, normal, halfVector);
			float intensity = iambi + idiff + ispec;

			// quantize intensity for cel shading
			float shadeIntensity = ceil(intensity * _NumShades) / _NumShades;
			return shadeIntensity;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed shadow = SHADOW_ATTENUATION(i);
			fixed4 albedo = tex2D(_MainTex, i.uv_MainTex);
			float3 lighting = i.diffuse * shadow + i.ambient;
			fixed4 output = fixed4(albedo.rgb * i.color * lighting, albedo.w);
			UNITY_APPLY_FOG(i.fogCoord, output);
			return output;
		}
		
		uint  my_rand()
		{
			uint next1 = 1151752134u;
			uint next2 = 2070363486u;
			next1 = next1 * 1701532575u + 571550083u;
			next2 = next2 * 3145804233u + 4178903934u;
			return (next1 << 16) ^ next2;
		}

		float U_m1_p1() {
			return float(my_rand())*(1.0f / 2147483648.0f) - 1.0f;
		}
		
		float3 pick_random_point_in_sphere() {
			float x0, x1, x2, x3, d2;
			do {
				x0 = U_m1_p1();
				x1 = U_m1_p1();
				x2 = U_m1_p1();
				x3 = U_m1_p1();
				d2 = x0*x0 + x1*x1 + x2*x2 + x3*x3;
			} while (d2>1.0f);
			float scale = 1.0f / d2;
			return float3(2 * (x1*x3 + x0*x2)*scale,
				2 * (x2*x3 + x0*x1)*scale,
				(x0*x0 + x3*x3 - x1*x1 - x2*x2)*scale);
		}

		float3 pick_random_point_in_semisphere(float3 v) {
			float3 result = pick_random_point_in_sphere();
			float vdotr = dot(result, v);
			if (vdotr<0) {
				result.x = -result.x;
				result.y = -result.y;
				result.z = -result.z;
			}
			return result;
		}

		float raycast(float3 rayStart, float3 rayDirection) {
			float numSteps = _NumRayStepMax;
			float rayStepLength = 1 / numSteps;

			// Get ray values
			float3 rayDir = normalize(rayDirection);
			float3 rayStep = rayDir * rayStepLength;
			
			//float3 rayStart = input.objectPos;
			float3 t = max((0.5 - rayStart) / rayDir, (-0.5 - rayStart) / rayDir);
			float3 rayEnd = rayStart + (min(t.x, min(t.y, t.z)) * rayDir);
			// Add noise
			float rand = frac(sin(dot(rayStart.xy, float2(12.9898, 78.233))) * 43758.5453);
			rayStart += rayDir * rand * 0.01;

			// Offset to texture coordinates
			rayEnd += 0.5;
			rayStart += 0.5;

			float rayLengthMax = distance(rayStart, rayEnd);
			uint rayNumSteps = max(min(rayLengthMax / rayStepLength, _NumRayStepMax), 0);

			// Init ray values
			float currentIntensity = 0;
			float3 currentRayPos = rayStart;
			float intensityThreshold = _Threshold;

			int currentNumSteps = 0;
			// Linear search
			for (; currentNumSteps < rayNumSteps; currentNumSteps++)
			{
				currentIntensity = sampleVolumeMip(currentRayPos, _MipLevel);
				if (currentIntensity >= intensityThreshold) break;
				currentRayPos += rayStep;
			}

			// If traversal fail discard pixel
			if (currentIntensity < intensityThreshold) return 0.0f;

			rayStep *= 0.5;
			currentRayPos -= rayStep;

			// Binary search
			for (uint j = 0; j < 4; j++)
			{
				rayStep *= 0.5;
				currentIntensity = sampleVolumeMip(currentRayPos, _MipLevel);
				currentRayPos += (currentIntensity >= intensityThreshold) ? -rayStep : rayStep;
			}
			return sampleVolumeMip(currentRayPos, _MipLevel);
		}

		float getAttenuationRay(float3 position, float raydir) {
			float intensityThreshold = _Threshold;
			float attenuation = 0.0f;
			float rangeCheck = 1.0f;
			int samplesize = 10;
			float currentIntensity = sampleVolumeMip(position, _MipLevel);
			for (int i = 0; i < samplesize; i++) {
				float3 rayDirection = pick_random_point_in_semisphere(-raydir);
				float v = raycast(position, rayDirection);
				attenuation += (v < currentIntensity ? 1.0 : 0.0) * rangeCheck;
			}	
			return attenuation;// clamp(1.0 - attenuation, 0.0, 1.0);
		}

		//void frag_surf_opaque(v2f input, out float4 color : COLOR0, out float depth : DEPTH)
		void frag_surf_opaque(v2f input, out float4 color : COLOR0, out float depth : SV_DepthGreaterEqual) 
			//void frag_surf_opaque(v2f input, out float4 color : COLOR0, out float depth : SV_DepthLessEqual)
		{
			color = float4(0.25, 0.25, 0.25, 1);
			depth = input.pos.z;

			//if (input.discardInstance == 1) discard;

			float numSteps = _NumRayStepMax;
			float rayStepLength = 1 / numSteps;

			// Get ray values
			float3 rayDir = normalize(input.viewDir);
			float3 rayStep = rayDir * rayStepLength;

			// Find the ray start
			/*float3 rayEnd = input.objectPos;
			float3 t = max((0.5 - rayEnd) / -rayDir, (-0.5 - rayEnd) / -rayDir);
			float3 rayStart = rayEnd + (min(t.x, min(t.y, t.z)) * -rayDir);*/

			// Find the ray end
			float3 rayStart = input.objectPos;
			float3 t = max((0.5 - rayStart) / rayDir, (-0.5 - rayStart) / rayDir);
			float3 rayEnd = rayStart + (min(t.x, min(t.y, t.z)) * rayDir);

			// Add noise
			float rand = frac(sin(dot(input.pos.xy, float2(12.9898, 78.233))) * 43758.5453);
			rayStart += rayDir * rand * 0.01;

			// Offset to texture coordinates
			rayEnd += 0.5;
			rayStart += 0.5;

			float rayLengthMax = distance(rayStart, rayEnd);
			uint rayNumSteps = max(min(rayLengthMax / rayStepLength, _NumRayStepMax), 0);

			// Init ray values
			float currentIntensity = 0;
			float3 currentRayPos = rayStart;
			float intensityThreshold = _Threshold;

			int currentNumSteps = 0;
			// Linear search
			for (; currentNumSteps < rayNumSteps; currentNumSteps++)
			{
				currentIntensity = sampleVolumeMip(currentRayPos, _MipLevel);
				if (currentIntensity >= intensityThreshold) break;
				currentRayPos += rayStep;
			}

			// If traversal fail discard pixel
			if (currentIntensity < intensityThreshold) discard;

			rayStep *= 0.5;
			currentRayPos -= rayStep;

			// Binary search
			for (uint j = 0; j < 4; j++)
			{
				rayStep *= 0.5;
				currentIntensity = sampleVolumeMip(currentRayPos, _MipLevel);
				currentRayPos += (currentIntensity >= intensityThreshold) ? -rayStep : rayStep;
			}

			float texelSize = 1.0f / _GridSize;
			float3 normal = getNormal(currentRayPos, texelSize);
			float ndotl = pow(max(0.0, dot(rayDir, normal)), 0.4);

			float metallic = dot(rayDir, normal);
			metallic = smoothstep(_lowerBounds, _upperBounds, metallic); // smooth interpolation between values
																		 // shift matallic value from range [0, 1] to range[0.5, 1]
			metallic = metallic / 2 + 0.5f; // shift metallic intensity by 0.5
											//o_FragColor.xyz = metallic * u_baseColor; // modulate final color
			ndotl = metallic;
			
			float3 lightDirection = normalize(input.ambient.xyz);
			float ndotl2 = saturate(dot(normal, lightDirection.xyz));
			ndotl2 = smoothstep(_lowerBounds, _upperBounds, ndotl2); // smooth interpolation between values
																		 // shift matallic value from range [0, 1] to range[0.5, 1]
			ndotl2 = ndotl2 / 2 + 0.5f; // shift metallic intensity by 0.5
											//o_FragColor.xyz = metallic * u_baseColor; // modulate final color
			ndotl = ndotl2;

			float3 ambient = ShadeSH9(float4(normal, 1.0f));
			float3 diffuse = (ndotl * input.color.rgb);
			fixed shadow = SHADOW_ATTENUATION(input);
			fixed4 albedo = tex2D(_MainTex, input.uv_MainTex);
			
			float3 lighting = diffuse * shadow + ambient;
			fixed4 output = fixed4(albedo.rgb * input.color * lighting, albedo.w);
			UNITY_APPLY_FOG(input.fogCoord, output);

			/*float shadein = shadeintensity(rayDir, normal);
			float3 normalDirection = normalize(
			mul(float4(normal, 0.0), transpose(input.modelMatrix)).xyz);
			
			float NdotL = dot(normalDirection, lightDirection);
			if (NdotL <= 0.0) NdotL = 0;
			else NdotL = 1;
			NdotL = smoothstep(0, 0.025f, NdotL);
			*/
			//float3 currentWorldPos = mul(input.modelMatrix, float4(currentRayPos - 0.5, 1));
			float3 currentWorldPos = float4(currentRayPos - 0.5, 1).xyz + input.modelMatrix.xyz;
			depth = getDepth(currentWorldPos);
			float attenuation = getAttenuationRay(currentRayPos, rayDir);
			//float attenuation = calcAO(currentRayPos, rayDir);
			color = float4(output.xyz*attenuation, 1.0f);
			//color = float4(_BaseColor.xyz * ndotl, 0.5f);
			//color = float4(input.pos.x/20000,0,0, 1);
			//float4 worldLightPos = mul(transpose(UNITY_MATRIX_MV), unity_LightPosition[0]);
			//float3 lightDir = normalize(float3(_WorldSpaceLightPos0.xyz));

			//color = half4(lightDir, 1);
		}








			ENDCG
		}
		}
	}
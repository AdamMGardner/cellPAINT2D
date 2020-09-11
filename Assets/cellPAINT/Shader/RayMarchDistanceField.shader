Shader "Custom/RayMarchVolume"
{
	Properties
	{
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

		CGINCLUDE

#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"
#include "AutoLight.cginc"
#pragma target 5.0

	int _GridSize;
	int _MipLevel;
	int _NumRayStepMax;
	float _Threshold;
	sampler3D _VolumeTex;
	sampler2D _MainTex;
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
		int discardInstance : INT0;
		float3 viewDir : C0LOR0;
		float3 objectPos : C0LOR1;
		float3 lightPos : TEXCOORD1;
		centroid float4 pos : SV_POSITION;
		float4x4 modelMatrix : world;
	};

	//v2f vert(uint vertexId : SV_VertexID)
	v2f vert(uint id : SV_VertexID, uint instanceId : SV_InstanceID)
	{
		/*uint id = vertexId % 36;
		uint instanceId = vertexId / 36;*/

		float4x4 modelMatrix = _CubeMatrices[instanceId];
		float4x4 modelMatrixInv = transpose(modelMatrix);

		float3 objectPos = _CubeVertices[_CubeIndices[id]] ;
		float4 worldPos = mul(modelMatrix, float4(objectPos,1));
		float4 instancePos = mul(modelMatrix, float4(0,0,0, 1));
		
		v2f output;
		output.discardInstance = 0;
		if(instancePos.x < 0 ) output.discardInstance = 1;
		output.objectPos = objectPos;
		output.viewDir = mul(modelMatrixInv, worldPos - _WorldSpaceCameraPos);
		output.pos = mul(UNITY_MATRIX_VP, worldPos);
		output.modelMatrix = modelMatrix;
		output.lightPos = mul(modelMatrixInv, -_WorldSpaceLightPos0.xyz);
		TRANSFER_SHADOW(output)
		return output;
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

	float shadeintensity(float3 directionToCamera,float3 normal) {
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

	fixed4 getLigthing(v2f input, float3 normal) {
		float3 lightDirection = normalize(input.lightPos);
		float ndotl2 = saturate(dot(normal, lightDirection.xyz));
		float smootheD_ndotl2 = smoothstep(_lowerBounds, _upperBounds, ndotl2); // smooth interpolation between values
																				// shift matallic value from range [0, 1] to range[0.5, 1]
		smootheD_ndotl2 = smootheD_ndotl2 / 2 + 0.5f; // shift metallic intensity by 0.5
													  //o_FragColor.xyz = metallic * u_baseColor; // modulate final color
													  //float ndotl = smootheD_ndotl2;

		float3 ambient = ShadeSH9(float4(normal, 1.0f));
		float3 diffuse = (smootheD_ndotl2 * (ndotl2 / 2.0 + 0.5f) * _BaseColor.xyz.rgb);
		fixed shadow = SHADOW_ATTENUATION(input);
		fixed4 albedo = tex2D(_MainTex, input.pos.xy);

		float3 lighting = diffuse * shadow + ambient;
		fixed4 output = fixed4(albedo.rgb *_BaseColor * lighting, albedo.w);
		UNITY_APPLY_FOG(input.fogCoord, output);
		return output;
	}

	//void frag_surf_opaque(v2f input, out float4 color : COLOR0, out float depth : DEPTH)
	void frag_surf_opaque(v2f input, out float4 color : SV_Target, out float depth : SV_DepthLessEqual)
	//void frag_surf_opaque(v2f input, out float4 color : COLOR0, out float depth : SV_DepthGreaterEqual)
	//void frag_surf_opaque(v2f input, out float4 color : COLOR0, out float depth : SV_DepthLessEqual)
	{
		color = float4(0.25, 0.25, 0.25, 1);
		depth = input.pos.z;
		
		if (input.discardInstance == 1) discard;

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
				
		//int stepScale = 1;

		//// Mip search
		//for (; currentNumSteps < rayNumSteps; currentNumSteps+= stepScale)
		//{
		//	currentIntensity = sampleVolumeMip(currentRayPos, 1);
		//	if (currentIntensity > 0) break;
		//	currentRayPos += rayStep *stepScale;
		//}

		//if (currentIntensity <= 0) discard;

		//// Restore previous step
		//currentRayPos -= rayStep * stepScale;
		////currentNumSteps -= stepScale * 5;
		
		// Linear search
		for(;currentNumSteps < rayNumSteps; currentNumSteps++)
		{
			currentIntensity = sampleVolumeMip(currentRayPos, _MipLevel);
			if(currentIntensity >= intensityThreshold) break;			
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
		
		float3 currentWorldPos = mul(input.modelMatrix, float4(currentRayPos - 0.5, 1));
		depth = getDepth(currentWorldPos);
		
		fixed4 output = getLigthing(input, normal);

		color = float4(_BaseColor.xyz * ndotl,1.0f);

		float4 worldLightPos = mul(transpose(UNITY_MATRIX_MV), unity_LightPosition[0]);
		float3 lightDir = normalize(float3(_WorldSpaceLightPos0.xyz));

		//color = half4(lightDir, 1);
	}

	ENDCG

	Subshader
	{
		ZTest LEqual
		ZWrite On
		Cull Back
		//Cull Front

		Pass
		{
		Tags{ "LightMode" = "ForwardBase" }
		CGPROGRAM
		#pragma vertex vert		
		#pragma fragment frag_surf_opaque		
		ENDCG
	}
	}

		Fallback off
}

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/RayMarchVolumeCube"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_MipLevel("Mip Level", Int) = 0
		_GridSize("Grid Size", Int) = 0
		[NoScaleOffset] _VolumeTex("Volume Texture", 3D) = "" {}
		_NumRayStepMax("_Num Ray Step Max", Int) = 32
		_Threshold("Intensity Threshold", Range(0, 1)) = 0.5
		_lowerBounds("lower smooth", Range(0, 1)) = 0.1
		_upperBounds("upper smooth", Range(0, 1)) = 0.4
		_lowerBoundsAO("lower smooth ao", Range(0, 1)) = 0.1
		_upperBoundsAO("upper smooth ao", Range(0, 1)) = 0.4
		_NumShades("Number Of Shades", Range(1, 5)) = 1
		_BaseColor("Base Color", Color) = (1, 1, 1, 1)
		_NumSample("AO sample", Int) = 5
		_NumRing("AO ring", Int) = 5

	}

		CGINCLUDE

#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"
#include "AutoLight.cginc"
#pragma target 5.0

	int _GridSize;
	int _MipLevel;
	int _NumRayStepMax;
	int _NumSample;
	int _NumRing;
	float _Threshold;
	sampler3D _VolumeTex;
	sampler2D _MainTex;
	float _NumShades;
	float4 _BaseColor;
	float _lowerBounds;
	float _upperBounds;
	float _lowerBoundsAO;
	float _upperBoundsAO;
	sampler2D_float _CameraDepthTexture;

	matrix Identity =
	{
		{ 1, 0, 0, 0 },
		{ 0, 1, 0, 0 },
		{ 0, 0, 1, 0 },
		{ 0, 0, 0, 1 }
	};

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{		
		int discardInstance : INT0;
		float2 uv_MainTex : TEXCOORD0;
		float3 lightPos : TEXCOORD1;
		float4 scrPos: TEXCOORD2;
		float3 viewDir : C0LOR0;
		float3 objectPos : C0LOR1;
		centroid float4 pos : SV_POSITION;
		float4x4 modelMatrix : world;
	};

	//v2f vert(uint vertexId : SV_VertexID)
	v2f vert(appdata_full v)//uint id : SV_VertexID)//, uint instanceId : SV_InstanceID)
	{
		/*uint id = vertexId % 36;
		uint instanceId = vertexId / 36;*/
		
		float4 vertex = UnityObjectToClipPos(v.vertex);

		float4x4 modelMatrix = unity_ObjectToWorld;// _CubeMatrices[instanceId];//_Object2World
		float4x4 modelMatrixInv = unity_WorldToObject;// transpose(modelMatrix); //_Object2World

		float3 objectPos = v.vertex;// _CubeVertices[_CubeIndices[id]];
		float4 worldPos = mul(modelMatrix, float4(objectPos,1));
		float4 instancePos = mul(modelMatrix, float4(0,0,0, 1));
		
		v2f output;
		output.discardInstance = 0;
		//if(instancePos.x < 0 ) output.discardInstance = 1;
		output.objectPos = objectPos;
		output.viewDir = mul(modelMatrixInv, worldPos - _WorldSpaceCameraPos);
		output.pos = mul(UNITY_MATRIX_VP, worldPos);
		output.scrPos = ComputeScreenPos(worldPos);
		output.modelMatrix = modelMatrix;
		output.lightPos = mul(modelMatrixInv, -_WorldSpaceLightPos0.xyz);
		output.uv_MainTex = v.texcoord;
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

	float3 raycast(float3 rayStart, float3 rayDirection) {
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
		if (currentIntensity < intensityThreshold) return float3(0.,0.,0.);

		rayStep *= 0.5;
		currentRayPos -= rayStep;

		// Binary search
		for (uint j = 0; j < 4; j++)
		{
			rayStep *= 0.5;
			currentIntensity = sampleVolumeMip(currentRayPos, _MipLevel);
			currentRayPos += (currentIntensity >= intensityThreshold) ? -rayStep : rayStep;
		}
		return currentRayPos;
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
		float3 diffuse = (smootheD_ndotl2 * (ndotl2/2.0+0.5f) * _BaseColor.xyz.rgb);
		fixed shadow = SHADOW_ATTENUATION(input);
		fixed4 albedo = tex2D(_MainTex, input.uv_MainTex);

		float3 lighting = diffuse * shadow + ambient;
		fixed4 output = fixed4(albedo.rgb *_BaseColor , albedo.w);
		UNITY_APPLY_FOG(input.fogCoord, output);
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

	float randfloat(float3 myVector) {
		return frac(sin(_Time[0] * dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
	}

	float U_m1_p1() {
		return float(my_rand())*(1.0f / 2147483648.0f) - 1.0f;
	}

	float3 pick_random_point_in_sphere(float3 v ) {
		float x0, x1, x2, x3, d2;
		do {
			x0 = randfloat(v);//float rand = frac(sin(dot(rayStart.xy, float2(12.9898, 78.233))) * 43758.5453);
			x1 = randfloat(v+1);
			x2 = randfloat(v-1);
			x3 = randfloat(v*2);
			d2 = x0*x0 + x1*x1 + x2*x2 + x3*x3;
		} while (d2>1.0f);
		float scale = 1.0f / d2;
		return float3(2 * (x1*x3 + x0*x2)*scale,
			2 * (x2*x3 + x0*x1)*scale,
			(x0*x0 + x3*x3 - x1*x1 - x2*x2)*scale);
	}

	float3 pick_random_point_in_semisphere(float3 v) {
		float3 result = pick_random_point_in_sphere(v);
		float vdotr = dot(result, v);
		float angle = acos(vdotr);
		
		if (vdotr<0) {
			result.x = -result.x;
			result.y = -result.y;
			result.z = -result.z;
		}
		return result;
	}

	float getAtt(float3 position, float raydir, float4x4 modelMatrix, float dataStep, float depth) {
		float3 v = raycast(position, raydir);
		if ((v.x + v.y + v.z) == 0.0f) return 0.0f;
		float vIntensity = sampleVolumeMip(v, _MipLevel);
		float4 worldV = mul(modelMatrix, float4(v - 0.5, 1));
		float d = getDepth(worldV.xyz);
		float a = (d < depth ? 1.0 : 0.0);
		return a;
	}

	float getAttenuationRay(float3 position, float raydir, float4x4 modelMatrix, float dataStep) {
		float intensityThreshold = _Threshold;
		float attenuation = 0.0f;
		float rangeCheck = 1.0f;
		int samplesize = 10;
		float d = 0.0f;
		float currentIntensity = sampleVolumeMip(position, _MipLevel);
		float4 currentWorldPos = mul(modelMatrix, float4(position - 0.5, 1));
		float depth = getDepth(currentWorldPos.xyz);
		for (int i = 0; i < samplesize; i++) {
			attenuation += getAtt( position, raydir + float3(0, 0, dataStep*(i+1)) ,  modelMatrix,  dataStep, depth);
			attenuation += getAtt( position, raydir + float3(0, dataStep*(i + 1),0 ),  modelMatrix,  dataStep, depth);
			attenuation += getAtt( position, raydir + float3(dataStep*(i + 1), 0, 0),  modelMatrix,  dataStep, depth);
			attenuation += getAtt( position, raydir + float3(-dataStep*(i + 1), 0, 0) , modelMatrix, dataStep, depth);
			attenuation += getAtt( position, raydir + float3(0, -dataStep*(i + 1), 0) , modelMatrix, dataStep, depth);
			attenuation += getAtt( position, raydir + float3(0, 0, -dataStep*(i + 1)) , modelMatrix, dataStep, depth);
		}
		return  clamp(1.0 - attenuation / (samplesize*2), 0.0, 1.0);// clamp(1.0 - attenuation, 0.0, 1.0);
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

	float compareValue(float3 pos, float current) {
		float occ = 0.0f;
		float dd = sampleVolumeMip(pos, _MipLevel);
		if (dd >= _Threshold) {
			if (dd > current) {
				occ = 1.0f;
			}
		}
		return occ;
	}
	float3 getPositionAlong(float3 position, float dataStep, float3 dir)
	{
		float ao = 0.0f;
		float current_value = sampleVolumeMip(position, _MipLevel);
		ao += compareValue(position + float3(dataStep, dataStep, dataStep)*dir, current_value);
		ao += compareValue(position + float3(dataStep, dataStep, dataStep)*dir, current_value);
		ao += compareValue(position + float3(dataStep, dataStep, dataStep)*dir, current_value);
		ao += compareValue(position + float3(-dataStep, dataStep, dataStep)*dir, current_value);
		ao += compareValue(position + float3(dataStep, -dataStep, dataStep)*dir, current_value);
		ao += compareValue(position + float3(dataStep, dataStep, -dataStep)*dir, current_value);
		return clamp(ao / 12.0f,0.0f,1.0f);
	}

	float3 getPositionAround(float3 position, float dataStep,float3 dir)
	{
		float ao = 0.0f;
		float current_value = sampleVolumeMip(position, 0);
		ao += compareValue(position + float3(dataStep, 0, 0), current_value);
		ao += compareValue(position + float3(-dataStep, 0, 0), current_value);
		ao += compareValue(position + float3(0, dataStep, 0), current_value);
		ao += compareValue(position + float3(0, -dataStep, 0), current_value);
		ao += compareValue(position + float3(0, 0, dataStep), current_value);
		ao += compareValue(position + float3(0, 0, -dataStep), current_value);
		return ao/6.0f;
	}

	float calcAO(float3 pos, float3 nor, float size, int nstep)
	{
		float occ = 0.0;
		float ao = 0.0;
		float sca = 1.0;
		int n = 0;
		int nring = 5;
		float current_value = sampleVolumeMip(pos, 0);
		float3 rv = pick_random_point_in_semisphere(nor);
		for (int i = 0; i < _NumSample; i++)
		{
			for (int j = 0; j < _NumRing; j++)
			{
				
				//float hr = 0.01 + 0.12*float(j) / 4.0;
				//float3 aopos = nor * hr + pos;
				//aopos = rv*size*float(j) + pos;
				//float dd = map(aopos).x; distance to closest ?
				//float dd = sampleVolumeMip(aopos, 0);
				//if (dd >= _Threshold) {
				//	if (dd > current_value) {
				//		occ += 1.0 * sca;
				//		n++;
				//	}
				//}
				//ao += getPositionAlong(pos, size*float(j), rv);
				ao += getPositionAround(pos, size*float(j), rv);
				//ao += compareValue(pos + size*float(j)*nor, current_value);
				//occ += -(dd - hr)*sca;
				//sca *= 0.95;
			}
		}
		return clamp(1.0 - ao / (float)(nstep*nring), 0.0, 1.0);
	}

	float getAO(float depth,float3 position, float3 raydir, int nStep, float4x4 modelMatrix) {
		float ao = 0.0f;
		for (int i = 0; i < nStep; i++) {
			float3 rv = pick_random_point_in_semisphere(raydir);
			float3 invertRayPos = raycast(position, rv);
			if ((invertRayPos.x + invertRayPos.y + invertRayPos.z) == 0.0f) //no occlusion ? ;
			{
				ao += 0.0f;
			}
			else
			{
				//compare Z
				float4 rvWorldPos = mul(modelMatrix, float4(invertRayPos - 0.5, 1));
				float rvdepth = getDepth(rvWorldPos);
				float diff = depth - rvdepth;
				//ao += (abs(diff) < 0.001f ? 1 : 0.0);
				ao += (depth > rvdepth ? 1 : 0.0);
			}
		}
		return clamp(ao/(float)nStep,0,1);
	}

	//void frag_surf_opaque(v2f input, out float4 color : COLOR0, out float depth : DEPTH)
	void frag_surf_opaque(v2f input, out float4 color : SV_Target, out float depth : SV_DepthLessEqual)// SV_DepthLessEqual)//SV_DepthLessEqual)
	//void frag_surf_opaque(v2f input, out float4 color : COLOR0, out float depth : SV_DepthLessEqual)
	{
		color = float4(0.25, 0.25, 0.25, 1);
		depth = input.pos.z;
		
		if (input.discardInstance == 1) discard;
		
		float3 rayDir = normalize(input.viewDir);
		float3 rv = pick_random_point_in_semisphere(rayDir);
			
		float3 currentRayPos = raycast(input.objectPos, rayDir);
		//raycast around from the screen
		if ((currentRayPos.x+ currentRayPos.y+ currentRayPos.z)==0.0f) discard;

		float4 currentWorldPos = mul(input.modelMatrix, float4(currentRayPos - 0.5, 1));
		//depth = getDepth(currentWorldPos);

		float texelSize = 1.0f / _GridSize;
		float3 normal = getNormal(currentRayPos, texelSize);
		
		float3 lightDirection = normalize(input.lightPos);
		//float ao = getAO(depth, input.objectPos, rayDir,120, input.modelMatrix);

		float ndotl = pow(max(0.0, dot(rayDir, normal)), 0.4);
		
		float metallic = dot(rayDir, normal);
		float smoothed_metallic = smoothstep(_lowerBounds, _upperBounds, metallic); // smooth interpolation between values
												   // shift matallic value from range [0, 1] to range[0.5, 1]
		metallic = smoothed_metallic / 2 + 0.5f; // shift metallic intensity by 0.5
		//o_FragColor.xyz = metallic * u_baseColor; // modulate final color
		ndotl = metallic;
		
		fixed4 output = getLigthing(input, normal);

		//float4 currentWorldPos = mul(input.modelMatrix, float4(currentRayPos - 0.5, 1));
		//depth = getDepth(currentWorldPos);

		//float3 lightDirection = normalize(input.lightPos);
		//float ao =  getAttenuationRay(input.objectPos, mul(unity_WorldToObject, float4(lightDirection,1)).xyz, input.modelMatrix, texelSize / 10);
		
		//float ao = getAO(depth, input.objectPos, rayDir, 56, input.modelMatrix);
		float3 lookat = cross(normal, rayDir);
		
		float ao = calcAO(currentRayPos, normalize(-rayDir), texelSize, 4);
		//float ao = getAO(depth, input.objectPos, rayDir, 120, input.modelMatrix);
		//float ao = getAttenuation(input.objectPos, normalize(lookat));

		ao = smoothstep(_lowerBoundsAO, _upperBoundsAO, ao);
		float ndotl2 = saturate(dot(normal, lightDirection.xyz));
		if (ndotl2 < 0.5f) ndotl2 = 0.0f;
		float2 screenPosition = (input.scrPos.xy / input.scrPos.w);
		//offset and ray cast again
		//float ao = getAttenuationRay(input.objectPos, rayDir, input.modelMatrix, texelSize/10);
		
		float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenPosition.xy);
		depth = getDepth(currentWorldPos);
		float edgeDetection = 1;// (dot(rayDir, normal) > 0.5) ? 1 : 0.5;
		color = output * ao * edgeDetection * metallic;// 
		//float4 worldLightPos = mul(transpose(UNITY_MATRIX_MV), unity_LightPosition[0]);
		//float3 lightDir = normalize(float3(_WorldSpaceLightPos0.xyz));
		
		//blur the resulting ao ?
		//multipass
		//color = half4(lightDirection, 1);// *smoothstep(_lowerBounds, _upperBounds, ao);
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

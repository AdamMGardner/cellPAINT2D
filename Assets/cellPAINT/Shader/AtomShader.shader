// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AtomShader" 
{
	CGINCLUDE

	#include "UnityCG.cginc"
		
	uniform float scale;	
	uniform StructuredBuffer<float4> atomDataBuffer;
	uniform float4 pos;
	uniform float4 rot;

	struct vs2gs
	{
		float4 pos : SV_POSITION;					
	};
			
	struct gs2fs
	{
		centroid float4 pos : SV_Position;
		nointerpolation float radius : FLOAT0;				
		float2 uv: TEXCOORD0;						
	};	

	float3 QuaternionTransform(float4 q, float3 v)
	{
		float3 t = 2 * cross(q.xyz, v);
		return v + q.w * t + cross(q.xyz, t);
	}

	vs2gs VS(uint id : SV_VertexID)
	{
		float4 atomData = atomDataBuffer[id];				   
			    
		vs2gs output;				    		    
		//output.pos = float4(atomData.xyz*scale, 1);   
		output.pos = pos + float4(QuaternionTransform(rot, atomData.xyz) * scale,0);
		return output;
	}
			
	[maxvertexcount(4)]
	void GS(point vs2gs input[1], inout TriangleStream<gs2fs> triangleStream)
	{
		float radius = scale * 1.5f;
		float4 pos = UnityObjectToClipPos(input[0].pos);
		float4 offset = mul(UNITY_MATRIX_P, float4(radius, radius, 0, 1));

		gs2fs output;					
		output.radius = radius;

		//*****//

		output.uv = float2(1.0f, 1.0f);
		output.pos = pos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(1.0f, -1.0f);
		output.pos = pos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);	
								
		output.uv = float2(-1.0f, 1.0f);
		output.pos = pos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(-1.0f, -1.0f);
		output.pos = pos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);	
	}
			
	void FS (gs2fs input, out float4 color : COLOR0, out float depth : sv_depthgreaterequal) 
	{			
		// Find coverage mask
		/*coverage_mask = 0;

		float2 sample_uv;
		for(int i = 0; i < 8; i++)
		{
			sample_uv = EvaluateAttributeAtSample(input.uv, i);
			if(dot(sample_uv, sample_uv) <= 1) coverage_mask += 1 << (i);
		}

		if(coverage_mask == 0) discard;
		*/
		// Find normal
		float lensqr = dot(input.uv, input.uv);   
		if (lensqr > 1) discard;
		float3 normal = normalize(float3(input.uv, sqrt(1.0 - lensqr)));		
				
		// Find depth
		//float eyeDepth = LinearEyeDepth(input.pos.z);
		//eyeDepth += (coverage_mask == 255) ?  input.radius * (1-normal.z) : input.radius;
		//depth = 1 / (eyeDepth * _ZBufferParams.z) - _ZBufferParams.w / _ZBufferParams.z;
		
		float eyeDepth = LinearEyeDepth(input.pos.z) + input.radius * (1 - normal.z);
		depth = 1 / (eyeDepth * _ZBufferParams.z) - _ZBufferParams.w / _ZBufferParams.z;

		// Find color
		color = float4(0.8,0,0, 1);
	}

	/***/
	vs2gs VS_2(uint id : SV_VertexID)
	{
		float4 atomData = atomDataBuffer[id];				   
			    
		vs2gs output;				    		    
		output.pos = UnityObjectToClipPos(float4(atomData.xyz,1)); 
		return output;
	}

	void FS_2 (vs2gs input,  out float4 color : COLOR0) 
	{	
		//discard;		
		// Find color
		color = float4(1,0,0,1);				
	}



	ENDCG
	
	SubShader 
	{			
		Pass
		{		
			ZWrite On

			CGPROGRAM	
					
			#include "UnityCG.cginc"			
			
			#pragma vertex VS			
			#pragma fragment FS							
			#pragma geometry GS	
				
			#pragma only_renderers d3d11		
			#pragma target 5.0											
				
			ENDCG	
		}			
	}
	Fallback Off
}	
/// <summary>
/// Name: Unlit/Transparent
/// Description: Used for GUI.
/// Author: Joseph Cameron
/// </summary>

//#region change log
//Name: Joseph Cameron
//Description: initial implementation
//Date: October 10th, 2014
//
//Name: Joseph Cameron
//Description: reformatted code
//Date: October 30th, 2014
//
//Name: Joseph Cameron
//Description: added const to input arguments
//Date: November 20th, 2014
//
//Name: Joseph Cameron
//Description: cleaned up code
//Date: December 8th, 2014
//#endregion
Shader "GUI/UnlitTransparent" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Color   ("Material color", Color   ) = (1,1,1,1)
	
	}

	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		
		LOD 100
	
		ZWrite On
		//Blend SrcAlpha OneMinusSrcAlpha
		cull off 
	
		Pass 
		{  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#pragma target 2.0

			//************************
			// Data formats & uniforms
			//************************
			uniform sampler2D _MainTex    ;
			uniform float4    _MainTex_ST ;
			uniform fixed4    _Color      ;
			
			struct VertexInput 
			{
				float4 pos : POSITION0 ;
				float2 uv  : TEXCOORD0 ;
				
			};

			struct FragmentInput 
			{
				float4 pos : POSITION0 ;
				half2  uv  : TEXCOORD0 ;
				
			};

			//********
			// Shaders
			//********	
			FragmentInput vert (const VertexInput input)
			{
				FragmentInput output = (FragmentInput)0;
				{
					output.pos = mul(UNITY_MATRIX_MVP, input.pos);
					output.uv  = TRANSFORM_TEX(input.uv, _MainTex);
				
				}
				
				return output;
				
			}
			
			fixed4 frag (const FragmentInput input) : COLOR0
			{
				fixed4 output = (fixed4)0;
				{
					fixed2 uv = input.uv;
					uv[0]+=_Time;
				
					output  = tex2D(_MainTex, uv);
					output *= _Color;
					
					//if(output.r <= 0)
					//	discard;
							
				}
				
				return output;
				
			}
			
			ENDCG
			
		}
		
	}

}











/* This shader requires Unity Pro - if you are receiving an error
that "No subshaders can run on this card", or if this shader is pink and
doesn't present slots for the textures and properties, you will need to use a different shader
that is compatible with your version of Unity 
*/

Shader "River Shader" 
{
	Properties 
	{
		_BumpAmt  ("Distortion", range (0,128)) = 10
		_MainTex ("Tint Color (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap (RGB)", 2D) = "bump" {}
	}
	
	SubShader 
	{
		Tags { "Queue" = "Transparent-110" }
		
		GrabPass 
		{							
			Name "BASE"
			Tags { "LightMode" = "Always" }
		}
		
		Pass 
		{
			Name "BASE"
			Tags { "LightMode" = "Always" }
			Cull Back
			
		CGPROGRAM
		#pragma exclude_renderers gles
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		#pragma fragmentoption ARB_fog_exp2
		#include "UnityCG.cginc"

		sampler2D _GrabTexture : register(s0);
		float4 _GrabTexture_TexelSize;
		sampler2D _BumpMap : register(s1);
		sampler2D _MainTex : register(s2);

		struct v2f 
		{
			float4 uvgrab : TEXCOORD0;
			float2 uvbump : TEXCOORD1;
			float2 uvmain : TEXCOORD2;
		};

		uniform float _BumpAmt;

		half4 frag( v2f i ) : COLOR
		{
			half2 bump = tex2D( _BumpMap, i.uvbump + _Time * 0.5f).rg * 2 - 1;
			float2 offset = bump * _BumpAmt;

			offset *= _GrabTexture_TexelSize.xy;

			i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
			
			half4 col = tex2Dproj( _GrabTexture, i.uvgrab.xyw );
			half4 tint = tex2D( _MainTex, i.uvmain );
			
			return col * tint;
		}

		ENDCG
			SetTexture [_GrabTexture] {}	// Texture we grabbed in the pass above
			SetTexture [_BumpMap] {}		// Perturbation bumpmap
			SetTexture [_MainTex] {}		// Color tint
		}
	}
}

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
*
* EASYFader.shader
* Create a very nice and easy to use screen fade.
*
* Version 1.0.0
*
* Developed by Vortex Game Studios LTDA ME. (http://www.vortexstudios.com)
* Authors:		Alexandre Ribeiro de Sa (@alexribeirodesa)
*
*/

Shader "Vortex Game Studios/Filters/EASY Fader Effect" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Texture("Texture", 2D) = "white"
		[MaterialToggle] _TextureFill("TextureFill", float) = 1.0
		_TextureAspectX("Texture Aspect X", float) = 1.0
		_TextureAspectY("Texture Aspect Y", float) = 1.0
		_Value("Value", float) = 0.0

		_Type("Type", float) = 0.0
	}

	SubShader{
		Pass{
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			sampler2D _Texture;
			float _TextureFill;
			float _TextureAspectX;
			float _TextureAspectY;
			float _Value;
			float _Type;

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			fixed3 BrightnessContrast(in fixed3 RGB, fixed brightness, fixed contrast) {
				return fixed3((RGB.rgb - 0.5) * contrast + 0.5 + brightness);
			}

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				fixed4 output;
				output.rgb = tex2D(_MainTex, i.uv);

				if (_Type == 0.0) {				// Default
					_Value = _Value * _Color.a;
					output.rgb = fixed3(lerp(output.rgb, _Color.rgb, _Value));
				} else if (_Type == 1.0) {		// Bright
					// Converte a cor apra escalas de cinza, assim descobrimos se ela é uma cor clara ou escura :)
					_Value = _Value * _Color.a;
					fixed grayScale = dot(fixed3(0.2126, 0.7152, 0.0722), _Color.rgb);
					if (grayScale > 0.5) {
						output.rgb = BrightnessContrast(output.rgb, _Value, 1.0);
						output.rgb = clamp(output.rgb, 0.0, 1.0) - (_Value * (fixed3(1, 1, 1) - _Color.rgb));
					} else {
						output.rgb = BrightnessContrast(output.rgb, -_Value, 1.0);
						output.rgb = clamp(output.rgb, 0.0, 1.0) + (_Value *_Color.rgb);
					}
				} else {						// Texture
					fixed g;
					if (_TextureFill > 0.0)
						g = tex2D(_Texture, half2(i.uv.x, 1.0 - i.uv.y)).g;
					else
						g = tex2D(_Texture, half2(i.uv.x, 1.0 - i.uv.y) * half2(_TextureAspectX, _TextureAspectY)).g; 

					if ( g <= _Value) {
						output.rgb = fixed3(lerp(output.rgb, _Color.rgb, _Color.a));
					}
				}
				
				output.a = 1.0;
				return output;
			}
			
			ENDCG
		}
	}
	Fallback off
}
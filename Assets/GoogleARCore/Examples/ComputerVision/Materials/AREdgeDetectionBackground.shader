Shader "AREdgeDetectionBackground"
{
	Properties
	{
        _OverlayPercentage ("Overlay Percentage", Range (0.0, 1.0)) = 0.5
	}

	// For GLES3
	SubShader
	{
		Pass
		{
			ZWrite Off

			GLSLPROGRAM

			#pragma only_renderers gles3

			#ifdef SHADER_API_GLES3
			#extension GL_OES_EGL_image_external_essl3 : require
			#endif

			uniform vec4 _UvTopLeftRight;
			uniform vec4 _UvBottomLeftRight;

			#ifdef VERTEX

			#define kPortrait 1.0
			#define kPortraitUpsideDown 2.0
			#define kLandscapeLeft 3.0
			#define kLandscapeRight 4.0

			varying vec2 textureCoord;

			void main()
			{
				#ifdef SHADER_API_GLES3
				vec2 uvTop = mix(_UvTopLeftRight.xy, _UvTopLeftRight.zw, gl_MultiTexCoord0.x);
				vec2 uvBottom = mix(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, gl_MultiTexCoord0.x);
				textureCoord = mix(uvTop, uvBottom, gl_MultiTexCoord0.y);

				gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
				#endif
			}

			#endif

			#ifdef FRAGMENT
			varying vec2 textureCoord;
			uniform samplerExternalOES _MainTex;
			uniform sampler2D _ImageTex;
            uniform float _OverlayPercentage;

			void main()
			{
				#ifdef SHADER_API_GLES3
				if (textureCoord.x < _OverlayPercentage)
					gl_FragColor = texture(_MainTex, textureCoord);
				else
				{
					vec4 color = texture2D(_ImageTex, textureCoord);
					gl_FragColor.xyz = color.xxx;
				}
				#endif
			}

			#endif

			ENDGLSL
		}
	}

	FallBack Off
}

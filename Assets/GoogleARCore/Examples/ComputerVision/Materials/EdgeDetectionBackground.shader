Shader "EdgeDetectionBackground"
{
    Properties
    {
        _ImageTex ("Texture", 2D) = "white" {}
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
            uniform vec4 _ImageTex_ST;

            #ifdef VERTEX

            varying vec2 textureCoord;

            void main()
            {
                #ifdef SHADER_API_GLES3
                vec2 transformedUV = gl_MultiTexCoord0.xy * _ImageTex_ST.xy + _ImageTex_ST.zw;
                vec2 uvTop = mix(_UvTopLeftRight.xy, _UvTopLeftRight.zw, transformedUV.x);
                vec2 uvBottom = mix(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, transformedUV.x);
                textureCoord = mix(uvTop, uvBottom, transformedUV.y);

                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
                #endif
            }

            #endif

            #ifdef FRAGMENT
            varying vec2 textureCoord;
            uniform sampler2D _ImageTex;

            void main()
            {
                #ifdef SHADER_API_GLES3

                vec4 color = texture2D(_ImageTex, textureCoord);
                gl_FragColor = vec4(color.r, color.r, color.r, 1.0);
                #endif
            }

            #endif

            ENDGLSL
        }
    }

    // For running the computer vision sample in the Unity Editor on a desktop using
    // ARCore Instant Preview.
    Subshader
    {
      Pass
      {
        ZWrite Off

        CGPROGRAM

        #pragma exclude_renderers gles3
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        uniform float4 _UvTopLeftRight;
        uniform float4 _UvBottomLeftRight;
        uniform float4 _ImageTex_ST;

        struct appdata
        {
          float4 vertex : POSITION;
          float2 uv : TEXCOORD0;
        };

        struct v2f
        {
          float2 uv : TEXCOORD0;
          float4 vertex : SV_POSITION;
        };

        v2f vert(appdata v)
        {
          float2 transformedUV = v.uv * _ImageTex_ST.xy + _ImageTex_ST.zw;
          float2 uvTop = lerp(_UvTopLeftRight.xy, _UvTopLeftRight.zw, transformedUV.x);
          float2 uvBottom = lerp(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, transformedUV.x);

          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = lerp(uvTop, uvBottom, transformedUV.y);

          // Instant preview's texture is transformed differently.
          o.uv.x = 1.0 - o.uv.x;

          return o;
        }

        sampler2D _ImageTex;

        fixed4 frag(v2f i) : SV_Target
        {
          fixed4 color = tex2D(_ImageTex, i.uv);
          return fixed4(color.r, color.r, color.r, 1.0);
        }
        ENDCG
      }
    }
    FallBack Off
}


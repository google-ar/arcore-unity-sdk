Shader "ARCore/ARBackground"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _UvTopLeftRight ("UV of top corners", Vector) = (0, 1, 1, 1)
        _UvBottomLeftRight ("UV of bottom corners", Vector) = (0 , 0, 1, 0) 
    }

    // For GLES3 or GLES2 on device
    SubShader
    {
        Pass
        {
            ZWrite Off

            GLSLPROGRAM

            #pragma only_renderers gles3 gles

            #ifdef SHADER_API_GLES3
            #extension GL_OES_EGL_image_external_essl3 : require
            #else
            #extension GL_OES_EGL_image_external : require
            #endif

            uniform vec4 _UvTopLeftRight;
            uniform vec4 _UvBottomLeftRight;

            #ifdef VERTEX

            varying vec2 textureCoord;

            void main()
            {
                vec2 uvTop = mix(_UvTopLeftRight.xy, _UvTopLeftRight.zw, gl_MultiTexCoord0.x);
                vec2 uvBottom = mix(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, gl_MultiTexCoord0.x);
                textureCoord = mix(uvTop, uvBottom, gl_MultiTexCoord0.y);

                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            }

            #endif

            #ifdef FRAGMENT
            varying vec2 textureCoord;
            uniform samplerExternalOES _MainTex;

            void main()
            {
                #ifdef SHADER_API_GLES3
                gl_FragColor = texture(_MainTex, textureCoord);
                #else
                gl_FragColor = textureExternal(_MainTex, textureCoord);
                #endif
            }

            #endif

            ENDGLSL
        }
    }

  // For Instant Preview
  Subshader
  {
    Pass
    {
      ZWrite Off

      CGPROGRAM

      #pragma exclude_renderers gles3 gles
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      uniform float4 _UvTopLeftRight;
      uniform float4 _UvBottomLeftRight;

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
        float2 uvTop = lerp(_UvTopLeftRight.xy, _UvTopLeftRight.zw, v.uv.x);
        float2 uvBottom = lerp(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, v.uv.x);

        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = lerp(uvTop, uvBottom, v.uv.y);

        // Instant preview's texture is transformed differently.
        o.uv = o.uv.yx;
        o.uv.x = 1.0 - o.uv.x;

        return o;
      }

      sampler2D _MainTex;

      fixed4 frag(v2f i) : SV_Target
      {
        return tex2D(_MainTex, i.uv);
      }
      ENDCG
    }
  }

  FallBack Off
}

//-----------------------------------------------------------------------
// <copyright file="ARBackground.shader" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

Shader "ARCore/ARBackground"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _UvTopLeftRight ("UV of top corners", Vector) = (0, 1, 1, 1)
        _UvBottomLeftRight ("UV of bottom corners", Vector) = (0 , 0, 1, 0)
    }

    // For GLES3 or GLES2 on device
    SubShader
    {
        Pass
        {
            ZWrite Off
            Cull Off

            GLSLPROGRAM

            #pragma only_renderers gles3 gles

            // #ifdef SHADER_API_GLES3 cannot take effect because
            // #extension is processed before any Unity defined symbols.
            // Use "enable" instead of "require" here, so it only gives a
            // warning but not compile error when the implementation does not
            // support the extension.
            #extension GL_OES_EGL_image_external_essl3 : enable
            #extension GL_OES_EGL_image_external : enable

            uniform vec4 _UvTopLeftRight;
            uniform vec4 _UvBottomLeftRight;

            // Use the same method in UnityCG.cginc to convert from gamma to
            // linear space in glsl.
            vec3 GammaToLinearSpace(vec3 color)
            {
                return color *
                    (color * (color * 0.305306011 + 0.682171111) + 0.012522878);
            }

            #ifdef VERTEX

            varying vec2 textureCoord;
            varying vec2 uvCoord;

            void main()
            {
                vec2 uvTop = mix(_UvTopLeftRight.xy,
                                 _UvTopLeftRight.zw,
                                 gl_MultiTexCoord0.x);
                vec2 uvBottom = mix(_UvBottomLeftRight.xy,
                                    _UvBottomLeftRight.zw,
                                    gl_MultiTexCoord0.x);
                textureCoord = mix(uvTop, uvBottom, gl_MultiTexCoord0.y);
                uvCoord = vec2(gl_MultiTexCoord0.x, gl_MultiTexCoord0.y);

                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            }

            #endif

            #ifdef FRAGMENT
            varying vec2 textureCoord;
            varying vec2 uvCoord;
            uniform samplerExternalOES _MainTex;
            uniform sampler2D _TransitionIconTex;
            uniform vec4 _TransitionIconTexTransform;
            uniform float _Brightness;

            void main()
            {
                vec3 mainTexColor;

                #ifdef SHADER_API_GLES3
                mainTexColor = texture(_MainTex, textureCoord).rgb;
                #else
                mainTexColor = textureExternal(_MainTex, textureCoord).rgb;
                #endif

                if (_Brightness < 1.0)
                {
                    mainTexColor = mainTexColor * _Brightness;

                    if (_TransitionIconTexTransform.x > 0.0 &&
                        _TransitionIconTexTransform.z > 0.0)
                    {
                        vec2 uvCoordTex = vec2(uvCoord.x *
                                               _TransitionIconTexTransform.x +
                                               _TransitionIconTexTransform.y,
                                               uvCoord.y *
                                               _TransitionIconTexTransform.z +
                                               _TransitionIconTexTransform.w);

                        vec4 transitionColor = vec4(0.0);
                        if (uvCoordTex.x >= 0.0 &&
                            uvCoordTex.x <= 1.0 &&
                            uvCoordTex.y >= 0.0 &&
                            uvCoordTex.y <= 1.0)
                        {
                            transitionColor = texture2D(_TransitionIconTex,
                                                        uvCoordTex);
                        }

                        if (transitionColor.a > 0.0)
                        {
                            mainTexColor = mix(transitionColor.rgb,
                                               mainTexColor,
                                               _Brightness);
                        }
                    }
                }

#ifndef UNITY_COLORSPACE_GAMMA

                mainTexColor = GammaToLinearSpace(mainTexColor);
#endif
                gl_FragColor = vec4(mainTexColor, 1.0);
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
                float2 uvTop = lerp(_UvTopLeftRight.xy,
                                    _UvTopLeftRight.zw, v.uv.x);
                float2 uvBottom = lerp(_UvBottomLeftRight.xy,
                                       _UvBottomLeftRight.zw, v.uv.x);

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

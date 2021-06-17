//-----------------------------------------------------------------------
// <copyright file="OcclusionImageEffect.shader" company="Google LLC">
//
// Copyright 2020 Google LLC
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

Shader "Hidden/OcclusionImageEffect"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}  // Depth texture.
        _UvTopLeftRight ("UV of top corners", Vector) = (0, 1, 1, 1)
        _UvBottomLeftRight ("UV of bottom corners", Vector) = (0 , 0, 1, 0)
        _OcclusionTransparency ("Maximum occlusion transparency", Range(0, 1)) = 1
        _OcclusionOffsetMeters ("Occlusion offset [meters]", Float) = 0
        _TransitionSizeMeters ("Transition size [meters]", Float) = 0.05
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE

        #include "UnityCG.cginc"

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

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }
        ENDCG

        // Pass #0 renders an auxilary buffer - occlusion map that indicates the
        // regions of virtual objects that are behind real geometry.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../../../../SDK/Materials/ARCoreDepth.cginc"

            sampler2D _CameraDepthTexture;
            sampler2D _BackgroundTexture;
            bool _UseDepthFromPlanes;

            float _TransitionSizeMeters;

            fixed4 frag (v2f i) : SV_Target
            {
                float depthMeters = 0.0;
                if (_UseDepthFromPlanes)
                {
                    depthMeters = tex2Dlod(_CurrentDepthTexture, float4(i.uv, 0, 0)).r
                                    * ARCORE_FLOAT_TO_SHORT;
                    depthMeters *= ARCORE_DEPTH_SCALE;
                }
                else
                {
                    float2 depthUv = ArCoreDepth_GetUv(i.uv);
                    depthMeters = ArCoreDepth_GetMeters(depthUv);
                }

                float virtualDepth = LinearEyeDepth(
                    SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv)) -
                    _OcclusionOffsetMeters;

                // Far plane minus near plane.
                float maxVirtualDepth =
                    _ProjectionParams.z - _ProjectionParams.y;

                float occlusionAlpha =
                    1.0 - saturate(0.5 * (depthMeters - virtualDepth) /
                    (_TransitionSizeMeters * virtualDepth) + 0.5);

                // Masks out only the fragments with virtual objects.
                occlusionAlpha *= saturate(maxVirtualDepth - virtualDepth);

                // At this point occlusionAlpha is equal to 1.0 only for fully
                // occluded regions of the virtual objects.
                fixed4 background = tex2D(_BackgroundTexture, i.uv);

                return fixed4(background.rgb, occlusionAlpha);
            }
            ENDCG
        }

        // Pass #1 combines virtual and real cameras based on the occlusion map.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _OcclusionMap;
            sampler2D _BackgroundTexture;

            fixed _OcclusionTransparency;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 input = tex2D(_MainTex, i.uv);
                fixed4 background = tex2D(_BackgroundTexture, i.uv);

                // 0.0 - fully visible region, 1.0 - fully occluded region.
                fixed occlusionAlpha = tex2D(_OcclusionMap, i.uv).a;

                // Clips occlusion if we want to partially show occluded object.
                occlusionAlpha = min(occlusionAlpha, _OcclusionTransparency);

                return lerp(input, background, occlusionAlpha);
            }
            ENDCG
        }
    }
}

// Copyright 2017 Google LLC. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Don't remove the following line. It is used to bypass Unity
// upgrader change. This is necessary to make sure the shader
// continues to compile on Unity 5.2
// UNITY_SHADER_NO_UPGRADE

Shader "ARCore/PointCloud" {
    Properties{
        [PerRendererData] _Color("PointCloud Color", Color) = (0.121, 0.737, 0.823, 1.0)
        [HideInInspector][PerRendererData] _ScreenWidth("", Int) = 1440
        [HideInInspector][PerRendererData] _ScreenHeight("", Int) = 2560
    }
    SubShader{
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float size : PSIZE;
                float4 center : TEXCOORD0;
                float2 radius : TEXCOORD1;
            };

            fixed4 _Color;
            int _ScreenWidth;
            int _ScreenHeight;

            // It's hard to have both the SV_POSITION and VPOS in the same vertex-to-fragment structure.
            // So the vertex shader outputS the SV_POSITION as a separate “out” variable.
            v2f vert(appdata v, out float4 vertex : SV_POSITION)
            {
                v2f o;
                vertex = UnityObjectToClipPos(v.vertex);

                // Converts center.xy into [0,1] range then mutiplies them with screen size.
                o.center = ComputeScreenPos(vertex);
                o.center.xy /= o.center.w;
                o.center.x *= _ScreenWidth;
                o.center.y *= _ScreenHeight;

                o.size = v.uv.x;
                o.radius = v.uv;
                return o;
            }

            // vpos contains the integer coordinates of the current pixel, which is used
            // to caculate the distance between current pixel and center of the point.
            fixed4 frag(v2f i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
            {
                float dis = distance(vpos.xy, i.center.xy);
                if (dis > i.radius.x / 2)
                {
                    discard;
                }
                return _Color;
            }
            ENDCG
        }
    }
}

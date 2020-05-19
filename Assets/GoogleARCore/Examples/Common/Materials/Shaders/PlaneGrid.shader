//-----------------------------------------------------------------------
// <copyright file="PlaneGrid.shader" company="Google LLC">
//
// Copyright 2019 Google LLC. All Rights Reserved.
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

Shader "ARCore/PlaneGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridColor ("Grid Color", Color) = (1.0, 1.0, 0.0, 1.0)
        _PlaneNormal ("Plane Normal", Vector) = (0.0, 0.0, 0.0)
        _UvRotation ("UV Rotation", float) = 30
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest on
        ZWrite off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GridColor;
            float3 _PlaneNormal;
            fixed _UvRotation;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                fixed cosr = cos(_UvRotation);
                fixed sinr = sin(_UvRotation);
                fixed2x2 uvrotation = fixed2x2(cosr, -sinr, sinr, cosr);

                // Construct two vectors that are orthogonal to the normal.
                // This arbitrary choice is not co-linear with either horizontal
                // or vertical plane normals.
                const float3 arbitrary = float3(1.0, 1.0, 0.0);
                float3 vec_u = normalize(cross(_PlaneNormal, arbitrary));
                float3 vec_v = normalize(cross(_PlaneNormal, vec_u));

                // Project vertices in world frame onto vec_u and vec_v.
                float2 plane_uv = float2(dot(v.vertex.xyz, vec_u), dot(v.vertex.xyz, vec_v));
                float2 uv = plane_uv * _MainTex_ST.xy;
                o.uv = mul(uvrotation, uv);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return fixed4(_GridColor.rgb, col.r * i.color.a);
            }
            ENDCG
        }
    }
}

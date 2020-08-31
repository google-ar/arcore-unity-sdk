//-----------------------------------------------------------------------
// <copyright file="ARCoreDepth.cginc" company="Google LLC">
//
// Copyright 2020 Google LLC. All Rights Reserved.
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

#define ARCORE_DEPTH_SCALE 0.001        // mm to m
#define ARCORE_MAX_DEPTH_MM 8191.0
#define ARCORE_FLOAT_TO_5BITS 31        // (0.0, 1.0) -> (0, 31)
#define ARCORE_FLOAT_TO_6BITS 63        // (0.0, 1.0) -> (0, 63)
#define ARCORE_RGB565_RED_SHIFT 2048    // left shift 11 bits
#define ARCORE_RGB565_GREEN_SHIFT 32    // left shift 5 bits
#define ARCORE_BLEND_FADE_RANGE 0.01

sampler2D _CurrentDepthTexture;
uniform float4 _CurrentDepthTexture_TexelSize;
uniform float4 _UvTopLeftRight;
uniform float4 _UvBottomLeftRight;
uniform float _OcclusionBlendingScale;
uniform float _OcclusionOffsetMeters;

// Calculates depth texture UV given screen-space UV. Uses _UvTopLeftRight and _UvBottomLeftRight.
inline float2 ArCoreDepth_GetUv(float2 uv)
{
    float2 uvTop = lerp(_UvTopLeftRight.xy, _UvTopLeftRight.zw, uv.x);
    float2 uvBottom = lerp(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, uv.x);
    return lerp(uvTop, uvBottom, uv.y);
}

// Returns depth value in meters for a given depth texture UV. Uses _CurrentDepthTexture.
inline float ArCoreDepth_GetMeters(float2 uv)
{
    // The depth texture uses TextureFormat.RGB565.
    float4 rawDepth = tex2Dlod(_CurrentDepthTexture, float4(uv, 0, 0));
    float depth = (rawDepth.r * ARCORE_FLOAT_TO_5BITS * ARCORE_RGB565_RED_SHIFT)
                + (rawDepth.g * ARCORE_FLOAT_TO_6BITS * ARCORE_RGB565_GREEN_SHIFT)
                + (rawDepth.b * ARCORE_FLOAT_TO_5BITS);
    depth = min(depth, ARCORE_MAX_DEPTH_MM);
    depth *= ARCORE_DEPTH_SCALE;
    return depth;
}

inline float _ArCoreDepth_GetSampleAlpha(float2 uv, float3 virtualDepth)
{
    float realDepth = ArCoreDepth_GetMeters(uv);
    float signedDiffMeters = realDepth - virtualDepth;
    return saturate(signedDiffMeters / ARCORE_BLEND_FADE_RANGE);
}

inline float _ArCoreDepth_GetBlendedAlpha(float2 uv, float3 virtualDepth, float2 stride)
{
    // Hammersley low-discrepancy sampling, with triangular weighting.
    //
    // The 2D Hammersley point set of size N is usually defined as:
    //   H(i,N) = {i/N, phi2(i)}, with i=[0,N-1]
    // Where phi2 is the base-2 van der Corput sequence.
    //
    // See: https://en.wikipedia.org/wiki/Low-discrepancy_sequence
    // See: https://en.wikipedia.org/wiki/Van_der_Corput_sequence
    //
    // With N=2^n points in the 2D Hammersley set of size N lie on an NxN grid.
    // We choose N=8. To center these points around 0 we need to subctract 7/16.
    //
    // For the triangular weighting, we take weights from the matrix:
    //
    // +--                     --+
    // |  1  2  3  4  4  3  2  1 |
    // |  2  4  6  8  8  6  4  2 |
    // |  3  6  9 12 12  9  6  3 |
    // |  4  8 12 16 16 12  8  4 |
    // |  4  8 12 16 16 12  8  4 |
    // |  3  6  9 12 12  9  6  3 |
    // |  2  4  6  8  8  6  4  2 |
    // |  1  2  3  4  4  3  2  1 |
    // +--                     --+

    const float2 center_bias = float2(7.0/16.0, 7.0/16.0);

    float s;
    s  = _ArCoreDepth_GetSampleAlpha(uv + (float2(0.0/8.0, 0.0/1.0) - center_bias) *
                                     stride, virtualDepth) * 1.0/52.0;
    s += _ArCoreDepth_GetSampleAlpha(uv + (float2(1.0/8.0, 1.0/2.0) - center_bias) *
                                     stride, virtualDepth) * 8.0/52.0;
    s += _ArCoreDepth_GetSampleAlpha(uv + (float2(2.0/8.0, 1.0/4.0) - center_bias) *
                                     stride, virtualDepth) * 9.0/52.0;
    s += _ArCoreDepth_GetSampleAlpha(uv + (float2(3.0/8.0, 3.0/4.0) - center_bias) *
                                     stride, virtualDepth) * 8.0/52.0;
    s += _ArCoreDepth_GetSampleAlpha(uv + (float2(4.0/8.0, 1.0/8.0) - center_bias) *
                                     stride, virtualDepth) * 8.0/52.0;
    s += _ArCoreDepth_GetSampleAlpha(uv + (float2(5.0/8.0, 5.0/8.0) - center_bias) *
                                     stride, virtualDepth) * 9.0/52.0;
    s += _ArCoreDepth_GetSampleAlpha(uv + (float2(6.0/8.0, 3.0/8.0) - center_bias) *
                                     stride, virtualDepth) * 8.0/52.0;
    s += _ArCoreDepth_GetSampleAlpha(uv + (float2(7.0/8.0, 7.0/8.0) - center_bias) *
                                     stride, virtualDepth) * 1.0/52.0;

    return s;
}

// Returns an alpha to apply to occluded geometry that blends depth samples from nearby texels.
// Uses _OcclusionOffsetMeters, _CurrentDepthTexture_TexelSize, _OcclusionBlendingScale and
// _CurrentDepthTexture.
inline float ArCoreDepth_GetVisibility(float2 uv, float3 viewPos)
{
    float virtualDepth = -viewPos.z - _OcclusionOffsetMeters;
    float2 stride = _CurrentDepthTexture_TexelSize * _OcclusionBlendingScale;
    return _ArCoreDepth_GetBlendedAlpha(uv, virtualDepth, stride);
}

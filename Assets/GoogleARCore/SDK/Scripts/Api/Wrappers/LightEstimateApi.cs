//-----------------------------------------------------------------------
// <copyright file="LightEstimateApi.cs" company="Google">
//
// Copyright 2017 Google LLC. All Rights Reserved.
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

namespace GoogleARCoreInternal
{
    using System;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.Rendering;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    internal class LightEstimateApi
    {
        private readonly float[] k_SHConstants =
        {
            0.886227f, 1.023328f, 1.023328f,
            1.023328f, 0.858086f, 0.858086f,
            0.247708f, 0.858086f, 0.429043f
        };

        private NativeSession m_NativeSession;

#if !UNITY_2017_2_OR_NEWER
        private Color[] m_TempCubemapFacePixels = new Color[0];
#endif
        private float[] m_TempVector = new float[3];
        private float[] m_TempColor = new float[3];
        private float[] m_TempSHCoefficients = new float[27];
        private Cubemap m_HDRCubemap = null;
        private long m_CubemapTimestamp = -1;
        private int m_CubemapTextureId = 0;
        private bool m_PluginInitialized = false;

        public LightEstimateApi(NativeSession nativeSession)
        {
            m_NativeSession = nativeSession;
        }

        public IntPtr Create()
        {
            IntPtr lightEstimateHandle = IntPtr.Zero;
            ExternApi.ArLightEstimate_create(
                m_NativeSession.SessionHandle, ref lightEstimateHandle);
            return lightEstimateHandle;
        }

        public void Destroy(IntPtr lightEstimateHandle)
        {
            ExternApi.ArLightEstimate_destroy(lightEstimateHandle);
        }

        public LightEstimateState GetState(IntPtr lightEstimateHandle)
        {
            ApiLightEstimateState state = ApiLightEstimateState.NotValid;
            ExternApi.ArLightEstimate_getState(
                m_NativeSession.SessionHandle, lightEstimateHandle, ref state);
            return state.ToLightEstimateState();
        }

        public float GetPixelIntensity(IntPtr lightEstimateHandle)
        {
            float pixelIntensity = 0;
            ExternApi.ArLightEstimate_getPixelIntensity(
                m_NativeSession.SessionHandle, lightEstimateHandle, ref pixelIntensity);
            return pixelIntensity;
        }

        public Color GetColorCorrection(IntPtr lightEstimateHandle)
        {
            Color colorCorrection = Color.black;
            ExternApi.ArLightEstimate_getColorCorrection(
                m_NativeSession.SessionHandle, lightEstimateHandle, ref colorCorrection);
            return colorCorrection;
        }

        public void GetMainDirectionalLight(IntPtr sessionHandle, IntPtr lightEstimateHandle,
            out Quaternion lightRotation, out Color lightColor)
        {
            lightColor = Color.black;

            ExternApi.ArLightEstimate_getEnvironmentalHdrMainLightIntensity(sessionHandle,
                lightEstimateHandle, m_TempColor);
            lightColor.r = m_TempColor[0];
            lightColor.g = m_TempColor[1];
            lightColor.b = m_TempColor[2];

            // Apply the energy conservation term to the light color directly since Unity doesn't
            // have that term in their PBR shader.
            lightColor = lightColor / Mathf.PI;

            ExternApi.ArLightEstimate_getEnvironmentalHdrMainLightDirection(sessionHandle,
                lightEstimateHandle, m_TempVector);
            Vector3 lightDirection = Vector3.one;
            ConversionHelper.ApiVectorToUnityVector(m_TempVector, out lightDirection);

            // The ARCore output the light direction defined for shader usage: lightPos-pixelPos
            // We need to invert the direction to set it Unity world space.
            lightRotation = Quaternion.LookRotation(-lightDirection);
        }

        public void GetAmbientSH(IntPtr sessionHandle, IntPtr lightEstimateHandle,
            float[,] outSHCoefficients)
        {
            ExternApi.ArLightEstimate_getEnvironmentalHdrAmbientSphericalHarmonics(sessionHandle,
                lightEstimateHandle, m_TempSHCoefficients);

            // We need to invert the coefficients that contains the z axis to map it to
            // Unity world coordinate.
            // See: https://en.wikipedia.org/wiki/Table_of_spherical_harmonics
            // We also premultiply the constant that Unity applies to the SH to avoid
            // calculation in the shader.
            // Unity uses the following equation to calculate SH color in their shaders where the
            // constants that are used to calculate the SH color are baked in the SH coefficients
            // before passing to the shader.
            //   Say normal direction is (x, y, z)
            //   The color from SH given the normal direction is:
            //   color = SH[0] - SH[6] + SH[3]x + SH[1]y + SH[2]z                      (L0 + L1)
            //           + SH[4]xy + SH[5]yz + 3*SH[6]zz + SH[7]xz + SH[8](xx - yy)    (L2 + L3)
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    outSHCoefficients[j, i] = m_TempSHCoefficients[(j * 3) + i];
                    if (j == 2 || j == 5 || j == 7)
                    {
                        outSHCoefficients[j, i] = outSHCoefficients[j, i] * -1.0f;
                    }

                    outSHCoefficients[j, i] = outSHCoefficients[j, i] * k_SHConstants[j];

                    // Apply the energy conservation to SH coefficients as well.
                    outSHCoefficients[j, i] = outSHCoefficients[j, i] / Mathf.PI;
                }
            }
        }

        public Cubemap GetReflectionCubemap(IntPtr sessionHandle, IntPtr lightEstimateHandle)
        {
            int size = 0;
            bool usingGammaWorkflow = QualitySettings.activeColorSpace == ColorSpace.Gamma;

#if UNITY_2017_2_OR_NEWER
            // Cubemap.CreateExternalTexture only exists in Unity 2017 above.
            int textureId = 0;
            ApiTextureDataType dataType =
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 ?
                ApiTextureDataType.Half : ApiTextureDataType.Byte;
            TextureFormat format = dataType == ApiTextureDataType.Half ?
                TextureFormat.RGBAHalf : TextureFormat.RGBA32;

            if (!m_PluginInitialized)
            {
                ExternApi.ARCoreRenderingUtils_SetTextureDataType(dataType, true);
                ExternApi.ARCoreRenderingUtils_SetActiveColorSpace(usingGammaWorkflow);
                m_PluginInitialized = true;
            }

            ExternApi.ARCoreRenderingUtils_GetCubemapTexture(ref textureId, ref size);
            if (textureId != 0 && (m_HDRCubemap == null || textureId != m_CubemapTextureId))
            {
                m_HDRCubemap = Cubemap.CreateExternalTexture(size, format, true,
                    new IntPtr(textureId));
                m_CubemapTextureId = textureId;
            }

            long timestamp = GetTimestamp(sessionHandle, lightEstimateHandle);
            if (m_CubemapTimestamp != timestamp)
            {
                ExternApi.ARCoreRenderingUtils_SetARCoreLightEstimation(sessionHandle,
                    lightEstimateHandle);
                m_CubemapTimestamp = timestamp;
            }

            // Issue plugin event to update cubemap texture.
            GL.IssuePluginEvent(ExternApi.ARCoreRenderingUtils_GetRenderEventFunc(), 1);

#else
            // Gets raw color data from native plugin then update cubemap textures by
            // Cubemap.SetPixel().
            // Note, no GL texture will be created in this scenario.
            if (!m_PluginInitialized)
            {
                ExternApi.ARCoreRenderingUtils_SetTextureDataType(
                    ApiTextureDataType.Float, false);
                ExternApi.ARCoreRenderingUtils_SetActiveColorSpace(usingGammaWorkflow);
                m_PluginInitialized = true;
            }

            ExternApi.ARCoreRenderingUtils_GetCubemapTexture(ref m_CubemapTextureId, ref size);
            if (size > 0)
            {
                if (m_HDRCubemap == null)
                {
                    m_HDRCubemap = new Cubemap(size, TextureFormat.RGBAHalf, true);
                }

                if (m_TempCubemapFacePixels.Length != size)
                {
                    Array.Resize(ref m_TempCubemapFacePixels, size * size);
                }
            }

            long timestamp = GetTimestamp(sessionHandle, lightEstimateHandle);
            if (m_CubemapTimestamp != timestamp)
            {
                ExternApi.ARCoreRenderingUtils_SetARCoreLightEstimation(sessionHandle,
                    lightEstimateHandle);
                m_CubemapTimestamp = timestamp;

                if (m_HDRCubemap != null)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        ExternApi.ARCoreRenderingUtils_GetCubemapRawColors(i,
                            m_TempCubemapFacePixels);
                        m_HDRCubemap.SetPixels(m_TempCubemapFacePixels, CubemapFace.PositiveX + i);
                    }

                    // This operation is very expensive, only update cubemap texture when
                    // the light estimate is updated in this frame.
                    m_HDRCubemap.Apply();
                }
            }
#endif

            return m_HDRCubemap;
        }

        public long GetTimestamp(IntPtr sessionHandle, IntPtr lightEstimateHandle)
        {
            long timestamp = -1;
            ExternApi.ArLightEstimate_getTimestamp(
                sessionHandle, lightEstimateHandle, ref timestamp);
            return timestamp;
        }

        private struct ExternApi
        {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_create(
                IntPtr sessionHandle, ref IntPtr lightEstimateHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_destroy(IntPtr lightEstimateHandle);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_getState(
                IntPtr sessionHandle, IntPtr lightEstimateHandle, ref ApiLightEstimateState state);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_getPixelIntensity(
                IntPtr sessionHandle, IntPtr lightEstimateHandle, ref float pixelIntensity);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_getColorCorrection(
                IntPtr sessionHandle, IntPtr lightEstimateHandle, ref Color colorCorrection);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_getEnvironmentalHdrMainLightDirection(
                IntPtr sessionHandle, IntPtr lightEstimateHandle,
                float[] direction);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_getEnvironmentalHdrMainLightIntensity(
                IntPtr sessionHandle, IntPtr lightEstimateHandle,
                float[] color);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_getEnvironmentalHdrAmbientSphericalHarmonics(
                IntPtr session, IntPtr light_estimate, float[] out_coefficients_27);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_acquireEnvironmentalHdrCubemap(IntPtr session,
                IntPtr light_estimate, ref IntPtr out_textures_6);

            [AndroidImport(ApiConstants.ARCoreNativeApi)]
            public static extern void ArLightEstimate_getTimestamp(IntPtr session,
                IntPtr light_estimate, ref long timestamp);

            [AndroidImport("arcore_rendering_utils_api")]
            public static extern void ARCoreRenderingUtils_SetTextureDataType(
                ApiTextureDataType texture_data_type, bool create_gl_texture);

            [AndroidImport("arcore_rendering_utils_api")]
            public static extern void ARCoreRenderingUtils_SetActiveColorSpace(bool is_gamma_space);

            [AndroidImport("arcore_rendering_utils_api")]
            public static extern void ARCoreRenderingUtils_SetARCoreLightEstimation(
                IntPtr session, IntPtr cubemap_image);

            [AndroidImport("arcore_rendering_utils_api")]
            public static extern void ARCoreRenderingUtils_GetCubemapTexture(ref int out_texture_id,
                ref int out_width_height);

            [AndroidImport("arcore_rendering_utils_api")]
            public static extern void ARCoreRenderingUtils_GetCubemapRawColors(
                int face_index, Color[] out_pixel_colors);

            [AndroidImport("arcore_rendering_utils_api")]
            public static extern IntPtr ARCoreRenderingUtils_GetRenderEventFunc();
#pragma warning restore 626
        }
    }
}

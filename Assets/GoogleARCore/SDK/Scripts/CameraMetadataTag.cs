//-----------------------------------------------------------------------
// <copyright file="CameraMetadataTag.cs" company="Google LLC">
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

namespace GoogleARCore
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This enum follows the layout of NdkCameraMetadataTags.
    /// The values in the file are used for requesting / marshaling camera image's metadata.
    /// The comments have been removed to keep the code readable. Please refer to
    /// NdkCameraMetadataTags.h for documentation:
    /// https://developer.android.com/ndk/reference/ndk_camera_metadata_tags_8h.html .
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                     "SA1602:EnumerationItemsMustBeDocumented",
     Justification = "NdkCameraMetadataTags.")]
    public enum CameraMetadataTag
    {
        SectionColorCorrection = 0,
        SectionControl = 1,
        SectionEdge = 3,
        SectionFlash = 4,
        SectionFlashInfo = 5,
        SectionHotPixel = 6,
        SectionJpeg = 7,
        SectionLens = 8,
        SectionLensInfo = 9,
        SectionNoiseReduction = 10,
        SectionRequest = 12,
        SectionScaler = 13,
        SectionSensor = 14,
        SectionSensorInfo = 15,
        SectionShading = 16,
        SectionStatistics = 17,
        SectionStatisticsInfo = 18,
        SectionTonemap = 19,
        SectionInfo = 21,
        SectionBlackLevel = 22,
        SectionSync = 23,
        SectionDepth = 25,

        // Start Value Of Each Section.
        ColorCorrectionStart = SectionColorCorrection << 16,
        ControlStart = SectionControl << 16,
        EdgeStart = SectionEdge << 16,
        FlashStart = SectionFlash << 16,
        FlashInfoStart = SectionFlashInfo << 16,
        HotPixelStart = SectionHotPixel << 16,
        JpegStart = SectionJpeg << 16,
        LensStart = SectionLens << 16,
        LensInfoStart = SectionLensInfo << 16,
        NoiseReductionStart = SectionNoiseReduction << 16,
        RequestStart = SectionRequest << 16,
        ScalerStart = SectionScaler << 16,
        SensorStart = SectionSensor << 16,
        SensorInfoStart = SectionSensorInfo << 16,
        ShadingStart = SectionShading << 16,
        StatisticsStart = SectionStatistics << 16,
        StatisticsInfoStart = SectionStatisticsInfo << 16,
        TonemapStart = SectionTonemap << 16,
        InfoStart = SectionInfo << 16,
        BlackLevelStart = SectionBlackLevel << 16,
        SyncStart = SectionSync << 16,
        DepthStart = SectionDepth << 16,

        // Note that we only expose the keys that could be used in the camera metadata from the
        // capture result. The keys may only appear in CameraCharacteristics are not exposed here.
        ColorCorrectionMode = // Byte (Enum)
                ColorCorrectionStart,

        ColorCorrectionTransform = // Rational[33]
                ColorCorrectionStart + 1,

        ColorCorrectionGains = // Float[4]
                ColorCorrectionStart + 2,

        ColorCorrectionAberrationMode = // Byte (Enum)
                ColorCorrectionStart + 3,

        ControlAeAntibandingMode = // Byte (Enum)
                ControlStart,

        ControlAeExposureCompensation = // Int32
                ControlStart + 1,

        ControlAeLock = // Byte (Enum)
                ControlStart + 2,

        ControlAeMode = // Byte (Enum)
                ControlStart + 3,

        ControlAeRegions = // Int32[5areaCount]
                ControlStart + 4,

        ControlAeTargetFpsRange = // Int32[2]
                ControlStart + 5,

        ControlAePrecaptureTrigger = // Byte (Enum)
                ControlStart + 6,

        ControlAfMode = // Byte (Enum)
                ControlStart + 7,

        ControlAfRegions = // Int32[5areaCount]
                ControlStart + 8,

        ControlAfTrigger = // Byte (Enum)
                ControlStart + 9,

        ControlAwbLock = // Byte (Enum)
                ControlStart + 10,

        ControlAwbMode = // Byte (Enum)
                ControlStart + 11,

        ControlAwbRegions = // Int32[5areaCount]
                ControlStart + 12,

        ControlCaptureIntent = // Byte (Enum)
                ControlStart + 13,

        ControlEffectMode = // Byte (Enum)
                ControlStart + 14,

        ControlMode = // Byte (Enum)
                ControlStart + 15,

        ControlSceneMode = // Byte (Enum)
                ControlStart + 16,

        ControlVideoStabilizationMode = // Byte (Enum)
                ControlStart + 17,

        ControlAeState = // Byte (Enum)
                ControlStart + 31,

        ControlAfState = // Byte (Enum)
                ControlStart + 32,

        ControlAwbState = // Byte (Enum)
                ControlStart + 34,

        ControlPostRawSensitivityBoost = // Int32
                ControlStart + 40,

        EdgeMode = // Byte (Enum)
                EdgeStart,

        FlashMode = // Byte (Enum)
                FlashStart + 2,

        FlashState = // Byte (Enum)
                FlashStart + 5,

        HotPixelMode = // Byte (Enum)
                HotPixelStart,

        JpegGpsCoordinates = // Double[3]
                JpegStart,

        JpegGpsProcessingMethod = // Byte
                JpegStart + 1,

        JpegGpsTimestamp = // Int64
                JpegStart + 2,

        JpegOrientation = // Int32
                JpegStart + 3,

        JpegQuality = // Byte
                JpegStart + 4,

        JpegThumbnailQuality = // Byte
                JpegStart + 5,

        JpegThumbnailSize = // Int32[2]
                JpegStart + 6,

        LensAperture = // Float
                LensStart,

        LensFilterDensity = // Float
                LensStart + 1,

        LensFocalLength = // Float
                LensStart + 2,

        LensFocusDistance = // Float
                LensStart + 3,

        LensOpticalStabilizationMode = // Byte (Enum)
                LensStart + 4,

        LensPoseRotation = // Float[4]
                LensStart + 6,

        LensPoseTranslation = // Float[3]
                LensStart + 7,

        LensFocusRange = // Float[2]
                LensStart + 8,

        LensState = // Byte (Enum)
                LensStart + 9,

        LensIntrinsicCalibration = // Float[5]
                LensStart + 10,

        LensRadialDistortion = // Float[6]
                LensStart + 11,

        NoiseReductionMode = // Byte (Enum)
                NoiseReductionStart,

        RequestPipelineDepth = // Byte
                RequestStart + 9,

        ScalerCropRegion = // Int32[4]
                ScalerStart,

        SensorExposureTime = // Int64
                SensorStart,

        SensorFrameDuration = // Int64
                SensorStart + 1,

        SensorSensitivity = // Int32
                SensorStart + 2,

        SensorTimestamp = // Int64
                SensorStart + 16,

        SensorNeutralColorPoint = // Rational[3]
                SensorStart + 18,

        SensorNoiseProfile = // Double[2Cfa Channels]
                SensorStart + 19,

        SensorGreenSplit = // Float
                SensorStart + 22,

        SensorTestPatternData = // Int32[4]
                SensorStart + 23,

        SensorTestPatternMode = // Int32 (Enum)
                SensorStart + 24,

        SensorRollingShutterSkew = // Int64
                SensorStart + 26,

        SensorDynamicBlackLevel = // Float[4]
                SensorStart + 28,

        SensorDynamicWhiteLevel = // Int32
                SensorStart + 29,

        ShadingMode = // Byte (Enum)
                ShadingStart,

        StatisticsFaceDetectMode = // Byte (Enum)
                StatisticsStart,

        StatisticsHotPixelMapMode = // Byte (Enum)
                StatisticsStart + 3,

        StatisticsFaceIds = // Int32[N]
                StatisticsStart + 4,

        StatisticsFaceLandmarks = // Int32[N6]
                StatisticsStart + 5,

        StatisticsFaceRectangles = // Int32[N4]
                StatisticsStart + 6,

        StatisticsFaceScores = // Byte[N]
                StatisticsStart + 7,

        StatisticsLensShadingMap = // Float[4nm]
                StatisticsStart + 11,

        StatisticsSceneFlicker = // Byte (Enum)
                StatisticsStart + 14,

        StatisticsHotPixelMap = // Int32[2n]
                StatisticsStart + 15,

        StatisticsLensShadingMapMode = // Byte (Enum)
                StatisticsStart + 16,

        TonemapCurveBlue = // Float[N2]
                TonemapStart,

        TonemapCurveGreen = // Float[N2]
                TonemapStart + 1,

        TonemapCurveRed = // Float[N2]
                TonemapStart + 2,

        TonemapMode = // Byte (Enum)
                TonemapStart + 3,

        TonemapGamma = // Float
                TonemapStart + 6,

        TonemapPresetCurve = // Byte (Enum)
                TonemapStart + 7,

        BlackLevelLock = // Byte (Enum)
                BlackLevelStart,

        SyncFrameNumber = // Int64 (Enum)
                SyncStart,
    }
}

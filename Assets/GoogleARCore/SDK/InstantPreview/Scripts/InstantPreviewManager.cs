//-----------------------------------------------------------------------
// <copyright file="InstantPreviewManager.cs" company="Google LLC">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.SpatialTracking;

#if UNITY_EDITOR
    using UnityEditor;
#endif

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
    using IOSImport = GoogleARCoreInternal.DllImportNoop;
#endif

    /// <summary>
    /// Contains methods for managing communication to the Instant Preview
    /// plugin.
    /// </summary>
    public static class InstantPreviewManager
    {
        /// <summary>
        /// Name of the Instant Preview plugin library.
        /// </summary>
        public const string InstantPreviewNativeApi = "arcore_instant_preview_unity_plugin";

        // Guid is taken from meta file and should never change.
        private const string _apkGuid = "cf7b10762fe921e40a18151a6c92a8a6";
        private const string _noDevicesFoundAdbResult = "error: no devices/emulators found";
        private const float _maxTolerableAspectRatioDifference = 0.1f;
        private const string _mismatchedAspectRatioWarningFormatString =
            "Instant Preview camera texture aspect ratio ({0}) is different than Game view " +
            "aspect ratio ({1}).\n" +
            " To avoid distorted preview while using Instant Preview, set the Game view Aspect " +
            "to match the camera texture resolution ({2}x{3}).";

        private const string _instantPreviewInputWarning =
            "Touch ignored. Make sure your script contains `using Input = InstantPreviewInput;` " +
            "when using editor Play mode.\nTo learn more, see " +
            "https://developers.google.com/ar/develop/unity/instant-preview";

        private const string _warningToastFormat = "Instant Preview is not able to {0}. See " +
          "Unity console.";

        private const int _warningThrottleTimeSeconds = 5;

        private const float _unknownGameViewScale = (float)Single.MinValue;

        private static readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        private static Dictionary<string, DateTime> _sentWarnings =
            new Dictionary<string, DateTime>();

        private static HashSet<string> _oneTimeWarnings = new HashSet<string>();

        /// <summary>
        /// Gets a value indicating whether Instant Preview is providing the ARCore platform for the
        /// current environment.
        /// </summary>
        /// <value>Whether Instant Preview is providing the ARCore platform for the current
        /// environment.</value>
        public static bool IsProvidingPlatform
        {
            get
            {
#if UNITY_EDITOR
                return ARCoreProjectSettings.Instance.IsInstantPreviewEnabled;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Validates whether the SessionConfig enables limited supported features and also logs
        /// those features.
        /// </summary>
        /// <param name="config">The SessionConfig needs to be validated.</param>
        /// <returns><c>true</c>, if the SessionConfig isn't enable any limited support features.
        /// Otherwise, returns <c>false</c>.</returns>
        public static bool ValidateSessionConfig(ARCoreSessionConfig config)
        {
            bool isValid = true;
            if (config == null)
            {
                Debug.LogWarning("Attempted to check empty configuration.");
                return false;
            }

            if (config.LightEstimationMode != LightEstimationMode.Disabled)
            {
                LogLimitedSupportMessage("enable 'Light Estimation'", true);
                isValid = false;
            }

            if (config.AugmentedImageDatabase != null)
            {
                LogLimitedSupportMessage("enable 'Augmented Images'", true);
                isValid = false;
            }

            if (config.AugmentedFaceMode == AugmentedFaceMode.Mesh)
            {
                LogLimitedSupportMessage("enable 'Augmented Faces'", true);
                isValid = false;
            }

            if (config.InstantPlacementMode != InstantPlacementMode.Disabled)
            {
                LogLimitedSupportMessage("enable 'Instant Placement'", true);
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Generates a config supported by Instant Preview based on the given SessionConfig object.
        /// </summary>
        /// <param name="config">The base SessionConfig object.</param>
        /// <returns>A SessionConfig object copied from given SessionConfig object but
        /// all limited supported features are disabled.</returns>
        public static ARCoreSessionConfig GenerateInstantPreviewSupportedConfig(
            ARCoreSessionConfig config)
        {
            ARCoreSessionConfig newConfig = ScriptableObject.CreateInstance<ARCoreSessionConfig>();

            if (config == null)
            {
                Debug.LogWarning("Attempted to generate Instant Preview Supported Config from" +
                    "an empty SessionConfig object.");
            }
            else
            {
                newConfig.CopyFrom(config);
            }

            newConfig.LightEstimationMode = LightEstimationMode.Disabled;
            newConfig.AugmentedImageDatabase = null;
            newConfig.AugmentedFaceMode = AugmentedFaceMode.Disabled;
            newConfig.InstantPlacementMode = InstantPlacementMode.Disabled;
            return newConfig;
        }

        /// <summary>
        /// Logs a limited support message to the console for an instant preview feature.
        /// </summary>
        /// <param name="featureName">The name of the feature that has limited support.</param>
        /// <param name="logOnce">True, only logs the warning once.
        /// Otherwise, repeats logging after waring throttle time.</param>
        public static void LogLimitedSupportMessage(string featureName, bool logOnce = false)
        {
            Debug.LogErrorFormat(
                "Attempted to {0} which is not yet supported by Instant Preview.\n" +
                "Please build and run on device to use this feature.", featureName);

            if (logOnce && !_oneTimeWarnings.Contains(featureName))
            {
                string warning = string.Format(_warningToastFormat, featureName);
                NativeApi.SendToast(warning);
                _oneTimeWarnings.Add(featureName);
            }

            if (!logOnce && (!_sentWarnings.ContainsKey(featureName) ||
                (DateTime.UtcNow - _sentWarnings[featureName]).TotalSeconds >=
                _warningThrottleTimeSeconds))
            {
                string warning = string.Format(_warningToastFormat, featureName);
                NativeApi.SendToast(warning);
                _sentWarnings[featureName] = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Coroutine method that communicates to the Instant Preview plugin
        /// every frame.
        ///
        /// If Instant Preview is not the providing platform, this does nothing.
        /// </summary>
        /// <returns>Enumerator for a coroutine that updates Instant Preview
        /// every frame.</returns>
        public static IEnumerator InitializeIfNeeded()
        {
            // Terminates if instant preview is not providing the platform.
            if (!IsProvidingPlatform)
            {
                yield break;
            }

            // User may have explicitly disabled Instant Preview.
            if (ARCoreProjectSettings.Instance != null &&
                !ARCoreProjectSettings.Instance.IsInstantPreviewEnabled)
            {
                yield break;
            }

#if UNITY_EDITOR
            // When build platform is not Android, verify min game view scale is 1.0x to prevent
            // confusing 2x scaling when Unity editor is running on a high density display.
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                float minGameViewScale = GetMinGameViewScaleOrUnknown();
                if (minGameViewScale != 1.0)
                {
                    String viewScaleText = minGameViewScale == _unknownGameViewScale ?
                        "<unknown>" : string.Format("{0}x", minGameViewScale);
                    Debug.LogWarningFormat(
                        "Instant Preview disabled, {0} minimum Game view scale unsupported for " +
                        "target build platform '{1}'.\n" +
                        "To use Instant Preview, switch build platform to '{2}' from the 'Build " +
                        "settings' window.",
                        viewScaleText,
                        EditorUserBuildSettings.activeBuildTarget,
                        BuildTarget.Android);
                    yield break;
                }
            }

            // Determine if any augmented image databases need a rebuild.
            List<AugmentedImageDatabase> databases = new List<AugmentedImageDatabase>();
            bool shouldRebuild = false;

            var augmentedImageDatabaseGuids = AssetDatabase.FindAssets("t:AugmentedImageDatabase");
            foreach (var databaseGuid in augmentedImageDatabaseGuids)
            {
                var database = AssetDatabase.LoadAssetAtPath<AugmentedImageDatabase>(
                    AssetDatabase.GUIDToAssetPath(databaseGuid));
                databases.Add(database);

                shouldRebuild = shouldRebuild || database.IsBuildNeeded();
            }

            // If the preference is to ask the user to rebuild, ask now.
            if (shouldRebuild && PromptToRebuildAugmentedImagesDatabase())
            {
                foreach (var database in databases)
                {
                    string error;
                    database.BuildIfNeeded(out error);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogWarning("Failed to rebuild augmented image database: " + error);
                    }
                }
            }
#endif

            var adbPath = ShellHelper.GetAdbPath();
            if (adbPath == null)
            {
                Debug.LogError("Instant Preview requires your Unity Android SDK path to be set. " +
                    "Please set it under 'Preferences > External Tools > Android'. " +
                    "You may need to install the Android SDK first.");
                yield break;
            }
            else if (!File.Exists(adbPath))
            {
                Debug.LogErrorFormat(
                    "adb not found at \"{0}\". Please verify that 'Preferences > External Tools " +
                    "> Android' has the correct Android SDK path that the Android Platform Tools " +
                    "are installed, and that \"{0}\" exists. You may need to install the Android " +
                    "SDK first.", adbPath);
                yield break;
            }

            if (Environment.GetEnvironmentVariable("ADB_TRACE") != null)
            {
                Debug.LogWarning("Instant Preview: ADB_TRACE was defined. Unsetting environment " +
                                 "variable for compatibility with Instant Preview.");
                Environment.SetEnvironmentVariable("ADB_TRACE", null);
            }

            string localVersion;
            if (!StartServer(adbPath, out localVersion))
            {
                yield break;
            }

            yield return InstallApkAndRunIfConnected(adbPath, localVersion);

            yield return UpdateLoop(adbPath);
        }

        /// <summary>
        /// Uploads the latest camera video frame received from Instant Preview
        /// to the specified texture. The texture might be recreated if it is
        /// not the right size or null.
        /// </summary>
        /// <param name="backgroundTexture">Texture variable to store the latest
        /// Instant Preview video frame.</param>
        /// <returns>True if InstantPreview updated the background texture,
        /// false if it did not and the texture still needs updating.</returns>
        public static bool UpdateBackgroundTextureIfNeeded(ref Texture2D backgroundTexture)
        {
            if (!IsProvidingPlatform)
            {
                return false;
            }

            IntPtr pixelBytes;
            int width;
            int height;
            if (NativeApi.LockCameraTexture(out pixelBytes, out width, out height))
            {
                if (backgroundTexture == null || width != backgroundTexture.width ||
                    height != backgroundTexture.height)
                {
                    backgroundTexture = new Texture2D(width, height, TextureFormat.BGRA32, false);
                }

                backgroundTexture.LoadRawTextureData(pixelBytes, width * height * 4);
                backgroundTexture.Apply();

                NativeApi.UnlockCameraTexture();
            }

            return true;
        }

        private static IEnumerator UpdateLoop(string adbPath)
        {
            var renderEventFunc = NativeApi.GetRenderEventFunc();
            var shouldConvertToBgra =
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11;
            var loggedAspectRatioWarning = false;

            // Waits until the end of the first frame until capturing the screen size,
            // because it might be incorrect when first querying it.
            yield return _waitForEndOfFrame;

            var currentWidth = 0;
            var currentHeight = 0;
            var needToStartActivity = true;
            var prevFrameLandscape = false;

            RenderTexture screenTexture = null;
            RenderTexture targetTexture = null;
            RenderTexture bgrTexture = null;

            // Begins update loop. The coroutine will cease when the
            // ARCoreSession component it's called from is destroyed.
            for (;;)
            {
                yield return _waitForEndOfFrame;

                var curFrameLandscape = Screen.width > Screen.height;
                if (prevFrameLandscape != curFrameLandscape)
                {
                    needToStartActivity = true;
                }

                prevFrameLandscape = curFrameLandscape;
                if (needToStartActivity)
                {
                    string activityName = curFrameLandscape ? "InstantPreviewLandscapeActivity" :
                        "InstantPreviewActivity";
                    string output;
                    string errors;
                    ShellHelper.RunCommand(adbPath,
                        "shell am start -S -n com.google.ar.core.instantpreview/." + activityName,
                        out output, out errors);
                    needToStartActivity = false;
                }

                // Creates a target texture to capture the preview window onto.
                // Some video encoders prefer the dimensions to be a multiple of 16.
                var targetWidth = RoundUpToNearestMultipleOf16(Screen.width);
                var targetHeight = RoundUpToNearestMultipleOf16(Screen.height);

                if (targetWidth != currentWidth || targetHeight != currentHeight)
                {
                    screenTexture = new RenderTexture(targetWidth, targetHeight, 0);
                    targetTexture = screenTexture;

                    if (shouldConvertToBgra)
                    {
                        bgrTexture = new RenderTexture(
                            screenTexture.width, screenTexture.height, 0,
                            RenderTextureFormat.BGRA32);
                        targetTexture = bgrTexture;
                    }

                    currentWidth = targetWidth;
                    currentHeight = targetHeight;
                }

                NativeApi.Update();
                InstantPreviewInput.Update();

                if (NativeApi.AppShowedTouchWarning())
                {
                    Debug.LogWarning(_instantPreviewInputWarning);
                    NativeApi.UnityLoggedTouchWarning();
                }

                AddInstantPreviewTrackedPoseDriverWhenNeeded();

                Graphics.Blit(null, screenTexture);

                if (shouldConvertToBgra)
                {
                    Graphics.Blit(screenTexture, bgrTexture);
                }

                var cameraTexture = Frame.CameraImage.Texture;
                if (!loggedAspectRatioWarning && cameraTexture != null)
                {
                    var sourceWidth = cameraTexture.width;
                    var sourceHeight = cameraTexture.height;
                    var sourceAspectRatio = (float)sourceWidth / sourceHeight;
                    var destinationWidth = Screen.width;
                    var destinationHeight = Screen.height;
                    var destinationAspectRatio = (float)destinationWidth / destinationHeight;

                    if (Mathf.Abs(sourceAspectRatio - destinationAspectRatio) >
                        _maxTolerableAspectRatioDifference)
                    {
                        Debug.LogWarningFormat(
                            _mismatchedAspectRatioWarningFormatString, sourceAspectRatio,
                            destinationAspectRatio, sourceWidth, sourceHeight);
                        loggedAspectRatioWarning = true;
                    }
                }

                NativeApi.SendFrame(targetTexture.GetNativeTexturePtr());
                GL.IssuePluginEvent(renderEventFunc, (int)ApiRenderEvent.UpdateCubemapTexture);
            }
        }

        private static void AddInstantPreviewTrackedPoseDriverWhenNeeded()
        {
            foreach (var poseDriver in Component.FindObjectsOfType<TrackedPoseDriver>())
            {
                poseDriver.enabled = false;
                var gameObject = poseDriver.gameObject;
                var hasInstantPreviewTrackedPoseDriver =
                    gameObject.GetComponent<InstantPreviewTrackedPoseDriver>() != null;
                if (!hasInstantPreviewTrackedPoseDriver)
                {
                    gameObject.AddComponent<InstantPreviewTrackedPoseDriver>();
                }
            }
        }

        /// <summary>
        /// Tries to install and run the Instant Preview android app.
        /// </summary>
        /// <param name="adbPath">Path to adb to use for installing.</param>
        /// <param name="localVersion">Local version of Instant Preview plugin to compare installed
        /// APK against.</param>
        /// <returns>Enumerator for coroutine that handles installation if necessary.</returns>
        private static IEnumerator InstallApkAndRunIfConnected(string adbPath, string localVersion)
        {
            string apkPath = null;

#if UNITY_EDITOR
            apkPath = UnityEditor.AssetDatabase.GUIDToAssetPath(_apkGuid);
#endif // !UNITY_EDITOR

            // Early outs if set to install but the apk can't be found.
            if (!File.Exists(apkPath))
            {
                Debug.LogErrorFormat(
                    "Trying to install Instant Preview APK but reference to InstantPreview.apk " +
                    "is broken. Couldn't find an asset with .meta file guid={0}.", _apkGuid);
                yield break;
            }

            Result result = new Result();

            Thread checkAdbThread = new Thread((object obj) =>
            {
                Result res = (Result)obj;
                string output;
                string errors;

                // Gets version of installed apk.
                ShellHelper.RunCommand(adbPath,
                    "shell dumpsys package com.google.ar.core.instantpreview | grep versionName",
                    out output, out errors);
                string installedVersion = null;
                if (!string.IsNullOrEmpty(output) && string.IsNullOrEmpty(errors))
                {
                    installedVersion = output.Substring(output.IndexOf('=') + 1);
                }

                // Early outs if no device is connected.
                if (string.Compare(errors, _noDevicesFoundAdbResult) == 0)
                {
                    return;
                }

                // Prints errors and exits on failure.
                if (!string.IsNullOrEmpty(errors))
                {
                    Debug.LogError(errors);
                    return;
                }

                if (installedVersion == null)
                {
                    Debug.LogFormat(
                        "Instant Preview app not installed on device.",
                        apkPath);
                }
                else if (installedVersion != localVersion)
                {
                    Debug.LogFormat(
                        "Instant Preview installed version \"{0}\" does not match local version " +
                        "\"{1}\".",
                        installedVersion, localVersion);
                }

                res.ShouldPromptForInstall = installedVersion != localVersion;
            });
            checkAdbThread.Start(result);

            while (!checkAdbThread.Join(0))
            {
                yield return 0;
            }

            if (result.ShouldPromptForInstall)
            {
                if (PromptToInstall())
                {
                    Thread installThread = new Thread(() =>
                    {
                        string output;
                        string errors;

                        Debug.LogFormat(
                            "Installing Instant Preview app version {0}.",
                            localVersion);

                        ShellHelper.RunCommand(adbPath,
                            "uninstall com.google.ar.core.instantpreview",
                            out output, out errors);

                        ShellHelper.RunCommand(adbPath,
                            string.Format("install \"{0}\"", apkPath),
                            out output, out errors);

                        // Prints any output from trying to install.
                        if (!string.IsNullOrEmpty(output))
                        {
                            Debug.LogFormat("Instant Preview installation:\n{0}", output);
                        }

                        if (!string.IsNullOrEmpty(errors) && errors != "Success")
                        {
                            Debug.LogErrorFormat(
                                "Failed to install Instant Preview app:\n{0}", errors);
                        }
                    });
                    installThread.Start();

                    while (!installThread.Join(0))
                    {
                        yield return 0;
                    }
                }
                else
                {
                    yield break;
                }
            }
        }

        private static bool PromptToInstall()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.DisplayDialog(
                "Instant Preview",
                "To instantly reflect your changes on device, the Instant Preview app will be " +
                "installed on your connected device.\n\nTo disable Instant Preview in this " +
                "project, uncheck 'Instant Preview Enabled' under 'Edit > Project Settings > " +
                "ARCore'.",
                "Okay",
                "Don't Install This Time");
#else
            return false;
#endif
        }

        private static bool PromptToRebuildAugmentedImagesDatabase()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.DisplayDialog(
                "Augmented Images", "The Augmented Images database is out of date, rebuild it now?",
                "Build", "Don't Build This Time");
#else
            return false;
#endif
        }

        private static bool StartServer(string adbPath, out string version)
        {
            // Tries to start server.
            const int _instantPreviewVersionStringMaxLength = 64;
            var versionStringBuilder = new StringBuilder(_instantPreviewVersionStringMaxLength);
            var started = NativeApi.InitializeInstantPreview(
                adbPath, versionStringBuilder, versionStringBuilder.Capacity);
            if (!started)
            {
                Debug.LogErrorFormat(
                    "Couldn't start Instant Preview server with adb path \"{0}\".", adbPath);
                version = null;
                return false;
            }

            version = versionStringBuilder.ToString();
            Debug.LogFormat(
                "Instant Preview version {0}\n" +
                "To disable Instant Preview in this project, uncheck 'Instant Preview Enabled' " +
                "under 'Edit > Project Settings > ARCore'.",
                version);
            return true;
        }

        private static int RoundUpToNearestMultipleOf16(int value)
        {
            return (value + 15) & ~15;
        }

        private static float GetMinGameViewScaleOrUnknown()
        {
            try
            {
                var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
                if (gameViewType == null)
                {
                    return _unknownGameViewScale;
                }

                UnityEngine.Object[] gameViewObjects =
                    UnityEngine.Resources.FindObjectsOfTypeAll(gameViewType);
                if (gameViewObjects == null || gameViewObjects.Length == 0)
                {
                    return _unknownGameViewScale;
                }

                PropertyInfo minScaleProperty =
                    gameViewType.GetProperty(
                        "minScale", BindingFlags.Instance | BindingFlags.NonPublic);
                if (minScaleProperty == null)
                {
                    return _unknownGameViewScale;
                }

                return (float)minScaleProperty.GetValue(gameViewObjects[0], null);
            }
            catch
            {
                return _unknownGameViewScale;
            }
        }

        private struct NativeApi
        {
#pragma warning disable 626
            [AndroidImport(InstantPreviewNativeApi)]
            public static extern bool InitializeInstantPreview(
                string adbPath, StringBuilder version, int versionStringLength);

            [AndroidImport(InstantPreviewNativeApi)]
            public static extern void Update();

            [AndroidImport(InstantPreviewNativeApi)]
            public static extern IntPtr GetRenderEventFunc();

            [AndroidImport(InstantPreviewNativeApi)]
            public static extern void SendFrame(IntPtr renderTexture);

            [AndroidImport(InstantPreviewNativeApi)]
            public static extern bool LockCameraTexture(out IntPtr pixelBytes, out int width,
                out int height);

            [AndroidImport(InstantPreviewNativeApi)]
            public static extern void UnlockCameraTexture();

            [AndroidImport(InstantPreviewNativeApi)]
            public static extern bool IsConnected();

            [AndroidImport(InstantPreviewNativeApi)]
            public static extern bool AppShowedTouchWarning();

            [AndroidImport(InstantPreviewNativeApi)]
            public static extern bool UnityLoggedTouchWarning();

            [AndroidImport(InstantPreviewNativeApi)]
            public static extern void SendToast(string toastMessage);
#pragma warning restore 626
        }

        private class Result
        {
            public bool ShouldPromptForInstall = false;
        }
    }
}

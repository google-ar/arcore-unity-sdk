//-----------------------------------------------------------------------
// <copyright file="InstantPreviewBugReport.cs" company="Google">
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

namespace GoogleARCoreInternal
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    internal static class InstantPreviewBugReport
    {
        private const string k_FileNamePrefix = "arcore_unity_editor_bug_report_";

        [MenuItem("Help/Capture ARCore Bug Report")]
        private static void CaptureBugReport()
        {
            string desktopPath = Environment.GetFolderPath(
                           Environment.SpecialFolder.Desktop);
            DateTime timeStamp = DateTime.Now;
            string fileNameTimestamp = timeStamp.ToString("yyyyMMdd_hhmmss");
            string filePath = Path.Combine(
                desktopPath, k_FileNamePrefix + fileNameTimestamp + ".txt");
            StreamWriter writer;

            // Operating system and hardware info have to be handled separately based on OS
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.MacOSX:
                    writer = File.CreateText(filePath);

                    writer.WriteLine("*** GOOGLE ARCORE SDK FOR UNITY OSX BUG REPORT ***");
                    writer.WriteLine("Timestamp: " + timeStamp.ToString());

                    writer.WriteLine();
                    writer.WriteLine("*** OPERATING SYSTEM INFORMATION ***");
                    WriteCommand("system_profiler", "SPSoftwareDataType", writer);

                    writer.WriteLine("*** GRAPHICS INFORMATION ***");
                    WriteCommand("system_profiler", "SPDisplaysDataType", writer);

                    WriteOsIndependentFields(writer);

                    string stdOut;
                    string stdErr;

                    // Get PATH directories to search for adb in.
                    ShellHelper.RunCommand(
                        "/bin/bash", "-c -l \"echo $PATH\"", out stdOut, out stdErr);
                    stdOut.Trim();

                    writer.WriteLine("*** ADB VERSIONS ON PATH ***");
                    WriteAdbPathVersions(stdOut.Split(':'), writer);

                    writer.WriteLine("*** TYPE -A ADB ***");
                    WriteCommand("/bin/bash", "-c -l \"type -a adb\"", writer);

                    writer.WriteLine("*** RUNNING ADB PROCESSES ***");
                    WriteCommand(
                        "/bin/bash", "-c -l \"ps -ef | grep -i adb | grep -v grep\"", writer);

                    writer.WriteLine("*** RUNNING UNITY PROCESSES ***");
                    WriteCommand(
                        "/bin/bash", "-c -l \"ps -ef | grep -i Unity | grep -v grep\"", writer);

                    writer.Close();

                    Debug.Log(
                        "ARCore bug report captured. File can be found here:\n" +
                        Path.GetFullPath(filePath));
                    break;

                case OperatingSystemFamily.Windows:
                    writer = File.CreateText(filePath);

                    writer.WriteLine("*** GOOGLE ARCORE SDK FOR UNITY WINDOWS BUG REPORT ***");
                    writer.WriteLine("Timestamp: " + timeStamp.ToString());

                    writer.WriteLine("*** OPERATING SYSTEM INFORMATION ***");
                    WriteCommand("cmd.exe", "/C systeminfo", writer);

                    writer.WriteLine("*** GRAPHICS INFORMATION ***");
                    WriteCommand(
                        "cmd.exe", "/C wmic path win32_VideoController get /format:list", writer);

                    WriteOsIndependentFields(writer);

                    string pathStr = Environment.GetEnvironmentVariable("PATH").Trim();

                    writer.WriteLine("*** ADB VERSIONS ON PATH ***");
                    WriteAdbPathVersions(pathStr.Split(';'), writer);

                    writer.WriteLine("*** RUNNING ADB PROCESSES ***");
                    WriteCommand("cmd.exe",
                        "/C TASKLIST | c:\\Windows\\System32\\findstr.exe \"adb\"", writer);

                    writer.WriteLine("*** RUNNING UNITY PROCESSES ***");
                    WriteCommand("cmd.exe",
                        "/C TASKLIST | c:\\Windows\\System32\\findstr.exe \"Unity\"", writer);

                    writer.Close();

                    Debug.Log(
                        "ARCore bug report captured. File can be found here:\n" +
                        Path.GetFullPath(filePath));
                    break;
                default:
                    string dialogMessage = "ARCore does not support capturing bug reports for " +
                        SystemInfo.operatingSystemFamily + " at this time.";

                    EditorUtility.DisplayDialog("ARCore Bug Report", dialogMessage, "OK");
                    break;
            }
        }

        // Writes the fields that don't have to be handled differently based on Operating System
        private static void WriteOsIndependentFields(StreamWriter writer)
        {
            writer.WriteLine("*** UNITY VERSION ***");
            writer.WriteLine(Application.unityVersion);
            writer.WriteLine();

            writer.WriteLine("*** UNITY RUNTIME PLATFORM ***");
            writer.WriteLine(Application.platform);
            writer.WriteLine();

            writer.WriteLine("*** ARCORE SDK FOR UNITY VERSION ***");
            writer.WriteLine(GoogleARCore.VersionInfo.Version);
            writer.WriteLine();

            // Can be null
            string adbPath = ShellHelper.GetAdbPath();

            writer.WriteLine("*** ARCORE APP VERSION ***");
            WritePackageVersionString(adbPath, "com.google.ar.core", writer);

            writer.WriteLine("*** INSTANT PREVIEW APP VERSION ***");
            WritePackageVersionString(adbPath, "com.google.ar.core.instantpreview", writer);

            StringBuilder instantPreviewPluginVer = new StringBuilder(64);

            // Get Instant preview version by running the server.
            NativeApi.InitializeInstantPreview(
                adbPath, instantPreviewPluginVer, instantPreviewPluginVer.Capacity);

            writer.WriteLine("*** INSTANT PREVIEW PLUGIN VERSION ***");
            writer.WriteLine(instantPreviewPluginVer);
            writer.WriteLine();

            writer.WriteLine("*** ADB DEVICES ***");
            WriteCommand(adbPath, "devices -l", writer);

            writer.WriteLine("*** DEVICE FINGERPRINT ***");
            WriteCommand(adbPath, "shell getprop ro.build.fingerprint", writer);

            writer.WriteLine("*** ADB VERSION USED BY UNITY ***");
            WriteCommand(adbPath, "version", writer);
        }

        private static void WriteAdbPathVersions(string[] pathDirs, StreamWriter writer)
        {
            // Search through directories in PATH to find the version of adb used from PATH
            foreach (var path in pathDirs)
            {
                // Ignore paths that contain illegal characters.
                if (path.IndexOfAny(Path.GetInvalidPathChars()) == -1)
                {
                    string fullAdbPath = Path.Combine(path, ShellHelper.GetAdbFileName());
                    if (File.Exists(fullAdbPath))
                    {
                        WriteCommand(fullAdbPath, "version", writer);
                    }
                }
            }
        }

        private static void WriteCommand(string program, string arguments, StreamWriter writer)
        {
            if (string.IsNullOrEmpty(program))
            {
                writer.WriteLine("error: program path was null");
            }
            else
            {
                string stdOut;
                string stdErr;

                ShellHelper.RunCommand(program, arguments, out stdOut, out stdErr);

                if (!string.IsNullOrEmpty(stdOut))
                {
                    writer.WriteLine(stdOut);
                }

                if (!string.IsNullOrEmpty(stdErr))
                {
                    writer.WriteLine(stdErr);
                }
            }

            writer.WriteLine();
        }

        private static void WritePackageVersionString(
            string adbPath, string package, StreamWriter writer)
        {
            if (string.IsNullOrEmpty(adbPath))
            {
                writer.WriteLine("error: adb path was null");
            }
            else
            {
                string stdOut;
                string stdErr;
                string arguments = "shell pm dump " + package + " | " +
                    "egrep -m 1 -i 'versionName' | sed -n 's/.*versionName=//p'";

                ShellHelper.RunCommand(adbPath, arguments, out stdOut, out stdErr);

                // If stdOut is populated, the device is connected and the app is installed
                if (!string.IsNullOrEmpty(stdOut))
                {
                    writer.WriteLine(stdOut);
                }
                else
                {
                    // If stdErr isn't empty, then either the device isn't connected or something
                    // else went wrong, such as adb not being installed.
                    if (!string.IsNullOrEmpty(stdErr))
                    {
                        writer.WriteLine(stdErr);
                    }
                    else
                    {
                        // If stdErr is empty, then the device is connected and the app isn't
                        // installed
                        writer.WriteLine(package + " is not installed on device");
                    }
                }
            }

            writer.WriteLine();
        }

        private struct NativeApi
        {
#pragma warning disable 626
            [DllImport(InstantPreviewManager.InstantPreviewNativeApi)]
            public static extern bool InitializeInstantPreview(
                string adbPath, StringBuilder version, int versionStringLength);
#pragma warning restore 626
        }
    }
}

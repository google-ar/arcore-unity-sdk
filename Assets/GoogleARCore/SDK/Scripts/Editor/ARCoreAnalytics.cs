//-----------------------------------------------------------------------
// <copyright file="ARCoreAnalytics.cs" company="Google LLC">
//
// Copyright 2019 Google LLC
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
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Google.Protobuf;
    using GoogleARCoreInternal.Proto;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Networking;

    [InitializeOnLoad]
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class ARCoreAnalytics
    {
        public bool EnableAnalytics;
        private const string _enableAnalyticsKey = "EnableGoogleARCoreAnalytics";
        private const string _googleAnalyticsHost = "https://play.googleapis.com/log";
        private const long _analyticsResendDelayTicks = TimeSpan.TicksPerDay * 7;
        private long _lastUpdateTicks;
        private bool _verbose;
#if UNITY_2017_1_OR_NEWER
        private UnityWebRequest _webRequest;
#endif

        /// <summary>
        /// Static constructor permits a once-on-load analytics collection event.
        /// </summary>
        static ARCoreAnalytics()
        {
            // Create the new instance.
            Instance = new ARCoreAnalytics();
            Instance.Load();

            // Send analytics immediately.
            Instance.SendAnalytics(_googleAnalyticsHost, LogRequestUtils.BuildLogRequest(), false);

            // Use the Editor Update callback to monitor the communication to the server.
            EditorApplication.update +=
                new EditorApplication.CallbackFunction(Instance.OnAnalyticsUpdate);
        }

        public static ARCoreAnalytics Instance { get; private set; }

        /// <summary>
        /// Loads analytics settings.
        /// </summary>
        public void Load()
        {
            EnableAnalytics = EditorPrefs.GetBool(_enableAnalyticsKey, true);
        }

        /// <summary>
        /// Saves current analytics preferences.
        /// </summary>
        public void Save()
        {
            EditorPrefs.SetBool(_enableAnalyticsKey, EnableAnalytics);
        }

        /// <summary>
        /// Generates an analytics package and sends it to the Clearcut servers.
        /// </summary>
        /// <param name="analyticsHost">Address of host to send the analytics to.</param>
        /// <param name="logRequest">Data to send to the analytics server.</param>
        /// <param name="verbose">If true, display debug messages in the console.</param>
        public void SendAnalytics(string analyticsHost, LogRequest logRequest, bool verbose)
        {
#if UNITY_2017_1_OR_NEWER
            // Save the time sending was last attempted.
            _lastUpdateTicks = DateTime.Now.Ticks;

            // Only send if analytics is enabled.
            if (EnableAnalytics == false)
            {
                if (verbose == true)
                {
                    Debug.Log("Google ARCore SDK for Unity analytics is disabled, not sending.");
                }

                return;
            }

            // Only allow one instance of the request at a time.
            if (_webRequest != null)
            {
                if (verbose == true)
                {
                    Debug.Log("Google ARCore SDK for Unity analytics is already sending data.");
                }

                return;
            }

            // Send the data to the server.
            UnityWebRequest webRequest = new UnityWebRequest(analyticsHost);
            webRequest.method = UnityWebRequest.kHttpVerbPOST;
            webRequest.uploadHandler = new UploadHandlerRaw(logRequest.ToByteArray());
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/x-protobuf");
            webRequest.SendWebRequest();

            // Set the verbosity preference for this request.
            _verbose = verbose;
            if (verbose == true)
            {
                Debug.Log("Sending Google ARCore SDK for Unity analytics.");
            }

            // The editor callback will follow through with this request.
            _webRequest = webRequest;
#endif
        }

        /// <summary>
        /// Periodically checks back to update the current logging request, or if
        /// enough time has passed, initiate a new logging request.
        /// </summary>
        private void OnAnalyticsUpdate()
        {
#if UNITY_2017_1_OR_NEWER
            // Nothing to do if Analytics isn't enabled.
            if (EnableAnalytics == false)
            {
                return;
            }

            // Process the current web request.
            if (_webRequest != null)
            {
                if (_webRequest.isDone == true)
                {
                    if (_verbose == true)
                    {
#if UNITY_2020_2_OR_NEWER
                        if (_webRequest.result == UnityWebRequest.Result.ConnectionError)
#else
                        if (_webRequest.isNetworkError == true)
#endif
                        {
                            Debug.Log("Error sending Google ARCore SDK for Unity analytics: " +
                                      _webRequest.error);
                        }
                        else
                        {
                            Debug.Log("Google ARCore SDK for Unity analytics sent: " +
                                      _webRequest.downloadHandler.text);
                        }
                    }

                    _webRequest = null;
                }
            }

            // Resend analytics periodically (once per week if the editor remains open.)
            if (DateTime.Now.Ticks - _lastUpdateTicks >= _analyticsResendDelayTicks)
            {
                Instance.SendAnalytics(
                    _googleAnalyticsHost, LogRequestUtils.BuildLogRequest(), false);
            }
#endif
        }
    }
}

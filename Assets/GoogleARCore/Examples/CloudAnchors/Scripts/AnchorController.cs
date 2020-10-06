//-----------------------------------------------------------------------
// <copyright file="AnchorController.cs" company="Google LLC">
//
// Copyright 2018 Google LLC
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

namespace GoogleARCore.Examples.CloudAnchors
{
    using GoogleARCore;
    using GoogleARCore.CrossPlatform;
    using UnityEngine;
    using UnityEngine.Networking;
#if ARCORE_IOS_SUPPORT
    using UnityEngine.XR.iOS;
#endif

    /// <summary>
    /// A Controller for the Anchor object that handles hosting and resolving the Cloud Anchor.
    /// </summary>
#pragma warning disable 618
    public class AnchorController : NetworkBehaviour
#pragma warning restore 618
    {
        /// <summary>
        /// The customized timeout duration for resolving request to prevent retrying to resolve
        /// indefinitely.
        /// </summary>
        private const float _resolvingTimeout = 10.0f;

        /// <summary>
        /// The Cloud Anchor ID that will be used to host and resolve the Cloud Anchor. This
        /// variable will be syncrhonized over all clients.
        /// </summary>
#pragma warning disable 618
        [SyncVar(hook = "OnChangeId")]
#pragma warning restore 618
        private string _cloudAnchorId = string.Empty;

        /// <summary>
        /// Indicates whether this script is running in the Host.
        /// </summary>
        private bool _isHost = false;

        /// <summary>
        /// Indicates whether an attempt to resolve the Cloud Anchor should be made.
        /// </summary>
        private bool _shouldResolve = false;

        /// <summary>
        /// Record the time since started resolving.
        /// If it passed the resolving timeout, additional instruction displays.
        /// </summary>
        private float _timeSinceStartResolving = 0.0f;

        /// <summary>
        /// Indicates whether passes the resolving timeout duration or the anchor has been
        /// successfully resolved.
        /// </summary>
        private bool _passedResolvingTimeout = false;

        /// <summary>
        /// The anchor mesh object.
        /// In order to avoid placing the Anchor on identity pose, the mesh object should
        /// be disabled by default and enabled after hosted or resolved.
        /// </summary>
        private GameObject _anchorMesh;

        /// <summary>
        /// The Cloud Anchors example controller.
        /// </summary>
        private CloudAnchorsExampleController _cloudAnchorsExampleController;

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _cloudAnchorsExampleController =
                GameObject.Find("CloudAnchorsExampleController")
                    .GetComponent<CloudAnchorsExampleController>();
            _anchorMesh = transform.Find("AnchorMesh").gameObject;
            _anchorMesh.SetActive(false);
        }

        /// <summary>
        /// The Unity OnStartClient() method.
        /// </summary>
        public override void OnStartClient()
        {
            if (_cloudAnchorId != string.Empty)
            {
                _shouldResolve = true;
            }
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (!_shouldResolve)
            {
                return;
            }

            if (!_cloudAnchorsExampleController.IsResolvingPrepareTimePassed())
            {
                return;
            }

            if (!_passedResolvingTimeout)
            {
                _timeSinceStartResolving += Time.deltaTime;

                if (_timeSinceStartResolving > _resolvingTimeout)
                {
                    _passedResolvingTimeout = true;
                    _cloudAnchorsExampleController.OnResolvingTimeoutPassed();
                }
            }

            ResolveAnchorFromId(_cloudAnchorId);
        }

        /// <summary>
        /// Command run on the server to set the Cloud Anchor Id.
        /// </summary>
        /// <param name="cloudAnchorId">The new Cloud Anchor Id.</param>
#pragma warning disable 618
        [Command]
#pragma warning restore 618
        public void CmdSetCloudAnchorId(string cloudAnchorId)
        {
            _cloudAnchorId = cloudAnchorId;
        }

        /// <summary>
        /// Gets the Cloud Anchor Id.
        /// </summary>
        /// <returns>The Cloud Anchor Id.</returns>
        public string GetCloudAnchorId()
        {
            return _cloudAnchorId;
        }

        /// <summary>
        /// Hosts the user placed cloud anchor and associates the resulting Id with this object.
        /// </summary>
        /// <param name="lastPlacedAnchor">The last placed anchor.</param>
        public void HostLastPlacedAnchor(Component lastPlacedAnchor)
        {
            _isHost = true;
            _anchorMesh.SetActive(true);

#if !UNITY_IOS
            var anchor = (Anchor)lastPlacedAnchor;
#elif ARCORE_IOS_SUPPORT
            var anchor = (UnityARUserAnchorComponent)lastPlacedAnchor;
#endif

#if !UNITY_IOS || ARCORE_IOS_SUPPORT
            XPSession.CreateCloudAnchor(anchor).ThenAction(result =>
            {
                if (result.Response != CloudServiceResponse.Success)
                {
                    Debug.Log(string.Format("Failed to host Cloud Anchor: {0}", result.Response));

                    _cloudAnchorsExampleController.OnAnchorHosted(
                        false, result.Response.ToString());
                    return;
                }

                Debug.Log(string.Format(
                    "Cloud Anchor {0} was created and saved.", result.Anchor.CloudId));
                CmdSetCloudAnchorId(result.Anchor.CloudId);

                _cloudAnchorsExampleController.OnAnchorHosted(true, result.Response.ToString());
            });
#endif
        }

        /// <summary>
        /// Resolves an anchor id and instantiates an Anchor prefab on it.
        /// </summary>
        /// <param name="cloudAnchorId">Cloud anchor id to be resolved.</param>
        private void ResolveAnchorFromId(string cloudAnchorId)
        {
            _cloudAnchorsExampleController.OnAnchorInstantiated(false);

            // If device is not tracking, let's wait to try to resolve the anchor.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            _shouldResolve = false;

            XPSession.ResolveCloudAnchor(cloudAnchorId).ThenAction(
                (System.Action<CloudAnchorResult>)(result =>
                    {
                        if (result.Response != CloudServiceResponse.Success)
                        {
                            Debug.LogError(string.Format(
                                "Client could not resolve Cloud Anchor {0}: {1}",
                                cloudAnchorId, result.Response));

                            _cloudAnchorsExampleController.OnAnchorResolved(
                                false, result.Response.ToString());
                            _shouldResolve = true;
                            return;
                        }

                        Debug.Log(string.Format(
                            "Client successfully resolved Cloud Anchor {0}.",
                            cloudAnchorId));

                        _cloudAnchorsExampleController.OnAnchorResolved(
                            true, result.Response.ToString());
                        OnResolved(result.Anchor.transform);
                        _anchorMesh.SetActive(true);
                    }));
        }

        /// <summary>
        /// Callback invoked once the Cloud Anchor is resolved.
        /// </summary>
        /// <param name="anchorTransform">Transform of the resolved Cloud Anchor.</param>
        private void OnResolved(Transform anchorTransform)
        {
            var cloudAnchorController = GameObject.Find("CloudAnchorsExampleController")
                                                  .GetComponent<CloudAnchorsExampleController>();
            cloudAnchorController.SetWorldOrigin(anchorTransform);

            // Mark resolving timeout passed so it won't fire OnResolvingTimeoutPassed event.
            _passedResolvingTimeout = true;
        }

        /// <summary>
        /// Callback invoked once the Cloud Anchor Id changes.
        /// </summary>
        /// <param name="newId">New identifier.</param>
        private void OnChangeId(string newId)
        {
            if (!_isHost && newId != string.Empty)
            {
                _cloudAnchorId = newId;
                _shouldResolve = true;
            }
        }
    }
}

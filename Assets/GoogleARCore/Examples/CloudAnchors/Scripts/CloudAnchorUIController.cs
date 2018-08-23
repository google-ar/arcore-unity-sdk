//-----------------------------------------------------------------------
// <copyright file="CloudAnchorUIController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
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
    using System.Net;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controller managing UI for the Cloud Anchors Example.
    /// </summary>
    public class CloudAnchorUIController : MonoBehaviour
    {
        /// <summary>
        /// A gameobject parenting UI for displaying feedback and errors.
        /// </summary>
        public Text SnackbarText;

        /// <summary>
        /// A text element displaying the current Room.
        /// </summary>
        public Text RoomText;

        /// <summary>
        /// A text element displaying the device's IP Address.
        /// </summary>
        public Text IPAddressText;

        /// <summary>
        /// The host anchor mode button.
        /// </summary>
        public Button HostAnchorModeButton;

        /// <summary>
        /// The resolve anchor mode button.
        /// </summary>
        public Button ResolveAnchorModeButton;

        /// <summary>
        /// The root for the input interface.
        /// </summary>
        public GameObject InputRoot;

        /// <summary>
        /// The input field for the room.
        /// </summary>
        public InputField RoomInputField;

        /// <summary>
        /// The input field for the ip address.
        /// </summary>
        public InputField IpAddressInputField;

        /// <summary>
        /// The field for toggling loopback (local) anchor resoltion.
        /// </summary>
        public Toggle ResolveOnDeviceToggle;

        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            IPAddressText.text = "My IP Address: " + _GetDeviceIpAddress();
        }

        /// <summary>
        /// Shows UI for application "Ready Mode".
        /// </summary>
        public void ShowReadyMode()
        {
            HostAnchorModeButton.GetComponentInChildren<Text>().text = "Host";
            HostAnchorModeButton.interactable = true;
            ResolveAnchorModeButton.GetComponentInChildren<Text>().text = "Resolve";
            ResolveAnchorModeButton.interactable = true;
            SnackbarText.text = "Please select Host or Resolve to continue";
            InputRoot.SetActive(false);
        }

        /// <summary>
        /// Shows UI for the beginning phase of application "Hosting Mode".
        /// </summary>
        /// <param name="snackbarText">Optional text to put in the snackbar.</param>
        public void ShowHostingModeBegin(string snackbarText = null)
        {
            HostAnchorModeButton.GetComponentInChildren<Text>().text = "Cancel";
            HostAnchorModeButton.interactable = true;
            ResolveAnchorModeButton.GetComponentInChildren<Text>().text = "Resolve";
            ResolveAnchorModeButton.interactable = false;

            if (string.IsNullOrEmpty(snackbarText))
            {
                SnackbarText.text =
                    "The room code is now available. Please place an anchor to host, press Cancel to Exit.";
            }
            else
            {
                SnackbarText.text = snackbarText;
            }

            InputRoot.SetActive(false);
        }

        /// <summary>
        /// Shows UI for the attempting to host phase of application "Hosting Mode".
        /// </summary>
        public void ShowHostingModeAttemptingHost()
        {
            HostAnchorModeButton.GetComponentInChildren<Text>().text = "Cancel";
            HostAnchorModeButton.interactable = false;
            ResolveAnchorModeButton.GetComponentInChildren<Text>().text = "Resolve";
            ResolveAnchorModeButton.interactable = false;
            SnackbarText.text = "Attempting to host anchor...";
            InputRoot.SetActive(false);
        }

        /// <summary>
        /// Shows UI for the beginning phase of application "Resolving Mode".
        /// </summary>
        /// <param name="snackbarText">Optional text to put in the snackbar.</param>
        public void ShowResolvingModeBegin(string snackbarText = null)
        {
            HostAnchorModeButton.GetComponentInChildren<Text>().text = "Host";
            HostAnchorModeButton.interactable = false;
            ResolveAnchorModeButton.GetComponentInChildren<Text>().text = "Cancel";
            ResolveAnchorModeButton.interactable = true;

            if (string.IsNullOrEmpty(snackbarText))
            {
                SnackbarText.text = "Input Room and IP address to resolve anchor.";
            }
            else
            {
                SnackbarText.text = snackbarText;
            }

            InputRoot.SetActive(true);
        }

        /// <summary>
        /// Shows UI for the attempting to resolve phase of application "Resolving Mode".
        /// </summary>
        public void ShowResolvingModeAttemptingResolve()
        {
            HostAnchorModeButton.GetComponentInChildren<Text>().text = "Host";
            HostAnchorModeButton.interactable = false;
            ResolveAnchorModeButton.GetComponentInChildren<Text>().text = "Cancel";
            ResolveAnchorModeButton.interactable = false;
            SnackbarText.text = "Attempting to resolve anchor.";
            InputRoot.SetActive(false);
        }

        /// <summary>
        /// Shows UI for the successful resolve phase of application "Resolving Mode".
        /// </summary>
        public void ShowResolvingModeSuccess()
        {
            HostAnchorModeButton.GetComponentInChildren<Text>().text = "Host";
            HostAnchorModeButton.interactable = false;
            ResolveAnchorModeButton.GetComponentInChildren<Text>().text = "Cancel";
            ResolveAnchorModeButton.interactable = true;
            SnackbarText.text = "The anchor was successfully resolved.";
            InputRoot.SetActive(false);
        }

        /// <summary>
        /// Sets the room number in the UI.
        /// </summary>
        /// <param name="roomNumber">The room number to set.</param>
        public void SetRoomTextValue(int roomNumber)
        {
            RoomText.text = "Room: " + roomNumber;
        }

        /// <summary>
        /// Gets the value of the resolve on device checkbox.
        /// </summary>
        /// <returns>The value of the resolve on device checkbox.</returns>
        public bool GetResolveOnDeviceValue()
        {
            return ResolveOnDeviceToggle.isOn;
        }

        /// <summary>
        /// Gets the value of the room number input field.
        /// </summary>
        /// <returns>The value of the room number input field.</returns>
        public int GetRoomInputValue()
        {
            int roomNumber;
            if (int.TryParse(RoomInputField.text, out roomNumber))
            {
                return roomNumber;
            }

            return 0;
        }

        /// <summary>
        /// Gets the value of the ip address input field.
        /// </summary>
        /// <returns>The value of the ip address input field.</returns>
        public string GetIpAddressInputValue()
        {
            return IpAddressInputField.text;
        }

        /// <summary>
        /// Handles a change to the "Resolve on Device" checkbox.
        /// </summary>
        /// <param name="isResolveOnDevice">If set to <c>true</c> resolve on device.</param>
        public void OnResolveOnDeviceValueChanged(bool isResolveOnDevice)
        {
            IpAddressInputField.interactable = !isResolveOnDevice;
        }

        /// <summary>
        /// Gets the device ip address.
        /// </summary>
        /// <returns>The device ip address.</returns>
        private string _GetDeviceIpAddress()
        {
            string ipAddress = "Unknown";
#if UNITY_2018_2_OR_NEWER
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);

            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = address.ToString();
                    break;
                }
            }
#else
            ipAddress = Network.player.ipAddress;
#endif
            return ipAddress;
        }
    }
}

//-----------------------------------------------------------------------
// <copyright file="NetworkManagerUIController.cs" company="Google LLC">
//
// Copyright 2018 Google LLC. All Rights Reserved.
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
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.Networking.Match;
    using UnityEngine.Networking.Types;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// Controller managing UI for joining and creating rooms.
    /// </summary>
#pragma warning disable 618
    [RequireComponent(typeof(CloudAnchorsNetworkManager))]
#pragma warning restore 618
    public class NetworkManagerUIController : MonoBehaviour
    {
        /// <summary>
        /// The snackbar text.
        /// </summary>
        public Text SnackbarText;

        /// <summary>
        /// The Label showing the current active room.
        /// </summary>
        public GameObject CurrentRoomLabel;

        /// <summary>
        /// The return to lobby button in AR Scene.
        /// </summary>
        public GameObject ReturnButton;

        /// <summary>
        /// The Cloud Anchors Example Controller.
        /// </summary>
        public CloudAnchorsExampleController CloudAnchorsExampleController;

        /// <summary>
        /// The Panel containing the list of available rooms to join.
        /// </summary>
        public GameObject RoomListPanel;

        /// <summary>
        /// Text indicating that no previous rooms exist.
        /// </summary>
        public Text NoPreviousRoomsText;

        /// <summary>
        /// The prefab for a row in the available rooms list.
        /// </summary>
        public GameObject JoinRoomListRowPrefab;

        /// <summary>
        /// The number of matches that will be shown.
        /// </summary>
        private const int _matchPageSize = 5;

        /// <summary>
        /// The Network Manager.
        /// </summary>
#pragma warning disable 618
        private CloudAnchorsNetworkManager _manager;
#pragma warning restore 618

        /// <summary>
        /// The current room number.
        /// </summary>
        private string _currentRoomNumber;

        /// <summary>
        /// The Join Room buttons.
        /// </summary>
        private List<GameObject> _joinRoomButtonsPool = new List<GameObject>();

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            // Initialize the pool of Join Room buttons.
            for (int i = 0; i < _matchPageSize; i++)
            {
                GameObject button = Instantiate(JoinRoomListRowPrefab);
                button.transform.SetParent(RoomListPanel.transform, false);
                button.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(0, -(100 * i));
                button.SetActive(true);
                button.GetComponentInChildren<Text>().text = string.Empty;
                _joinRoomButtonsPool.Add(button);
            }

#pragma warning disable 618
            _manager = GetComponent<CloudAnchorsNetworkManager>();
#pragma warning restore 618
            _manager.StartMatchMaker();
            _manager.matchMaker.ListMatches(
                startPageNumber: 0,
                resultPageSize: _matchPageSize,
                matchNameFilter: string.Empty,
                filterOutPrivateMatchesFromResults: false,
                eloScoreTarget: 0,
                requestDomain: 0,
                callback: OnMatchList);
            ChangeLobbyUIVisibility(true);
        }

        /// <summary>
        /// Handles the user intent to create a new room.
        /// </summary>
        public void OnCreateRoomClicked()
        {
            _manager.matchMaker.CreateMatch(_manager.matchName, _manager.matchSize,
                                           true, string.Empty, string.Empty, string.Empty,
                                           0, 0, OnMatchCreate);
        }

        /// <summary>
        /// Handles a user intent to return to the lobby.
        /// </summary>
        public void OnReturnToLobbyClick()
        {
            ReturnButton.GetComponent<Button>().interactable = false;
            if (_manager.matchInfo == null)
            {
                OnMatchDropped(true, null);
                return;
            }

            _manager.matchMaker.DropConnection(_manager.matchInfo.networkId,
                _manager.matchInfo.nodeId, _manager.matchInfo.domain, OnMatchDropped);
        }

        /// <summary>
        /// Handles the user intent to refresh the room list.
        /// </summary>
        public void OnRefhreshRoomListClicked()
        {
            _manager.matchMaker.ListMatches(
                startPageNumber: 0,
                resultPageSize: _matchPageSize,
                matchNameFilter: string.Empty,
                filterOutPrivateMatchesFromResults: false,
                eloScoreTarget: 0,
                requestDomain: 0,
                callback: OnMatchList);
        }

        /// <summary>
        /// Callback indicating that the Cloud Anchor was instantiated and the host request was
        /// made.
        /// </summary>
        /// <param name="isHost">Indicates whether this player is the host.</param>
        public void OnAnchorInstantiated(bool isHost)
        {
            if (isHost)
            {
                SnackbarText.text = "Hosting Cloud Anchor...";
            }
            else
            {
                SnackbarText.text =
                    "Cloud Anchor added to session! Attempting to resolve anchor...";
            }
        }

        /// <summary>
        /// Callback indicating that the Cloud Anchor was hosted.
        /// </summary>
        /// <param name="success">If set to <c>true</c> indicates the Cloud Anchor was hosted
        /// successfully.</param>
        /// <param name="response">The response string received.</param>
        public void OnAnchorHosted(bool success, string response)
        {
            if (success)
            {
                SnackbarText.text = "Cloud Anchor successfully hosted! Tap to place more stars.";
            }
            else
            {
                SnackbarText.text = "Cloud Anchor could not be hosted. " + response;
            }
        }

        /// <summary>
        /// Callback indicating that the Cloud Anchor was resolved.
        /// </summary>
        /// <param name="success">If set to <c>true</c> indicates the Cloud Anchor was resolved
        /// successfully.</param>
        /// <param name="response">The response string received.</param>
        public void OnAnchorResolved(bool success, string response)
        {
            if (success)
            {
                SnackbarText.text = "Cloud Anchor successfully resolved! Tap to place more stars.";
            }
            else
            {
                SnackbarText.text =
                    "Cloud Anchor could not be resolved. Will attempt again. " + response;
            }
        }

        /// <summary>
        /// Use the snackbar to display the error message.
        /// </summary>
        /// <param name="debugMessage">The debug message to be displayed on the snackbar.</param>
        public void ShowDebugMessage(string debugMessage)
        {
            SnackbarText.text = debugMessage;
        }

        /// <summary>
        /// Handles the user intent to join the room associated with the button clicked.
        /// </summary>
        /// <param name="match">The information about the match that the user intents to
        /// join.</param>
#pragma warning disable 618
        private void OnJoinRoomClicked(MatchInfoSnapshot match)
#pragma warning restore 618
        {
            _manager.matchName = match.name;
            _manager.matchMaker.JoinMatch(match.networkId, string.Empty, string.Empty,
                                         string.Empty, 0, 0, OnMatchJoined);
        }

        /// <summary>
        /// Callback that happens when a <see cref="NetworkMatch.ListMatches"/> request has been
        /// processed on the server.
        /// </summary>
        /// <param name="success">Indicates if the request succeeded.</param>
        /// <param name="extendedInfo">A text description for the error if success is false.</param>
        /// <param name="matches">A list of matches corresponding to the filters set in the initial
        /// list request.</param>
#pragma warning disable 618
        private void OnMatchList(
            bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
#pragma warning restore 618
        {
            if (!success)
            {
                SnackbarText.text = "Could not list matches: " + extendedInfo;
                return;
            }

            _manager.OnMatchList(success, extendedInfo, matches);
            if (_manager.matches != null)
            {
                // Reset all buttons in the pool.
                foreach (GameObject button in _joinRoomButtonsPool)
                {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                    button.GetComponentInChildren<Text>().text = string.Empty;
                }

                NoPreviousRoomsText.gameObject.SetActive(_manager.matches.Count == 0);

                // Add buttons for each existing match.
                int i = 0;
#pragma warning disable 618
                foreach (var match in _manager.matches)
#pragma warning restore 618
                {
                    if (i >= _matchPageSize)
                    {
                        break;
                    }

                    var text = "Room " + GetRoomNumberFromNetworkId(match.networkId);
                    GameObject button = _joinRoomButtonsPool[i++];
                    button.GetComponentInChildren<Text>().text = text;
                    button.GetComponentInChildren<Button>().onClick.AddListener(() =>
                        OnJoinRoomClicked(match));
                    button.GetComponentInChildren<Button>().onClick.AddListener(
                        CloudAnchorsExampleController.OnEnterResolvingModeClick);
                    button.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Callback that happens when a <see cref="NetworkMatch.CreateMatch"/> request has been
        /// processed on the server.
        /// </summary>
        /// <param name="success">Indicates if the request succeeded.</param>
        /// <param name="extendedInfo">A text description for the error if success is false.</param>
        /// <param name="matchInfo">The information about the newly created match.</param>
#pragma warning disable 618
        private void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
#pragma warning restore 618
        {
            if (!success)
            {
                SnackbarText.text = "Could not create match: " + extendedInfo;
                return;
            }

            _manager.OnMatchCreate(success, extendedInfo, matchInfo);
            _currentRoomNumber = GetRoomNumberFromNetworkId(matchInfo.networkId);
            SnackbarText.text = "Connecting to server...";
            ChangeLobbyUIVisibility(false);
            CurrentRoomLabel.GetComponentInChildren<Text>().text = "Room: " + _currentRoomNumber;
        }

        /// <summary>
        /// Callback that happens when a <see cref="NetworkMatch.JoinMatch"/> request has been
        /// processed on the server.
        /// </summary>
        /// <param name="success">Indicates if the request succeeded.</param>
        /// <param name="extendedInfo">A text description for the error if success is false.</param>
        /// <param name="matchInfo">The info for the newly joined match.</param>
#pragma warning disable 618
        private void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
#pragma warning restore 618
        {
            if (!success)
            {
                SnackbarText.text = "Could not join to match: " + extendedInfo;
                return;
            }

            _manager.OnMatchJoined(success, extendedInfo, matchInfo);
            _currentRoomNumber = GetRoomNumberFromNetworkId(matchInfo.networkId);
            SnackbarText.text = "Connecting to server...";
            ChangeLobbyUIVisibility(false);
            CurrentRoomLabel.GetComponentInChildren<Text>().text = "Room: " + _currentRoomNumber;
        }

        /// <summary>
        /// Callback that happens when a <see cref="NetworkMatch.DropConnection"/> request has been
        /// processed on the server.
        /// </summary>
        /// <param name="success">Indicates if the request succeeded.</param>
        /// <param name="extendedInfo">A text description for the error if success is false.
        /// </param>
        private void OnMatchDropped(bool success, string extendedInfo)
        {
            ReturnButton.GetComponent<Button>().interactable = true;
            if (!success)
            {
                SnackbarText.text = "Could not drop the match: " + extendedInfo;
                return;
            }

            _manager.OnDropConnection(success, extendedInfo);
#pragma warning disable 618
            NetworkManager.Shutdown();
#pragma warning restore 618
            SceneManager.LoadScene("CloudAnchors");
        }

        /// <summary>
        /// Changes the lobby UI Visibility by showing or hiding the buttons.
        /// </summary>
        /// <param name="visible">If set to <c>true</c> the lobby UI will be visible. It will be
        /// hidden otherwise.</param>
        private void ChangeLobbyUIVisibility(bool visible)
        {
            foreach (GameObject button in _joinRoomButtonsPool)
            {
                bool active = visible && button.GetComponentInChildren<Text>().text != string.Empty;
                button.SetActive(active);
            }

            CloudAnchorsExampleController.OnLobbyVisibilityChanged(visible);
        }

        private string GetRoomNumberFromNetworkId(NetworkID networkID)
        {
            return (System.Convert.ToInt64(networkID.ToString()) % 10000).ToString();
        }
    }
}

﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GoogleARCore;

namespace GoogleARCore.HelloAR
{
    public class ARCoreForTownController : MonoBehaviour
    {

        /// <summary>
        /// The Town Controller to use to place town
        /// </summary>
        public TownController m_townController;

        /// <summary>
        /// The ARCore Device prefab.
        /// </summary>
        public GameObject m_ARCoreDevice;

        /// <summary>
        /// Renders points detected by ARCore.
        /// </summary>
        public GameObject m_pointCloud;

        /// <summary>
        /// The first-person camera being used to render the passthrough camera.
        /// </summary>
        public Camera m_firstPersonCamera;

        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject m_trackedPlanePrefab;

        /// <summary>
        /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
        /// </summary>
        public GameObject m_searchingForPlaneUI;

        private List<TrackedPlane> m_newPlanes = new List<TrackedPlane> ();

        private List<TrackedPlane> m_allPlanes = new List<TrackedPlane> ();

        private List<GameObject> m_allPlaneObjects = new List<GameObject> ();

        private Color[] m_planeColors = new Color[] {
            new Color (1.0f, 1.0f, 1.0f),
            new Color (0.956f, 0.262f, 0.211f),
            new Color (0.913f, 0.117f, 0.388f),
            new Color (0.611f, 0.152f, 0.654f),
            new Color (0.403f, 0.227f, 0.717f),
            new Color (0.247f, 0.317f, 0.709f),
            new Color (0.129f, 0.588f, 0.952f),
            new Color (0.011f, 0.662f, 0.956f),
            new Color (0f, 0.737f, 0.831f),
            new Color (0f, 0.588f, 0.533f),
            new Color (0.298f, 0.686f, 0.313f),
            new Color (0.545f, 0.764f, 0.290f),
            new Color (0.803f, 0.862f, 0.223f),
            new Color (1.0f, 0.921f, 0.231f),
            new Color (1.0f, 0.756f, 0.027f)
        };

        public Camera Init ()
        {
            m_ARCoreDevice.SetActive (true);
            m_pointCloud.SetActive (true);
            m_searchingForPlaneUI.SetActive (true);
            return m_firstPersonCamera;
        }

        public void ARUpdate ()
        {
            _QuitOnConnectionErrors ();

            // The tracking state must be FrameTrackingState.Tracking in order to access the Frame.
            if (Frame.TrackingState != FrameTrackingState.Tracking) {
                const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
                Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
                return;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Frame.GetNewPlanes (ref m_newPlanes);

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
            for (int i = 0; i < m_newPlanes.Count; i++) {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
                GameObject planeObject = Instantiate (m_trackedPlanePrefab, Vector3.zero, Quaternion.identity,
                                             transform);
                planeObject.GetComponent<TrackedPlaneVisualizer> ().SetTrackedPlane (m_newPlanes [i]);

                // Apply a random color and grid rotation.
                planeObject.GetComponent<Renderer> ().material.SetColor ("_GridColor", m_planeColors [Random.Range (0,
                    m_planeColors.Length - 1)]);
                planeObject.GetComponent<Renderer> ().material.SetFloat ("_UvRotation", Random.Range (0.0f, 360.0f));
                m_allPlaneObjects.Add (planeObject);
            }

            // Disable the snackbar UI when no planes are valid.
            bool showSearchingUI = true;
            Frame.GetAllPlanes (ref m_allPlanes);
            for (int i = 0; i < m_allPlanes.Count; i++) {
                if (m_allPlanes [i].IsValid) {
                    showSearchingUI = false;
                    break;
                }
            }

            m_searchingForPlaneUI.SetActive (showSearchingUI);

            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch (0)).phase != TouchPhase.Began) {
                return;
            }

            // Has an object been touched?
            Ray raycast = m_firstPersonCamera.ScreenPointToRay (Input.GetTouch (0).position);
            RaycastHit raycastHit;
            if (Physics.Raycast (raycast, out raycastHit)) {
                if (raycastHit.collider.name == "ad-cube" || raycastHit.collider.CompareTag ("Character")) {
                    // Prevent object touch from spawning character or ad
                    return;
                }
            }

            // Has a plane been touched?
            TrackableHit hit;
            TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds | TrackableHitFlag.PlaneWithinPolygon;
            if (Session.Raycast (m_firstPersonCamera.ScreenPointToRay (touch.position), raycastFilter, out hit)) {
                Debug.Log ("Plane touched");
                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                // world evolves.
                GoogleARCore.Anchor anchor = Session.CreateAnchor (hit.Point, Quaternion.identity);

                GameObject townObject = m_townController.PlaceTown (hit.Point, anchor.transform);

                if (townObject != null) {
                    // Use a plane attachment component to maintain Andy's y-offset from the plane
                    // (occurs after anchor updates).
                    townObject.GetComponent<PlaneAttachment> ().Attach (hit.Plane);
                }

                // Hide point cloud and plane grids
                m_pointCloud.GetComponent<MeshRenderer> ().enabled = false;
                foreach (GameObject plane in m_allPlaneObjects) {
                    plane.GetComponent<TrackedPlaneVisualizer> ().enabled = false;
                    plane.GetComponent<Renderer> ().enabled = false;
                }
            }
        }

        /// <summary>
        /// Quit the application if there was a connection error for the ARCore session.
        /// </summary>
        private void _QuitOnConnectionErrors ()
        {
            // Do not update if ARCore is not tracking.
            if (Session.ConnectionState == SessionConnectionState.DeviceNotSupported) {
                _ShowAndroidToastMessage ("This device does not support ARCore.");
                Application.Quit ();
            } else if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission) {
                _ShowAndroidToastMessage ("Camera permission is needed to run this application.");
                Application.Quit ();
            } else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed) {
                _ShowAndroidToastMessage ("ARCore encountered a problem connecting.  Please start the app again.");
                Application.Quit ();
            }
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        /// <param name="length">Toast message time length.</param>
        private static void _ShowAndroidToastMessage (string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");

            if (unityActivity != null) {
                AndroidJavaClass toastClass = new AndroidJavaClass ("android.widget.Toast");
                unityActivity.Call ("runOnUiThread", new AndroidJavaRunnable (() => {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject> ("makeText", unityActivity,
                                                        message, 0);
                    toastObject.Call ("show");
                }));
            }
        }
    }
}

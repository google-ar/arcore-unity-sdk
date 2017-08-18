//-----------------------------------------------------------------------
// <copyright file="TrackedPlaneManager.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;

    public class TrackedPlaneManager
    {
        private List<TrackedPlaneRecord> m_allPlaneRecords = new List<TrackedPlaneRecord>();

        private List<TrackedPlaneRecord> m_newPlaneRecords = new List<TrackedPlaneRecord>();

        private float m_lastUpdateTime;

        private List<ApiPlaneData> m_mostRecentApiPlanes = new List<ApiPlaneData>();

        private HashSet<int> m_tmpLastestPlanesHash = new HashSet<int>();

        public void GetNewPlanes(ref List<TrackedPlane> newPlanes)
        {
            newPlanes.Clear();
            for (int i = 0; i < m_newPlaneRecords.Count; i++)
            {
                newPlanes.Add(m_newPlaneRecords[i].m_plane);
            }
        }

        public void GetAllPlanes(ref List<TrackedPlane> allPlanes)
        {
            allPlanes.Clear();
            for (int i = 0; i < m_allPlaneRecords.Count; i++)
            {
                allPlanes.Add(m_allPlaneRecords[i].m_plane);
            }
        }

        public void OnApplicationPause(bool isPaused)
        {
            if (!isPaused)
            {
                return;
            }

            for (int i = 0; i < m_allPlaneRecords.Count; i++)
            {
                ApiPlaneData invalidPlane = new ApiPlaneData();
                invalidPlane.id = m_allPlaneRecords[i].m_planeData.id;
                m_allPlaneRecords[i].m_updatePlane(invalidPlane, null, true);
            }
        }

        public void EarlyUpdate()
        {
            m_newPlaneRecords.Clear();

            const float MIN_DELTA_BETWEEN_UPDATES = .1f;
            if ((Time.time - m_lastUpdateTime) < MIN_DELTA_BETWEEN_UPDATES)
            {
                return;
            }

            // We are performing an update of the planes, update the time/frame tracking and clear the new planes list.
            m_lastUpdateTime = Time.time;

            var forceUpdate = _UpdateMostRecentApiPlanes();
            _UpdatePlanes(m_mostRecentApiPlanes, m_newPlaneRecords, m_allPlaneRecords, forceUpdate);

            return;
        }

        private bool _UpdateMostRecentApiPlanes()
        {
            IntPtr apiPlanesPtr = IntPtr.Zero;
            int planeCount = 0;
            if (TangoClientApi.TangoService_Experimental_getPlanes(ref apiPlanesPtr, ref planeCount).IsTangoFailure())
            {
                for (int i =0; i < m_mostRecentApiPlanes.Count; i++)
                {
                    ApiPlaneData planeData = m_mostRecentApiPlanes[i];
                    planeData.isValid = false;
                    m_mostRecentApiPlanes[i] = planeData;
                }

                return true;
            }

            // The planes api is handling a COM reset, and the current state is not available. Leave the most recent
            // Api planes collection unchanged.
            if (apiPlanesPtr == null)
            {
                return false;
            }

            // Marshal the most recent planes returned from the Api into m_mostRecentApiPlanes.
            m_mostRecentApiPlanes.Clear();
            MarshalingHelper.AddUnmanagedStructArrayToList<ApiPlaneData>(apiPlanesPtr, planeCount,
                m_mostRecentApiPlanes);

            if (TangoClientApi.TangoPlaneData_free(apiPlanesPtr, planeCount).IsTangoFailure())
            {
                ARDebug.LogErrorFormat("Failed to deallocate planes from the ARCore API.");
            }

            return false;
        }

        private void _UpdatePlanes(List<ApiPlaneData> latestPlaneData, List<TrackedPlaneRecord> newPlaneRecords,
            List<TrackedPlaneRecord> planeRecords, bool forceUpdate)
        {
            // Add latest planedata to a convenient hash on ID (note: not hashing plane itself to avoid potential boxing
            // of the struct on comparison overloads).
            m_tmpLastestPlanesHash.Clear();
            for (int i = 0; i < latestPlaneData.Count; i++)
            {
                m_tmpLastestPlanesHash.Add(latestPlaneData[i].id);
            }

            // Create or update all planes that appear in latestPlaneData.
            newPlaneRecords.Clear();
            for (int i = 0; i < latestPlaneData.Count; i++)
            {
                int planeRecordIndex = _FindPlaneRecordById(latestPlaneData[i].id, planeRecords);
                if (planeRecordIndex != -1)
                {
                    planeRecords[planeRecordIndex] = new TrackedPlaneRecord(planeRecords[planeRecordIndex].m_plane,
                        latestPlaneData[i], planeRecords[planeRecordIndex].m_updatePlane);
                    planeRecords[planeRecordIndex].m_updatePlane(latestPlaneData[i], null, forceUpdate);
                }
                else
                {
                    Action<ApiPlaneData, TrackedPlane, bool> updatePlane;
                    var trackedPlane = new TrackedPlane(latestPlaneData[i], out updatePlane);
                    newPlaneRecords.Add(new TrackedPlaneRecord(trackedPlane, latestPlaneData[i], updatePlane));
                }
            }

            // Add new planes into plane records
            for (int i = 0; i < newPlaneRecords.Count; i++)
            {
                planeRecords.Add(newPlaneRecords[i]);
            }

            // Invalidate planes that were previously tracking but not present in latestPlaneData.  Delete planes that
            // are invalid or subsumed.
            for (int i = planeRecords.Count - 1; i >= 0; i--)
            {
                if (!m_tmpLastestPlanesHash.Contains(planeRecords[i].m_planeData.id))
                {
                    ApiPlaneData invalidPlane = new ApiPlaneData();
                    invalidPlane.id = planeRecords[i].m_planeData.id;
                    planeRecords[i].m_updatePlane(invalidPlane, null, true);
                    planeRecords.RemoveAt(i);
                }
                else if (planeRecords[i].m_planeData.subsumedBy > -1)
                {
                    int subsumerIndex = _FindPlaneRecordById(planeRecords[i].m_planeData.subsumedBy, planeRecords);
                    if (subsumerIndex == -1)
                    {
                        ARDebug.LogError("Failed to find a record of the subsuming plane.");
                        continue;
                    }

                    planeRecords[i].m_updatePlane(planeRecords[i].m_planeData, planeRecords[subsumerIndex].m_plane,
                        true);
                    planeRecords.RemoveAt(i);
                }
            }
        }

        private int _FindPlaneRecordById(int id, List<TrackedPlaneRecord> planeRecords)
        {
            for (int i = 0; i < planeRecords.Count; i++)
            {
                if (planeRecords[i].m_planeData.id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        private struct TrackedPlaneRecord
        {
            public TrackedPlane m_plane;

            public ApiPlaneData m_planeData;

            public Action<ApiPlaneData, TrackedPlane, bool> m_updatePlane;

            public TrackedPlaneRecord(TrackedPlane plane, ApiPlaneData planeData,
                Action<ApiPlaneData, TrackedPlane, bool> updatePlane)
            {
                m_plane = plane;
                m_planeData = planeData;
                m_updatePlane = updatePlane;
            }
        }
    }
}

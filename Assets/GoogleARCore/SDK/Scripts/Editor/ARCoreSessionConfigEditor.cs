//-----------------------------------------------------------------------
// <copyright file="ARCoreSessionConfigEditor.cs" company="Google">
//
// Copyright 2019 Google Inc. All Rights Reserved.
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
    using GoogleARCore;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ARCoreSessionConfig))]
    internal class ARCoreSessionConfigEditor : Editor
    {
        /// <summary>
        /// Implements custom inspector for ARCoreSessionConfig.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ARCoreSessionConfig config = (ARCoreSessionConfig)target;
            MonoScript script = MonoScript.FromScriptableObject(config);
            GUI.enabled = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Script");
            EditorGUILayout.ObjectField(script, typeof(ARCoreSessionConfig), true);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("MatchCameraFramerate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PlaneFindingMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LightEstimationMode"));
            GUI.enabled = false;
            EditorGUILayout.TextArea(
                "When \"Environmental HDR Without Reflections\" light is selected, ARCore:\n" +
                "1. Updates rotation and color of the directional light on the " +
                "EnvironmentalLight component.\n" +
                "2. Updates Skybox ambient lighting Spherical Harmonics.\n\n" +
                "When \"Environmental HDR With Reflections\" light is selected, ARCore also:\n" +
                "3. Overrides the environmental reflections in the scene with a " +
                "realtime reflections cubemap.", EditorStyles.textArea);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableCloudAnchor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AugmentedImageDatabase"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraFocusMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AugmentedFaceMode"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}

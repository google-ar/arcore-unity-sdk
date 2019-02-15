// <copyright file="PointcloudVisualizerEditor.cs" company="Google">
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

namespace GoogleARCore.Examples.Common
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Controls how the PointcloudVisualizer component will be rendered in the Editor GUI.
    /// </summary>
    [CustomEditor(typeof(PointcloudVisualizer))]
    [CanEditMultipleObjects]
    public class PointcloudVisualizerEditor : Editor
    {
        private SerializedProperty m_Script;
        private SerializedProperty m_PointColor;
        private SerializedProperty m_DefaultSize;
        private SerializedProperty m_MaxPointCount;
        private SerializedProperty m_EnablePopAnimation;
        private SerializedProperty m_MaxPointsToAddPerFrame;
        private SerializedProperty m_AnimationDuration;
        private SerializedProperty m_PopSize;

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            m_PointColor = serializedObject.FindProperty("PointColor");
            m_DefaultSize = serializedObject.FindProperty("m_DefaultSize");
            m_MaxPointCount = serializedObject.FindProperty("m_MaxPointCount");
            m_EnablePopAnimation = serializedObject.FindProperty("EnablePopAnimation");
            m_MaxPointsToAddPerFrame = serializedObject.FindProperty("MaxPointsToAddPerFrame");
            m_AnimationDuration = serializedObject.FindProperty("AnimationDuration");
            m_PopSize = serializedObject.FindProperty("m_PopSize");
        }

        /// <summary>
        /// The Unity OnInspectorGUI() method.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            var pointcloudVisualizerScript = target as PointcloudVisualizer;

            EditorGUILayout.PropertyField(
                m_PointColor,
                new GUIContent(
                    "Point Color",
                    _GetTooltip(pointcloudVisualizerScript, "PointColor")));

            EditorGUILayout.PropertyField(
                m_DefaultSize,
                new GUIContent(
                    "Default Size",
                    _GetTooltip(pointcloudVisualizerScript, "m_DefaultSize")));

            EditorGUILayout.PropertyField(
                m_MaxPointCount,
                new GUIContent(
                    "Max Point Count",
                    _GetTooltip(pointcloudVisualizerScript, "m_MaxPointCount")));

            EditorGUILayout.PropertyField(
                m_EnablePopAnimation,
                new GUIContent(
                    "Enable Pop Animation",
                    _GetTooltip(pointcloudVisualizerScript, "EnablePopAnimation")));

            // Hide animation related fields if the pop animation is disabled.
            using (var group = new EditorGUILayout.FadeGroupScope(
                Convert.ToSingle(pointcloudVisualizerScript.EnablePopAnimation)))
            {
                if (group.visible == true)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(
                        m_MaxPointsToAddPerFrame,
                        new GUIContent(
                            "Max Points To Add Per Frame",
                            _GetTooltip(pointcloudVisualizerScript, "MaxPointsToAddPerFrame")));

                    EditorGUILayout.PropertyField(
                        m_AnimationDuration,
                        new GUIContent(
                            "Animation Duration",
                            _GetTooltip(pointcloudVisualizerScript, "AnimationDuration")));

                    EditorGUILayout.PropertyField(
                        m_PopSize,
                        new GUIContent(
                            "Pop Size",
                            _GetTooltip(pointcloudVisualizerScript, "m_PopSize")));

                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Gets the tooltip attribute from a field.
        /// </summary>
        /// <returns>The string of the tooltip attribute.</returns>
        /// <param name="obj">The object containing the field.</param>
        /// <param name="fieldName">The field name.</param>
        private string _GetTooltip(object obj, string fieldName)
        {
            FieldInfo field =
                obj.GetType().GetField(
                    fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            TooltipAttribute[] attributes =
                field.GetCustomAttributes(typeof(TooltipAttribute), true) as TooltipAttribute[];

            return attributes.Length > 0 ? attributes[0].tooltip : String.Empty;
        }
    }
}

// <copyright file="PointcloudVisualizerEditor.cs" company="Google LLC">
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
        private SerializedProperty _script;
        private SerializedProperty _pointColor;
        private SerializedProperty _defaultSize;
        private SerializedProperty _maxPointCount;
        private SerializedProperty _enablePopAnimation;
        private SerializedProperty _maxPointsToAddPerFrame;
        private SerializedProperty _animationDuration;
        private SerializedProperty _popSize;

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            _script = serializedObject.FindProperty("m_Script");
            if (_script == null)
            {
                // Early versions of Unity used `_script` instead of `m_Script`.
                _script = serializedObject.FindProperty("_script");
            }

            _pointColor = serializedObject.FindProperty("PointColor");
            _defaultSize = serializedObject.FindProperty("_defaultSize");
            _maxPointCount = serializedObject.FindProperty("_maxPointCount");
            _enablePopAnimation = serializedObject.FindProperty("EnablePopAnimation");
            _maxPointsToAddPerFrame = serializedObject.FindProperty("MaxPointsToAddPerFrame");
            _animationDuration = serializedObject.FindProperty("AnimationDuration");
            _popSize = serializedObject.FindProperty("_popSize");
        }

        /// <summary>
        /// The Unity OnInspectorGUI() method.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Only attempt to add built-in srcipt property if it was actually found in OnEnable().
            if (_script != null)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_script, true, new GUILayoutOption[0]);
                GUI.enabled = true;
            }

            var pointcloudVisualizerScript = target as PointcloudVisualizer;

            EditorGUILayout.PropertyField(
                _pointColor,
                new GUIContent(
                    "Point Color",
                    GetTooltip(pointcloudVisualizerScript, "PointColor")));

            EditorGUILayout.PropertyField(
                _defaultSize,
                new GUIContent(
                    "Default Size",
                    GetTooltip(pointcloudVisualizerScript, "_defaultSize")));

            EditorGUILayout.PropertyField(
                _maxPointCount,
                new GUIContent(
                    "Max Point Count",
                    GetTooltip(pointcloudVisualizerScript, "_maxPointCount")));

            EditorGUILayout.PropertyField(
                _enablePopAnimation,
                new GUIContent(
                    "Enable Pop Animation",
                    GetTooltip(pointcloudVisualizerScript, "EnablePopAnimation")));

            // Hide animation related fields if the pop animation is disabled.
            using (var group = new EditorGUILayout.FadeGroupScope(
                Convert.ToSingle(pointcloudVisualizerScript.EnablePopAnimation)))
            {
                if (group.visible == true)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(
                        _maxPointsToAddPerFrame,
                        new GUIContent(
                            "Max Points To Add Per Frame",
                            GetTooltip(pointcloudVisualizerScript, "MaxPointsToAddPerFrame")));

                    EditorGUILayout.PropertyField(
                        _animationDuration,
                        new GUIContent(
                            "Animation Duration",
                            GetTooltip(pointcloudVisualizerScript, "AnimationDuration")));

                    EditorGUILayout.PropertyField(
                        _popSize,
                        new GUIContent(
                            "Pop Size",
                            GetTooltip(pointcloudVisualizerScript, "_popSize")));

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
        private string GetTooltip(object obj, string fieldName)
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

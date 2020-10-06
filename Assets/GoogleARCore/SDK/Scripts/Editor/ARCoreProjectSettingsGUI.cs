//-----------------------------------------------------------------------
// <copyright file="ARCoreProjectSettingsGUI.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    // Common GUI for ARCore Project Settings, used by ARCoreProjectSettingsWindow and
    // ARCoreProjectSettingsProvider.
    internal class ARCoreProjectSettingsGUI
    {
        private static readonly float _toggleLabelWidth = 180;
        private static readonly float _groupLabelIndent = 15;
        private static readonly float _groupFieldIndent =
            EditorGUIUtility.labelWidth - _groupLabelIndent;

        /// <summary>
        /// Get the display name array of the provided enum array.
        /// This function would be called via reflection so it needs to be public.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="availbleEnums">Array of enums.</param>
        /// <returns>Array of strings representing those enums.</returns>
        public static string[] GetEnumNames<T>(Array availbleEnums)
        {
            return availbleEnums.OfType<T>().Select(
                    v =>
                    {
                        MemberInfo memberInfo = typeof(T).GetMember(v.ToString()).First();
                        DisplayNameAttribute displayName =
                            GetAttribute<DisplayNameAttribute>(memberInfo);
                        if (displayName == null)
                        {
                            return v.ToString();
                        }
                        else
                        {
                            return displayName.DisplayString;
                        }
                    })
                .ToArray();
        }

        // Render ARCore Project Settings for ARCoreProjectSettingsWindow and
        // ARCoreProjectSettingsProvider.
        internal static void OnGUI(bool renderForStandaloneWindow)
        {
            DrawGUI(ARCoreProjectSettings.Instance);
        }

        private static void DrawGUI(object targetObject)
        {
            Type targetType = targetObject.GetType();
            foreach (FieldInfo fieldInfo in targetType.GetFields())
            {
                if (!ShouldDisplay(fieldInfo, targetObject))
                {
                    continue;
                }

                string fieldName;
                DisplayNameAttribute fieldDisplay = GetAttribute<DisplayNameAttribute>(fieldInfo);
                if (fieldDisplay == null)
                {
                    fieldName = string.Format("{0}", fieldInfo.Name);
                }
                else
                {
                    fieldName = fieldDisplay.DisplayString;
                }

                if (fieldInfo.FieldType.IsEnum)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(fieldName);

                    Array availbleEnums;
                    EnumRangeAttribute enumRange = GetAttribute<EnumRangeAttribute>(fieldInfo);
                    if (enumRange != null)
                    {
                        MethodInfo checkingFunction =
                            targetType.GetMethod(enumRange.CheckingFunction);
                        availbleEnums = (Array)checkingFunction.Invoke(
                            targetObject, new object[] { });
                    }
                    else
                    {
                        availbleEnums = Enum.GetValues(fieldInfo.FieldType);
                    }

                    string[] enumNames =
                        (string[])typeof(ARCoreProjectSettingsGUI)
                        .GetMethod("GetEnumNames")
                        .MakeGenericMethod(fieldInfo.FieldType)
                        .Invoke(null, new object[] { availbleEnums });
                    var currentValue = fieldInfo.GetValue(targetObject);
                    int currentIndex = Array.IndexOf(availbleEnums, currentValue);
                    if (currentIndex == -1)
                    {
                        currentIndex = 0;
                    }

                    var selectedIndex =
                        EditorGUILayout.Popup(
                            currentIndex,
                            enumNames,
                            GUILayout.Width(_groupFieldIndent));
                    fieldInfo.SetValue(targetObject, availbleEnums.GetValue(selectedIndex));
                    EditorGUILayout.EndHorizontal();
                }
                else if (fieldInfo.FieldType == typeof(Boolean))
                {
                    float originalWidth = EditorGUIUtility.labelWidth;
                    Boolean value = (Boolean)fieldInfo.GetValue(targetObject);
                    EditorGUIUtility.labelWidth = _toggleLabelWidth;
                    value = UnityEditor.EditorGUILayout.Toggle(new GUIContent(fieldName), value);
                    EditorGUIUtility.labelWidth = originalWidth;
                    fieldInfo.SetValue(targetObject, value);
                }
                else if (fieldInfo.FieldType == typeof(string))
                {
                    EditorGUILayout.BeginHorizontal();
                    string value = (string)fieldInfo.GetValue(targetObject);
                    EditorGUILayout.LabelField(fieldName, GUILayout.Width(_groupFieldIndent));
                    value = EditorGUILayout.TextField(value);
                    fieldInfo.SetValue(targetObject, value);
                    EditorGUILayout.EndHorizontal();
                }
                else if (fieldInfo.FieldType == typeof(int))
                {
                    EditorGUILayout.BeginHorizontal();
                    int value = (int)fieldInfo.GetValue(targetObject);
                    EditorGUILayout.LabelField(fieldName, GUILayout.Width(_groupFieldIndent));
                    value = (int)Convert.ToInt32(EditorGUILayout.TextField(value.ToString()));
                    fieldInfo.SetValue(targetObject, value);
                    EditorGUILayout.EndHorizontal();
                }
                else if (fieldInfo.FieldType == typeof(long))
                {
                    EditorGUILayout.BeginHorizontal();
                    long value = (long)fieldInfo.GetValue(targetObject);
                    EditorGUILayout.LabelField(fieldName, GUILayout.Width(_groupFieldIndent));
                    value = (long)Convert.ToInt64(EditorGUILayout.TextField(value.ToString()));
                    fieldInfo.SetValue(targetObject, value);
                    EditorGUILayout.EndHorizontal();
                }
                else if (fieldInfo.FieldType == typeof(float))
                {
                    EditorGUILayout.BeginHorizontal();
                    float value = (float)fieldInfo.GetValue(targetObject);
                    EditorGUILayout.LabelField(fieldName, GUILayout.Width(_groupFieldIndent));
                    value = (float)Convert.ToSingle(EditorGUILayout.TextField(value.ToString()));
                    fieldInfo.SetValue(targetObject, value);
                    EditorGUILayout.EndHorizontal();
                }
                else if (fieldInfo.FieldType == typeof(double))
                {
                    EditorGUILayout.BeginHorizontal();
                    double value = (double)fieldInfo.GetValue(targetObject);
                    EditorGUILayout.LabelField(fieldName, GUILayout.Width(_groupFieldIndent));
                    value = (double)Convert.ToDouble(EditorGUILayout.TextField(value.ToString()));
                    fieldInfo.SetValue(targetObject, value);
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);
                DisplayHelpInfo(fieldInfo, targetObject);
            }
        }

        private static bool ShouldDisplay(FieldInfo fieldInfo, object targetObject)
        {
            Type targetType = targetObject.GetType();

            HideInInspector hideInInspector = GetAttribute<HideInInspector>(fieldInfo);
            if (hideInInspector != null)
            {
                return false;
            }

            DisplayConditionAttribute displayCondition =
                GetAttribute<DisplayConditionAttribute>(fieldInfo);
            if (displayCondition != null)
            {
                MethodInfo checkingFunction =
                    targetType.GetMethod(displayCondition.CheckingFunction);
                return (bool)checkingFunction.Invoke(
                    targetObject, new object[] { });
            }

            return true;
        }

        private static void DisplayHelpInfo(FieldInfo fieldInfo, object targetObject)
        {
            Type targetType = targetObject.GetType();

            DynamicHelpAttribute dynamicHelp = GetAttribute<DynamicHelpAttribute>(fieldInfo);
            HelpAttribute helpInfo;
            if (dynamicHelp != null)
            {
                MethodInfo checkingFunction =
                    targetType.GetMethod(dynamicHelp.CheckingFunction);
                helpInfo =
                    (HelpAttribute)checkingFunction.Invoke(
                        targetObject, new object[] { });
            }
            else
            {
                helpInfo = GetAttribute<HelpAttribute>(fieldInfo);
            }

            if (helpInfo != null)
            {
                MessageType messageType =
                    (MessageType)Enum.Parse(typeof(MessageType), helpInfo.MessageType.ToString());
                EditorGUILayout.HelpBox(helpInfo.HelpMessage, messageType);
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
        }

        private static T GetAttribute<T>(MemberInfo memberInfo)
        {
            object[] targetAttributes =
                memberInfo.GetCustomAttributes(typeof(T), false);
            if (targetAttributes.Length > 0)
            {
                return (T)targetAttributes[0];
            }

            return default(T);
        }
    }
}

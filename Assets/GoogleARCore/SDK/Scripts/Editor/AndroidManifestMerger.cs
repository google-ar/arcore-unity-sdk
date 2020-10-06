//-----------------------------------------------------------------------
// <copyright file="AndroidManifestMerger.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using GoogleARCore;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    internal class AndroidManifestMerger
    {
        private const string _xmlns = "http://schemas.android.com/apk/res/android";
        private const string _androidManifestFormat =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <manifest xmlns:android=""http://schemas.android.com/apk/res/android"">
                {0}
            </manifest>";

        private static readonly HashSet<string> _elementsAlwaysMerged =
            new HashSet<string>
                {
                    "application",
                    "data",
                    "path-permission",
                    "grant-uri-permission",
                    "uses-sdk",
                    "supports-screen",
                    "uses-configuration"
                };

        private static readonly HashSet<string> _elementsMergedByKey =
            new HashSet<string>
                {
                    "action",
                    "activity",
                    "category",
                    "instrumentation",
                    "meta-data",
                    "permission-group",
                    "permission",
                    "permission-tree",
                    "provider",
                    "receiver",
                    "service",
                    "supports-gl-texture",
                    "uses-library",
                    "uses-permission",
                };

        private static readonly HashSet<string> _elementsMergedOnlyChildren =
            new HashSet<string>
                {
                    "manifest",
                };

        /// <summary>
        /// Transfer an XML snippet into a valid AndroidManifest XDocument.
        /// </summary>
        /// <param name="snippet">The XML string snippet which should under node 'manifest'.
        /// </param>
        /// <returns>The transferred XDocument.</returns>
        public static XDocument TransferToXDocument(string snippet)
        {
            return XDocument.Parse(string.Format(_androidManifestFormat, snippet));
        }

        /// <summary>
        /// Merge two XElement into one.
        /// </summary>
        /// <param name="element1">The first XElement need to merged.</param>
        /// <param name="element2">The second XElement need to merged.</param>
        /// <returns>The merged XElement.</returns>
        public static XElement MergeXElement(XElement element1, XElement element2)
        {
            XElement resultElement = new XElement(element1.Name);

            if (_elementsMergedOnlyChildren.Contains(element1.Name.LocalName))
            {
                resultElement.ReplaceAttributes(element1.Attributes());
            }
            else
            {
                MergeAttributes(element1, element2, ref resultElement);
            }

            MergeChildren(element1, element2, ref resultElement);
            return resultElement;
        }

        private static void MergeAttributes(
            XElement element1, XElement element2, ref XElement resultElement)
        {
            resultElement.ReplaceAttributes(element1.Attributes());

            foreach (XAttribute attrInElement2 in element2.Attributes())
            {
                XAttribute attrInElement1 =
                    element1.Attributes(attrInElement2.Name).FirstOrDefault();

                if (attrInElement1 == null)
                {
                    resultElement.SetAttributeValue(
                        attrInElement2.Name, attrInElement2.Value);
                }
                else
                {
                    XNamespace androidNamespace = _xmlns;
                    if (attrInElement1.Value != attrInElement2.Value)
                    {
                        if ((element1.Name == "uses-feature" ||
                                element1.Name == "uses-library") &&
                            attrInElement1.Name == androidNamespace + "required")
                        {
                            bool result = bool.Parse(attrInElement1.Value) ||
                                bool.Parse(attrInElement2.Value);
                            resultElement.SetAttributeValue(
                                attrInElement1.Name, result.ToString());
                        }
                        else if (element1.Name == "uses-sdk" &&
                            (attrInElement1.Name == androidNamespace + "minSdkVersion" ||
                            attrInElement1.Name == androidNamespace + "targetSdkVersion"))
                        {
                            Int16 result = Math.Max(Int16.Parse(attrInElement1.Value),
                                Int16.Parse(attrInElement2.Value));
                            resultElement.SetAttributeValue(
                                attrInElement1.Name, result.ToString());
                        }
                        else
                        {
                            string errorMessage = string.Format(
                            "Element '{0}', android:name='{1}', Attribute '{2}'" +
                            " has value '{3}' isn't compatible with the previous setting '{4}'.",
                            element1.Name, GetAndroidAttribute(element1, "name"),
                            attrInElement1.Name, attrInElement2.Value, attrInElement1.Value);
                            Debug.LogError(errorMessage);
                            throw new BuildFailedException(errorMessage);
                        }
                    }
                }
            }
        }

        private static void MergeChildren(
            XElement element1, XElement element2, ref XElement resultElement)
        {
            foreach (XElement childElement1 in element1.Elements())
            {
                bool findSameKindElement = false;
                foreach (XElement childElement2 in element2.Elements())
                {
                    if (IsSameElement(childElement1, childElement2))
                    {
                        findSameKindElement = true;
                        resultElement.Add(
                            MergeXElement(childElement1, childElement2));
                        break;
                    }
                }

                if (!findSameKindElement)
                {
                    resultElement.Add(childElement1);
                }
            }

            foreach (XElement childElement2 in element2.Elements())
            {
                bool findSameKindElement = false;
                foreach (XElement resultChildElement in resultElement.Elements())
                {
                    if (IsSameElement(childElement2, resultChildElement))
                    {
                        findSameKindElement = true;
                        break;
                    }
                }

                if (!findSameKindElement)
                {
                    resultElement.Add(childElement2);
                }
            }
        }

        private static bool IsSameElement(XElement element1, XElement element2)
        {
            if (element1.Name != element2.Name)
            {
                return false;
            }

            if (_elementsMergedOnlyChildren.Contains(element1.Name.LocalName) ||
                _elementsAlwaysMerged.Contains(element1.Name.LocalName))
            {
                return true;
            }

            string androidName1 = GetAndroidAttribute(element1, "name");
            string androidName2 = GetAndroidAttribute(element2, "name");
            if (_elementsMergedByKey.Contains(element1.Name.LocalName) &&
                androidName1 == androidName2)
            {
                return true;
            }

            if (element1.Name == "uses-feature" &&
                androidName1 == androidName2 &&
                (androidName1 != string.Empty ||
                    GetAndroidAttribute(element1, "glEsVersion") ==
                    GetAndroidAttribute(element2, "glEsVersion")))
            {
                return true;
            }

            return false;
        }

        private static string GetAndroidAttribute(
            XElement element, string attributeName)
        {
            XNamespace androidNamespace = _xmlns;
            XAttribute androidAttribute = element.Attributes(
                androidNamespace + attributeName).FirstOrDefault();
            return androidAttribute == null ? string.Empty : androidAttribute.Value;
        }
    }
}

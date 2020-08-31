//-----------------------------------------------------------------------
// <copyright file="ARCoreUnitySDKManifest.cs" company="Unity">
//
// Copyright 2020 Unity Technologies All Rights Reserved.
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

#if UNITY_2020_1_OR_NEWER
using System.Xml;
using UnityEditor.Android;

namespace UnityEditor.XR.ARCore
{
    internal class ARCoreUnitySDKManifest : IPostGenerateGradleAndroidProject
    {
        static readonly string _androidURI = "http://schemas.android.com/apk/res/android";
        static readonly string _androidManifestPath = "/src/main/AndroidManifest.xml";
        static readonly string _androidPermissionCamera = "android.permission.CAMERA";

        public int callbackOrder
        {
            get { return 0; }
        }

        // This ensures the Android Manifest corresponds to
        // https://developers.google.com/ar/develop/java/enable-arcore
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifestPath = path + _androidManifestPath;
            var manifestDoc = new XmlDocument();
            manifestDoc.Load(manifestPath);

            var manifestNode = _FindFirstChild(manifestDoc, "manifest");
            if (manifestNode == null)
                return;

            var applicationNode = _FindFirstChild(manifestNode, "application");
            if (applicationNode == null)
                return;

            _FindOrCreateTagWithAttribute(manifestDoc, manifestNode,
                                          "uses-permission", "name", _androidPermissionCamera);
            _FindOrCreateTagWithAttributes(manifestDoc, applicationNode,
                                           "meta-data", "name",
                                           "unityplayer.SkipPermissionsDialog",
                                           "value", "true");
            _FindOrCreateTagWithAttributes(manifestDoc, applicationNode, "meta-data", "name",
                                           "unity.arcore-enable", "value", "true");

            manifestDoc.Save(manifestPath);
        }

        private XmlNode _FindFirstChild(XmlNode node, string tag)
        {
            if (node.HasChildNodes)
            {
                for (int i = 0; i < node.ChildNodes.Count; ++i)
                {
                    var child = node.ChildNodes[i];
                    if (child.Name == tag)
                        return child;
                }
            }

            return null;
        }

        private void _AppendNewAttribute(XmlDocument doc, XmlElement element,
            string attributeName, string attributeValue)
        {
            var attribute = doc.CreateAttribute(attributeName, _androidURI);
            attribute.Value = attributeValue;
            element.Attributes.Append(attribute);
        }

        private void _FindOrCreateTagWithAttribute(XmlDocument doc, XmlNode containingNode,
            string tagName, string attributeName, string attributeValue)
        {
            if (containingNode.HasChildNodes)
            {
                for (int i = 0; i < containingNode.ChildNodes.Count; ++i)
                {
                    var child = containingNode.ChildNodes[i];
                    if (child.Name == tagName)
                    {
                        var childElement = child as XmlElement;
                        if (childElement != null && childElement.HasAttributes)
                        {
                            var attribute = childElement.GetAttributeNode(attributeName,
                                                                          _androidURI);
                            if (attribute != null && attribute.Value == attributeValue)
                                return;
                        }
                    }
                }
            }

            // Didn't find it, so create it
            var element = doc.CreateElement(tagName);
            _AppendNewAttribute(doc, element, attributeName, attributeValue);
            containingNode.AppendChild(element);
        }

        private void _FindOrCreateTagWithAttributes(XmlDocument doc, XmlNode containingNode,
            string tagName, string firstAttributeName, string firstAttributeValue,
            string secondAttributeName, string secondAttributeValue)
        {
            if (containingNode.HasChildNodes)
            {
                for (int i = 0; i < containingNode.ChildNodes.Count; ++i)
                {
                    var childNode = containingNode.ChildNodes[i];
                    if (childNode.Name == tagName)
                    {
                        var childElement = childNode as XmlElement;
                        if (childElement != null && childElement.HasAttributes)
                        {
                            var firstAttribute = childElement.GetAttributeNode(
                                firstAttributeName, _androidURI);
                            if (firstAttribute == null ||
                                firstAttribute.Value != firstAttributeValue)
                                continue;

                            var secondAttribute = childElement.GetAttributeNode(
                                secondAttributeName, _androidURI);
                            if (secondAttribute != null)
                            {
                                secondAttribute.Value = secondAttributeValue;
                                return;
                            }

                            // Create it
                            _AppendNewAttribute(doc, childElement, secondAttributeName,
                                                secondAttributeValue);
                            return;
                        }
                    }
                }
            }

            // Didn't find it, so create it
            var element = doc.CreateElement(tagName);
            _AppendNewAttribute(doc, element, firstAttributeName, firstAttributeValue);
            _AppendNewAttribute(doc, element, secondAttributeName, secondAttributeValue);
            containingNode.AppendChild(element);
        }
    }
}

#endif //UNITY_2020_1_OR_NEWER

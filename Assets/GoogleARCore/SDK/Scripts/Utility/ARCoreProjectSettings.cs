//-----------------------------------------------------------------------
// <copyright file="ARCoreProjectSettings.cs" company="Google LLC">
//
// Copyright 2017 Google LLC
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
    using System.Reflection;
    using UnityEngine;


    /// <summary>
    /// Android Authentication Strategy.
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                     "SA1602:EnumerationItemsMustBeDocumented",
     Justification = "Internal.")]
    public enum AndroidAuthenticationStrategy
    {
        None = 0,
        [DisplayName("Api Key")]
        ApiKey = 1,
        [DisplayName("Keyless (recommended)")]
        Keyless = 2,
    }

    /// <summary>
    /// IOS Authentication Strategy.
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                     "SA1602:EnumerationItemsMustBeDocumented",
     Justification = "Internal.")]
    public enum IOSAuthenticationStrategy
    {
        None = 0,
        [DisplayName("Api Key")]
        ApiKey = 1,
        [DisplayName("Authentication Token (recommended)")]
        AuthenticationToken = 2,
    }


    [Serializable]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
     Justification = "Internal")]
    public class ARCoreProjectSettings
    {
        [HideInInspector]
        public string Version;

        [DisplayName("ARCore Required")]
        public bool IsARCoreRequired;

        [DisplayName("Instant Preview Enabled")]
        public bool IsInstantPreviewEnabled;

        [DisplayName("iOS Support Enabled")]
        public bool IsIOSSupportEnabled;

        [DisplayName("Android Authentication Strategy")]
        [DynamicHelp("GetAndroidStrategyHelpInfo")]
        [EnumRange("GetAndroidStrategyRange")]
        public AndroidAuthenticationStrategy AndroidAuthenticationStrategySetting =
            AndroidAuthenticationStrategy.None;

        [DisplayName("Android API Key")]
        [DisplayCondition("IsAndroidApiKeyFieldDisplayed")]
        public string CloudServicesApiKey;

        [DisplayName("iOS Authentication Strategy")]
        [DynamicHelp("GetIosStrategyHelpInfo")]
        [EnumRange("GetIosStrategyRange")]
        public IOSAuthenticationStrategy IOSAuthenticationStrategySetting =
            IOSAuthenticationStrategy.None;

        [DisplayName("iOS API Key")]
        [DisplayCondition("IsIosApiKeyFieldDisplayed")]
        public string IosCloudServicesApiKey;

        private const string _projectSettingsPath = "ProjectSettings/ARCoreProjectSettings.json";
        private static ARCoreProjectSettings _instance = null;

        public static ARCoreProjectSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (Application.isEditor)
                    {
                        _instance = new ARCoreProjectSettings();
                        _instance.Load();
                    }
                    else
                    {
                        Debug.LogError("Cannot access ARCoreProjectSettings outside of " +
                            "Unity Editor.");
                    }
                }

                return _instance;
            }
        }

        public void Load()
        {
            Version = GoogleARCore.VersionInfo.Version;
            IsARCoreRequired = true;
            IsInstantPreviewEnabled = true;
            CloudServicesApiKey = string.Empty;
            IosCloudServicesApiKey = string.Empty;
            AndroidAuthenticationStrategySetting = AndroidAuthenticationStrategy.None;
            IOSAuthenticationStrategySetting = IOSAuthenticationStrategy.None;

            string absolutePath = Application.dataPath + "/../" + _projectSettingsPath;
            if (File.Exists(absolutePath))
            {
                ARCoreProjectSettings settings = JsonUtility.FromJson<ARCoreProjectSettings>(
                    File.ReadAllText(absolutePath));
                foreach (FieldInfo fieldInfo in this.GetType().GetFields())
                {
                    fieldInfo.SetValue(this, fieldInfo.GetValue(settings));
                }
            }
            else
            {
                Debug.Log("Cannot find ARCoreProjectSettings at " + absolutePath);
            }

            if (AndroidAuthenticationStrategySetting == AndroidAuthenticationStrategy.None)
            {
                AndroidAuthenticationStrategySetting =
                    string.IsNullOrEmpty(ARCoreProjectSettings.Instance.CloudServicesApiKey) ?
                    AndroidAuthenticationStrategy.Keyless :
                    AndroidAuthenticationStrategy.ApiKey;
            }

            if (IOSAuthenticationStrategySetting == IOSAuthenticationStrategy.None)
            {
                IOSAuthenticationStrategySetting =
                    string.IsNullOrEmpty(ARCoreProjectSettings.Instance.IosCloudServicesApiKey) ?
                    IOSAuthenticationStrategy.AuthenticationToken :
                    IOSAuthenticationStrategy.ApiKey;
            }
            // Upgrades settings from V1.0.0 to V1.1.0.
            if (Version.Equals("V1.0.0"))
            {
                IsInstantPreviewEnabled = true;
                Version = "V1.1.0";
            }

            // Upgrades setting from V1.1.0 and V1.2.0 to V1.3.0.
            // Note: V1.2.0 went out with _versionString = V1.1.0
            if (Version.Equals("V1.1.0"))
            {
                IosCloudServicesApiKey = CloudServicesApiKey;
                Version = "V1.3.0";
            }

            if (!Version.Equals(GoogleARCore.VersionInfo.Version))
            {
                Version = GoogleARCore.VersionInfo.Version;
            }
        }

        public void Save()
        {
            File.WriteAllText(_projectSettingsPath, JsonUtility.ToJson(this));
        }

        public bool IsAndroidApiKeyFieldDisplayed()
        {
            if (AndroidAuthenticationStrategySetting == AndroidAuthenticationStrategy.ApiKey)
            {
                return true;
            }
            else
            {
                CloudServicesApiKey = string.Empty;
                return false;
            }
        }

        public HelpAttribute GetAndroidStrategyHelpInfo()
        {
            if (AndroidAuthenticationStrategySetting == AndroidAuthenticationStrategy.ApiKey)
            {
                return new HelpAttribute(
                    "Cloud Anchor persistence will not be availble on Android when 'API Key'" +
                        " authentication strategy is selected.",
                    HelpAttribute.HelpMessageType.Warning);
            }
            else
            {
                return null;
            }
        }

        public Array GetAndroidStrategyRange()
        {
            return new AndroidAuthenticationStrategy[]
                {
                    AndroidAuthenticationStrategy.ApiKey,
                    AndroidAuthenticationStrategy.Keyless,
                };
        }

        public bool IsIosApiKeyFieldDisplayed()
        {
            if (IOSAuthenticationStrategySetting == IOSAuthenticationStrategy.ApiKey)
            {
                return true;
            }
            else
            {
                IosCloudServicesApiKey = string.Empty;
                return false;
            }
        }

        public Array GetIosStrategyRange()
        {
            return new IOSAuthenticationStrategy[]
                {
                    IOSAuthenticationStrategy.ApiKey,
                    IOSAuthenticationStrategy.AuthenticationToken,
                };
        }

        public HelpAttribute GetIosStrategyHelpInfo()
        {
            if (IOSAuthenticationStrategySetting == IOSAuthenticationStrategy.ApiKey)
            {
                return new HelpAttribute(
                    "Cloud Anchor persistence will not be availble on iOS when 'API Key'" +
                        " authentication strategy is selected.",
                    HelpAttribute.HelpMessageType.Warning);
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// This attribute controls whether to display the field or not. The function name
    /// would be input as the parameter to this attribute. Note, the function must return
    /// the type bool, take no parameters, and be a member of ARCoreProjectSettings.
    /// </summary>
    public class DisplayConditionAttribute : Attribute
    {
        /// <summary>
        /// Reflection function that return the type bool, take no parameters,
        /// and be a member of ARCoreProjectSettings.
        /// </summary>
        public readonly string CheckingFunction;

        /// <summary>
        /// Initializes a new instance of the `DisplayCondition` class.
        /// </summary>
        /// <param name="checkingFunction">Reflection function.</param>
        public DisplayConditionAttribute(string checkingFunction)
        {
            CheckingFunction = checkingFunction;
        }
    }

    /// <summary>
    /// This attribute would affect the field displayed in the ProjectSettingGUI.
    /// It could be used for either a field or an enum. If this attribute isn’t provided,
    /// then the default field name would be the field name.
    /// </summary>
    public class DisplayNameAttribute : Attribute
    {
        /// <summary>
        /// Display string in the GUI.
        /// </summary>
        public readonly string DisplayString;

        /// <summary>
        /// Initializes a new instance of the `DisplayName` class.
        /// </summary>
        /// <param name="displayString">Display string in the GUI.</param>
        public DisplayNameAttribute(string displayString)
        {
            DisplayString = displayString;
        }
    }

    /// <summary>
    /// This attribute is used to generate a HelpBox based on the HelpAttribute
    /// return by the given reflection function. Note, the function must return
    /// the type HelpAttribute, take no parameters, and be a member of ARCoreProjectSettings.
    /// </summary>
    public class DynamicHelpAttribute : Attribute
    {
        /// <summary>
        /// Reflection function that return the type HelpAttribute, take no parameters,
        /// and be a member of ARCoreProjectSettings.
        /// </summary>
        public readonly string CheckingFunction;

        /// <summary>
        /// Initializes a new instance of the `DynamicHelp` class.
        /// </summary>
        /// <param name="checkingFunction">Reflection function.</param>
        public DynamicHelpAttribute(string checkingFunction)
        {
            CheckingFunction = checkingFunction;
        }
    }

    /// <summary>
    /// This attribute is used to control the enum ranges provided for popup.
    /// The function must be a member of ARCoreProjectSettings, return the type
    /// System.Array, and take no parameters.
    /// </summary>
    public class EnumRangeAttribute : Attribute
    {
        /// <summary>
        /// Reflection function that return the type System.Array, take no parameters,
        /// and be a member of ARCoreProjectSettings.
        /// </summary>
        public readonly string CheckingFunction;

        /// <summary>
        /// Initializes a new instance of the `EnumRange` class.
        /// </summary>
        /// <param name="checkingFunction">Reflection function.</param>
        public EnumRangeAttribute(string checkingFunction)
        {
            CheckingFunction = checkingFunction;
        }
    }
}

// Copyright 2015-2019 Mail.Ru Group. All Rights Reserved.

using System.IO;
using UnrealBuildTool;
using Tools.DotNETCommon;

public class AppCenter : ModuleRules
{
    public AppCenter(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicDependencyModuleNames.AddRange(
            new string[]
            {
                "Core",
				// ... add other public dependencies that you statically link with here ...
			}
            );


        PrivateDependencyModuleNames.AddRange(
            new string[]
            {
                "CoreUObject",
                "Engine",
                "Slate",
                "SlateCore",
				// ... add private dependencies that you statically link with here ...	
			}
            );

        // Configure build defines
        bool bEnableAnalytics = false;
        bool bEnableCrashes = false;
        bool bEnableDistribute = false;
        bool bEnablePush = false;

        // Read from config
        ConfigHierarchy Ini = ConfigCache.ReadHierarchy(ConfigHierarchyType.Engine, Target.ProjectFile.Directory, Target.Platform);

        string SettingsSection = "/Script/AppCenter.AppCenterSettings";
        Ini.GetBool(SettingsSection, "bEnableAnalytics", out bEnableAnalytics);
        Ini.GetBool(SettingsSection, "bEnableCrashes", out bEnableCrashes);
        Ini.GetBool(SettingsSection, "bEnableDistribute", out bEnableDistribute);
        Ini.GetBool(SettingsSection, "bEnablePush", out bEnablePush);
        bool bAnyModuleEnabled = (bEnableAnalytics | bEnableCrashes | bEnableDistribute | bEnablePush);

        PublicDefinitions.Add("WITH_APPCENTER=" + (bAnyModuleEnabled ? "1" : "0"));
        PublicDefinitions.Add("WITH_APPCENTER_ANALYTICS=" + (bEnableAnalytics ? "1" : "0"));
        PublicDefinitions.Add("WITH_APPCENTER_CRASHES=" + (bEnableCrashes ? "1" : "0"));
        PublicDefinitions.Add("WITH_APPCENTER_DISTIBUTE=" + (bEnableDistribute ? "1" : "0"));
        PublicDefinitions.Add("WITH_APPCENTER_PUSH=" + (bEnablePush ? "1" : "0"));

        if(bAnyModuleEnabled)
        {
            if (Target.Platform == UnrealTargetPlatform.Android)
            {
                PrivateIncludePaths.Add("AppCenter/Private/Android");

                PublicDependencyModuleNames.AddRange(new string[] { "Launch" });

                string PluginPath = Utils.MakePathRelativeTo(ModuleDirectory, Target.RelativeEnginePath);
                AdditionalPropertiesForReceipt.Add("AndroidPlugin", Path.Combine(PluginPath, "AppCenter_UPL_Android.xml"));

                /** 
                 * Application.mk
                 * 
                 * APP_STL := gnustl_static
                 * APP_ABI := armeabi-v7a, arm64-v8a
                 * APP_CXXFLAGS := -std=c++11 -D__STDC_LIMIT_MACROS
                 * APP_PLATFORM := android-19
                 */
                string ThirdPartyPath = Path.Combine(ModuleDirectory, "..", "ThirdParty");
                PublicIncludePaths.Add(Path.Combine(ThirdPartyPath, "Breakpad", "src"));

                PublicLibraryPaths.Add(Path.Combine(ThirdPartyPath, "Breakpad", "lib", "armeabi-v7a"));
                PublicLibraryPaths.Add(Path.Combine(ThirdPartyPath, "Breakpad", "lib", "arm64-v8a"));
                PublicAdditionalLibraries.Add("breakpad_client");
            }
            else if (Target.Platform == UnrealTargetPlatform.IOS)
            {
                PrivateIncludePaths.Add("AppCenter/Private/IOS");

                string PluginPath = Utils.MakePathRelativeTo(ModuleDirectory, Target.RelativeEnginePath);
                AdditionalPropertiesForReceipt.Add("IOSPlugin", Path.Combine(PluginPath, "AppCenter_UPL_IOS.xml"));

                // The AppCenter.framework is required to start the SDK. If it is not added to the project, 
                // the other modules won't work and your app won't compile.
                PublicAdditionalFrameworks.Add(
                    new Framework(
                        "AppCenter",
                        "../../ThirdParty/AppCenter-SDK-Apple/iOS/AppCenter.embeddedframework.zip"
                    )
                );

                if (bEnableAnalytics)
                {
                    PublicAdditionalFrameworks.Add(
                        new Framework(
                            "AppCenterAnalytics",
                            "../../ThirdParty/AppCenter-SDK-Apple/iOS/AppCenterAnalytics.embeddedframework.zip"
                        )
                    );
                }

                if (bEnableCrashes)
                {
                    PublicAdditionalFrameworks.Add(
                        new Framework(
                            "AppCenterCrashes",
                            "../../ThirdParty/AppCenter-SDK-Apple/iOS/AppCenterCrashes.embeddedframework.zip"
                        )
                    );
                }

                if (bEnableDistribute)
                {
                    PublicAdditionalFrameworks.Add(
                        new Framework(
                            "AppCenterDistribute",
                            "../../ThirdParty/AppCenter-SDK-Apple/iOS/AppCenterDistribute.embeddedframework.zip",
                            "AppCenterDistributeResources.bundle/Info.plist"
                        )
                    );
                }

                if (bEnablePush)
                {
                    PublicAdditionalFrameworks.Add(
                        new Framework(
                            "AppCenterPush",
                            "../../ThirdParty/AppCenter-SDK-Apple/iOS/AppCenterPush.embeddedframework.zip"
                        )
                    );
                }
            }
        }
    }
}

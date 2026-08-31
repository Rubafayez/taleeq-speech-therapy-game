#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

/// <summary>
/// iOS Post-Process Build Script
/// يُصلح كراش bad_weak_ptr الناتج عن iPad Keyboard Assistant Bar + Firebase
/// </summary>
public class iOSPostProcessBuild
{
#if UNITY_IOS
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        FixPlist(buildPath);
        FixXcodeProject(buildPath);
        PatchUnityAppController(buildPath);

        Debug.Log("[iOSPostProcess] All fixes applied successfully.");
    }

    // ====== Plist ======
    static void FixPlist(string buildPath)
    {
        string plistPath = buildPath + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        // تعطيل keyboard suggestions bar (sببب الكراش على iPad)
        plist.root.SetBoolean("UIKeyboardAutocorrectDefault", false);
        plist.WriteToFile(plistPath);
    }

    // ====== Xcode Project ======
    static void FixXcodeProject(string buildPath)
    {
        string projPath = PBXProject.GetPBXProjectPath(buildPath);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string targetGuid = proj.GetUnityMainTargetGuid();
        proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        proj.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");

        proj.WriteToFile(projPath);
    }

    // ====== Patch UnityAppController ======
    // نحقن كود Objective-C يُوقف keyboard assistant bar عند تشغيل التطبيق
    static void PatchUnityAppController(string buildPath)
    {
        string controllerPath = buildPath + "/Classes/UnityAppController.mm";

        if (!File.Exists(controllerPath))
        {
            Debug.LogWarning("[iOSPostProcess] UnityAppController.mm not found at: " + controllerPath);
            return;
        }

        string content = File.ReadAllText(controllerPath);

        // تأكد ما أضفناه قبل كذا (لتجنب التكرار)
        if (content.Contains("DisableKeyboardAssistantBar"))
        {
            Debug.Log("[iOSPostProcess] Keyboard fix already patched.");
            return;
        }

        // الكود اللي سنحقنه - يُوقف iPad keyboard assistant bar
        string injectedCode = @"
// ===== Keyboard Assistant Bar Fix (Auto-injected) =====
static void DisableKeyboardAssistantBar() {
    // يُوقف شريط keyboard assistant على iPad لمنع bad_weak_ptr crash
    [[NSNotificationCenter defaultCenter] addObserverForName:UIKeyboardWillShowNotification
        object:nil queue:[NSOperationQueue mainQueue]
        usingBlock:^(NSNotification *note) {
            dispatch_async(dispatch_get_main_queue(), ^{
                for (UIWindow *w in [UIApplication sharedApplication].windows) {
                    for (UIView *v in w.subviews) {
                        if ([NSStringFromClass([v class]) containsString:@""InputAssistant""] ||
                            [NSStringFromClass([v class]) containsString:@""KeyboardPlaceholder""]) {
                            v.hidden = YES;
                        }
                    }
                }
            });
        }];
}
// ===================================================
";

        // ابحث عن applicationDidFinishLaunching وأضف الكود بعده
        string searchStr = "- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions";

        if (content.Contains(searchStr))
        {
            // أضف الكود المحقون قبل الدالة مباشرةً
            content = content.Replace(searchStr, injectedCode + searchStr);

            // استدعاء دالة الإيقاف في أول السطر من applicationDidFinishLaunching
            string callStr = "DisableKeyboardAssistantBar();";
            string afterLaunch = "UnityInitApplicationGraphics();";

            if (content.Contains(afterLaunch) && !content.Contains(callStr))
            {
                content = content.Replace(afterLaunch, callStr + "\n    " + afterLaunch);
            }

            File.WriteAllText(controllerPath, content);
            Debug.Log("[iOSPostProcess] UnityAppController.mm patched - keyboard fix injected.");
        }
        else
        {
            Debug.LogWarning("[iOSPostProcess] Could not find didFinishLaunchingWithOptions in UnityAppController.mm");
        }
    }
#endif
}
#endif


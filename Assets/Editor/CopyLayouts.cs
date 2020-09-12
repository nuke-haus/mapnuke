using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class CopyLayouts : IPostprocessBuildWithReport
{
    public int callbackOrder => 10;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.StandaloneOSX)
        {
            Debug.Log("This is a standalone macos build");
            MacosPackageAssets(report, "Layouts");
            MacosPackageAssets(report, "Nations");
        }
        else
        {
            Debug.Log("This is a standalone linux/windows build");
            FetchLayout(report);
        }
    }

    private static void FetchLayout(BuildReport report)
    {
        var outPath = Path.GetDirectoryName(report.summary.outputPath);
        var layoutPath = $"{outPath}/MapNuke_Data/Layouts";
        Directory.CreateDirectory(layoutPath);
        foreach (var s in Directory.GetFiles("Assets/Layouts", "*.xml"))
        {
            if (s.EndsWith(".meta")) continue;
            File.Copy(s, $"{layoutPath}/{Path.GetFileName(s)}");
        }
    }


    // This logic fork exist because there does not seem to be an easy way to pull files specifically into the .app 
    private static void MacosPackageAssets(BuildReport report, string asset)
    {
        var outPath = Path.GetDirectoryName(report.summary.outputPath);
        var assetPath = $"{outPath}/MapNuke.app/Contents/{asset}";
        Directory.CreateDirectory(assetPath);
        foreach (var s in Directory.GetFiles($"Assets/{asset}", "*.xml"))
        {
            if (s.EndsWith(".meta")) continue;
            File.Copy(s, $"{assetPath}/{Path.GetFileName(s)}");
        }
    }
}

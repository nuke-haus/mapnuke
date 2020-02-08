using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class CopyLayouts : IPostprocessBuildWithReport
{
    public int callbackOrder => 10;

    public void OnPostprocessBuild(BuildReport report)
    {
        var out_path = Path.GetDirectoryName(report.summary.outputPath);
        var layout_path = out_path + "/MapNuke_Data/Layouts";
        Directory.CreateDirectory(layout_path);
        foreach (var s in Directory.GetFiles("Assets/Layouts", "*.xml"))
        {
            if (s.EndsWith(".meta")) continue;
            File.Copy(s, layout_path + '/' + Path.GetFileName(s));
        }
    }
}

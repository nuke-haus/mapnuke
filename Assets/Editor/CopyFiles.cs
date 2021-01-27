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

    private string[] m_paths = { "Assets/Layouts", 
        "Assets/Nations", 
        "Assets/NameData", 
        "Assets/NameFormats" };

    private string[] m_output_folders = { "Layouts", 
        "Nations", 
        "NameData", 
        "NameFormats" };

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.StandaloneWindows || 
            report.summary.platform == BuildTarget.StandaloneWindows64 ||
            report.summary.platform == BuildTarget.StandaloneLinux64)
        {
            var out_path = Path.GetDirectoryName(report.summary.outputPath);

            for (int i = 0; i < m_paths.Length; i++)
            {
                var path = m_paths[i];
                var out_folder = m_output_folders[i];
                var result_path = out_path + "/MapNuke_Data/" + out_folder;

                Directory.CreateDirectory(result_path);

                foreach (var s in Directory.GetFiles(path, "*.xml"))
                {
                    if (s.EndsWith(".meta"))
                    {
                        continue;
                    }

                    File.Copy(s, result_path + '/' + Path.GetFileName(s));
                }
            }
        }
        else if (report.summary.platform == BuildTarget.StandaloneOSX)
        {
            for (int i = 0; i < m_paths.Length; i++)
            {
                var path = m_paths[i];
                var out_folder = m_output_folders[i];
                var result_path = report.summary.outputPath + "/Contents/Resources/Data/" + out_folder;

                Directory.CreateDirectory(result_path);

                foreach (var s in Directory.GetFiles(path, "*.xml"))
                {
                    if (s.EndsWith(".meta"))
                    {
                        continue;
                    }

                    File.Copy(s, result_path + '/' + Path.GetFileName(s));
                }
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;


// https://docs.unity3d.com/Manual/CommandLineArguments.html
public class JenkinsBuild
{

    static string[] EnabledScenes = FindEnabledEditorScenes();

    public static void BuildMacOS()
    {
        var args = FindArgs();

        string fullPathAndName = args.targetDir + args.appName + ".app";
        BuildProject(EnabledScenes, fullPathAndName, BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildOptions.None);
    }

    public static void BuildWindows64()
    {
        var args = FindArgs();

        string fullPathAndName = args.targetDir + args.appName;

        System.Console.WriteLine("[JenkinsBuildCodeLog] Full path name is " + fullPathAndName);

        BuildProject(EnabledScenes, fullPathAndName, BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    public static void BuildLinux()
    {
        var args = FindArgs();

        string fullPathAndName = args.targetDir + args.appName;
        BuildProject(EnabledScenes, fullPathAndName, BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64, BuildOptions.None);
    }

    public static void BuildWebGL()
    {
        var args = FindArgs();

        string fullPathAndName = args.targetDir + args.appName;
        BuildProject(EnabledScenes, fullPathAndName, BuildTargetGroup.Standalone, BuildTarget.WebGL, BuildOptions.None);
    }

    private static Args FindArgs()
    {
        var returnValue = new Args();

        // find: -executeMethod
        //   +1: JenkinsBuild.BuildMacOS
        //   +2: FindTheGnome
        //   +3: D:\Jenkins\Builds\Find the Gnome\47\output
        string[] args = System.Environment.GetCommandLineArgs();
        var execMethodArgPos = -1;
        bool allArgsFound = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-executeMethod")
            {
                execMethodArgPos = i;
            }
            var realPos = execMethodArgPos == -1 ? -1 : i - execMethodArgPos - 2;
            if (realPos < 0)
                continue;

            if (realPos == 0)
                returnValue.appName = args[i];
            if (realPos == 1)
            {
                returnValue.targetDir = args[i];
                if (!returnValue.targetDir.EndsWith(System.IO.Path.DirectorySeparatorChar + ""))
                    returnValue.targetDir += System.IO.Path.DirectorySeparatorChar;

                allArgsFound = true;
            }
        }

        if (!allArgsFound)
            System.Console.WriteLine("[JenkinsBuildCodeLog] Incorrect Parameters for -executeMethod Format: -executeMethod JenkinsBuild.BuildWindows64 <app name> <output dir>");

        return returnValue;
    }

    private static string[] FindEnabledEditorScenes()
    {

        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.enabled)
                EditorScenes.Add(scene.path);

        return EditorScenes.ToArray();
    }

    private static void BuildProject(string[] scenes, string targetDir, BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, BuildOptions buildOptions)
    {
        System.Console.WriteLine("[JenkinsBuildCodeLog] Building:" + targetDir + " buildTargetGroup:" + buildTargetGroup.ToString() + " buildTarget:" + buildTarget.ToString());

        // https://docs.unity3d.com/ScriptReference/EditorUserBuildSettings.SwitchActiveBuildTarget.html
        bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
        if (switchResult)
        {
            System.Console.WriteLine("[JenkinsBuildCodeLog] Successfully changed Build Target to: " + buildTarget.ToString());
        }
        else
        {
            System.Console.WriteLine("[JenkinsBuildCodeLog] Unable to change Build Target to: " + buildTarget.ToString() + " Exiting...");
            return;
        }

        // https://docs.unity3d.com/ScriptReference/BuildPipeline.BuildPlayer.html
        BuildReport buildReport = BuildPipeline.BuildPlayer(scenes, targetDir, buildTarget, buildOptions);
        BuildSummary buildSummary = buildReport.summary;
        if (buildSummary.result == BuildResult.Succeeded)
        {
            System.Console.WriteLine("[JenkinsBuildCodeLog] Build Success: Time:" + buildSummary.totalTime + " Size:" + buildSummary.totalSize + " bytes");
        }
        else
        {
            System.Console.WriteLine("[JenkinsBuildCodeLog] Build Failed: Time:" + buildSummary.totalTime + " Total Errors:" + buildSummary.totalErrors);
        }
    }

    private class Args
    {
        public string appName = "AppName";
        public string targetDir = "~/Desktop";
    }
}
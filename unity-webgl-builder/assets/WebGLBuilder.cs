using UnityEditor;
using UnityEngine;

public static class WebGLBuilder
{
    [MenuItem("Tools/Switch to WebGL")]
    public static void SwitchToWebGL()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
        {
            Debug.Log("[WebGLBuilder] Already on WebGL platform.");
            return;
        }
        Debug.Log("[WebGLBuilder] Switching to WebGL...");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        Debug.Log("[WebGLBuilder] Switched to WebGL.");
    }

    [MenuItem("Tools/Build WebGL")]
    public static void BuildWebGL()
    {
        string buildPath = "Build/WebGL";
        string[] scenes = { "Assets/Scenes/SampleScene.unity" };

        // Disable Brotli compression for local server compatibility
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;

        Debug.Log($"[WebGLBuilder] Starting WebGL build to '{buildPath}' (compression disabled)...");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[WebGLBuilder] Build succeeded! Size: {report.summary.totalSize / 1024 / 1024} MB, Time: {report.summary.totalTime.TotalSeconds:F1}s");
        }
        else
        {
            Debug.LogError($"[WebGLBuilder] Build failed: {report.summary.result}");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error)
                        Debug.LogError($"  {msg.content}");
                }
            }
        }
    }
}

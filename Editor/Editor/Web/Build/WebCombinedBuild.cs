#if UNITY_WEBGL
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEditor.Build.Reporting;


namespace Lifeverse.AdaptiveEnvironment.Editor {
    public static class WebCombinedBuild {
        //This builds the player twice: a build with desktop-specific texture settings (Web_Build)
        // as well as mobile-specific texture settings (Web_Mobile), and combines the necessary files into one directory (Web_Build)
        [MenuItem("PolytopeSolutions/Web/Build/Dual Build")]
        public static void BuildGame() {
            string dualBuildPath = "Builds/WebBuilds";
            string desktopBuildName = "Web_Desktop";
            string mobileBuildName = "Web_Mobile";

            string desktopPath = Path.Combine(dualBuildPath, desktopBuildName);
            string mobilePath = Path.Combine(dualBuildPath, mobileBuildName);

            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            string[] scenes = new string[sceneCount];
            for (int i = 0; i < sceneCount; i++) {
                string sceneProjectPath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                scenes[i] = sceneProjectPath;// Path.Combine(Path.GetDirectoryName(sceneProjectPath), Path.GetFileNameWithoutExtension(sceneProjectPath));
                Debug.Log($"Web: Using Scene: {sceneProjectPath}");
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.target = BuildTarget.WebGL;
            buildPlayerOptions.options = BuildOptions.None;

            buildPlayerOptions.locationPathName = mobilePath;
            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded) {
                Debug.Log("Web: Mobile Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed) {
                Debug.Log("Web: Mobile Build failed");
                return;
            }

            buildPlayerOptions.locationPathName = desktopPath;
            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;
            report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            summary = report.summary;

            if (summary.result == BuildResult.Succeeded) {
                Debug.Log("Web: Desktop Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed) {
                Debug.Log("Web: Desktop Build failed");
                return;
            }

            // Copy the mobile.data file to the desktop build directory to consolidate them both
            if (File.Exists(Path.Combine(desktopPath, "Build", mobileBuildName + ".data")))
                FileUtil.DeleteFileOrDirectory(Path.Combine(desktopPath, "Build", mobileBuildName + ".data"));
            FileUtil.CopyFileOrDirectory(
                Path.Combine(mobilePath, "Build", mobileBuildName + ".data"), 
                Path.Combine(desktopPath, "Build", mobileBuildName + ".data")
            );

            // Start the build.
            LaunchLastBuild();
        }
        static System.Diagnostics.Process webServerProcess = new System.Diagnostics.Process();
        static int port = 8081;
        [MenuItem("PolytopeSolutions/Web/Build/Launch Last Built")]
        public static void LaunchLastBuild() {
            try {
                if (webServerProcess.HasExited) {
                    StartServer();
                }
            }
            catch (System.Exception exception) {
                EndServer();
                Debug.Log(exception.Message);
                return;
            }
            System.Diagnostics.Process webPageProcess = new System.Diagnostics.Process();
            webPageProcess.StartInfo.FileName = "http://localhost:" + port.ToString();
            webPageProcess.Start();
        }

        [MenuItem("PolytopeSolutions/Web/Build/EndServer")]
        static void EndServer() {
            try {
                webServerProcess.Kill();
            }
            catch (System.Exception exception) {
                Debug.Log(exception.Message);
            }
        }
        static void StartServer() {
#if !UNITY_6000_0_OR_NEWER
            string apppath = Path.GetDirectoryName(EditorApplication.applicationPath);
            string lastBuildPath = EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);
            Debug.Log($"Web: Starting server at port {port}. Build: {lastBuildPath}");

            try {
                webServerProcess.StartInfo.FileName = Path.Combine(apppath, @"Data\PlaybackEngines\WebGLSupport\BuildTools\SimpleWebServer.exe");
                webServerProcess.StartInfo.Arguments = $"\"{lastBuildPath}\" {port}";
                webServerProcess.StartInfo.UseShellExecute = false;
                webServerProcess.Start();
            }
            catch (System.Exception e) {
                Debug.Log(e.Message);
            }
#endif
        }
    }
}
#endif

#if UNITY_IOS

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;

public class PostProcessBuild {

	[PostProcessBuild]
	public static void OnPostProcessBuild(BuildTarget buildTarget, string path) {
		removeBundleFile(path);
	}

	private static void removeBundleFile(string path) {

		string projPath = Path.Combine(path, "Unity-iPhone.xcodeproj/project.pbxproj");
		PBXProject proj = new PBXProject();
		proj.ReadFromString(File.ReadAllText(projPath));
		string target = proj.TargetGuidByName("Unity-iPhone");

		string bundleFile = "Frameworks/Plugins/Mac/GZipUtil.bundle";
		string guid = proj.FindFileGuidByProjectPath(bundleFile);
		proj.RemoveFile(guid);
		string bundleFilePath = Path.Combine(path, bundleFile);
		Directory.Delete(bundleFilePath, true);

		proj.AddFileToBuild(target, proj.AddFile("usr/lib/libz.tbd", "Frameworks/libz.1.2.5.tbd", PBXSourceTree.Sdk));

		File.WriteAllText(projPath, proj.WriteToString());
	}
}

#endif

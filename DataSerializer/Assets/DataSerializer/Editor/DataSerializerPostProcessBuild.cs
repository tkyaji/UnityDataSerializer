#if UNITY_IOS

using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;

public class DataSerializerPostProcessBuild {

	[PostProcessBuild]
	public static void OnPostProcessBuild(BuildTarget buildTarget, string path) {
		string projPath = Path.Combine(path, "Unity-iPhone.xcodeproj/project.pbxproj");
		PBXProject proj = new PBXProject();
		proj.ReadFromString(File.ReadAllText(projPath));
		string target = proj.TargetGuidByName("Unity-iPhone");

		proj.AddFileToBuild(target, proj.AddFile("usr/lib/libz.1.2.5.tbd", "Frameworks/libz.1.2.5.tbd", PBXSourceTree.Sdk));

		File.WriteAllText(projPath, proj.WriteToString());
	}
}

#endif

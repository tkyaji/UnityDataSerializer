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
#if UNITY_2019_3_OR_NEWER
		string target = proj.GetUnityFrameworkTargetGuid();
#else
		string target = proj.TargetGuidByName("Unity-iPhone");
#endif

		proj.AddFileToBuild(target, proj.AddFile("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));

		File.WriteAllText(projPath, proj.WriteToString());
	}
}

#endif

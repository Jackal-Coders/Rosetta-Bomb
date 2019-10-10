using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.AccessControl;
using System;

public class DeploymentEditor : EditorWindow {

	const string DEPLOYKEY = "DEPLOYLOCATION";
	const string LSDLOC = "LAST_SUCCESSFUL_DEPLOY_LOC";

	static string tmp_deployLocation = "";
	static List<string> errorMessages = new List<string>();
	static bool firstGUICheck = true;

	[MenuItem("Mod Deployment/Deploy Location")]
	public static void ShowEditor() {
		GetWindow<DeploymentEditor>();
	}

	[MenuItem("Mod Deployment/Deploy Now")]
	public static void Deploy() {
		if (!EditorPrefs.HasKey(DEPLOYKEY)) {
			Debug.LogError("You must set a deploy location before deployment can be done. Mod Deployment->Deploy Location");
			return;
		}

		// grab the mod and push it to the deploy location
		string modPath = Application.dataPath;
		modPath = modPath.Substring(0, modPath.LastIndexOf('/')) + Path.DirectorySeparatorChar + "build" + Path.DirectorySeparatorChar + "WLCKTANE";

		DirectoryCopy(modPath, EditorPrefs.GetString(DEPLOYKEY) + Path.DirectorySeparatorChar + "WLCKTANE", true);

		Debug.Log("Deployment complete.");
		EditorPrefs.SetString(LSDLOC, EditorPrefs.GetString(DEPLOYKEY));
	}

	/// <summary>
	/// Retrieved from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
	/// </summary>
	/// <param name="sourceDirName"></param>
	/// <param name="destDirName"></param>
	/// <param name="copySubDirs"></param>
	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);

		if (!dir.Exists) {
			throw new DirectoryNotFoundException(
				"Source directory does not exist or could not be found: "
				+ sourceDirName);
		}

		DirectoryInfo[] dirs = dir.GetDirectories();
		// If the destination directory doesn't exist, create it.
		if (!Directory.Exists(destDirName)) {
			Directory.CreateDirectory(destDirName);
		}

		// Get the files in the directory and copy them to the new location.
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files) {
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, true);
		}

		// If copying subdirectories, copy them and their contents to new location.
		if (copySubDirs) {
			foreach (DirectoryInfo subdir in dirs) {
				string temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}
	}

	private void OnGUI() {
		if (firstGUICheck) {
			if (EditorPrefs.HasKey(DEPLOYKEY)) {
				tmp_deployLocation = EditorPrefs.GetString(DEPLOYKEY);
			}
			firstGUICheck = false;
		}

		errorMessages.Clear();

		EditorGUILayout.LabelField("Deploy Location:");

		// store normal gui color
		Color defaultColor = GUI.color;

		// check if text is valid
		bool valid = IsValidDeploymentPath(tmp_deployLocation);

		// set gui color if not valid or if it was successful last time
		GUI.color = (valid ? (EditorPrefs.HasKey(LSDLOC) && EditorPrefs.GetString(LSDLOC) == tmp_deployLocation ? new Color(0f, 1f, 0f, 0.4f) : defaultColor) : new Color(1f, 0f, 0f, 0.4f));

		// get text
		tmp_deployLocation = EditorGUILayout.TextField(tmp_deployLocation);

		// reset color
		GUI.color = defaultColor;

		// sets save button to disabled if not valid
		GUI.enabled = valid;

		if (GUILayout.Button("Save")) {
			EditorPrefs.SetString(DEPLOYKEY, tmp_deployLocation);
			Close();
		}

		if (GUILayout.Button("Deploy")) {
			Deploy();
		}

		GUI.enabled = true;

		foreach (string msg in errorMessages) {
			EditorGUILayout.LabelField(msg);
		}
	}

	private bool IsValidDeploymentPath(string path) {
		if (path == "" || path.Length == 0) {
			errorMessages.Add("Directory path is empty.");
			return false;
		}

		if (!Directory.Exists(path)) {
			errorMessages.Add("Directory does not exist.");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Retrieved from https://social.msdn.microsoft.com/Forums/vstudio/en-US/f81bea37-26f5-44d8-bac4-bc534bbb03b4/c-how-to-check-file-folder-if-writable?forum=netfxbcl
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	private bool HasAccess(string path) {

		DirectoryInfo dirInfo = new DirectoryInfo(path);
		try {
			DirectorySecurity dirAC = dirInfo.GetAccessControl(AccessControlSections.All);
			return true;
		}
		catch (PrivilegeNotHeldException) {
			errorMessages.Add("Privledges not held for this folder.");
			return false;
		} catch (UnauthorizedAccessException) {
			errorMessages.Add("Unauthorized Access detected for this folder.");
			return false;
		}
	}

}

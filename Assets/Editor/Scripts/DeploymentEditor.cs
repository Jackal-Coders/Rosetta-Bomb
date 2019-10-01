using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class DeploymentEditor : EditorWindow {

	static string deployLocation = "C:/Program Files (x86)/Steam/steamapps/common/Keep Talking and Nobody Explodes/mods";

	[MenuItem("Mod Deployment/Deploy Location")]
	public static void ShowEditor() {
		if (!EditorPrefs.HasKey("DEPLOYLOCATION"))
			EditorPrefs.SetString("DEPLOYLOCATION", "C:/Program Files (x86)/Steam/steamapps/common/Keep Talking and Nobody Explodes/mods");

		deployLocation = EditorPrefs.GetString("DEPLOYLOCATION");
		GetWindow<DeploymentEditor>();
	}

	private void OnGUI() {

		EditorGUI.BeginChangeCheck();

		deployLocation = EditorGUILayout.TextField("Deploy Location", deployLocation);

		if (EditorGUI.EndChangeCheck()) {
			EditorPrefs.SetString("DEPLOYLOCATION", deployLocation);
		}

	}


}

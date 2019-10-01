using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class LanguageTest : EditorWindow {

	private static string fileContents;
	private static string formattedContents;



	[MenuItem("Languages/Test")]
	public static void ShowWindow() {
		GetWindow<LanguageTest>().Show();
	}

	private void LoadTextAsset() {
		string path = EditorUtility.OpenFilePanel("Open Language Teset", "Assets", "txt");
		if (path != "") {
			fileContents = File.ReadAllText(path);
			if (LanguageBuilder.LoadedLanguage() != null) {
				formattedContents = LanguageBuilder.LoadedLanguage().Format(fileContents);
			}
		}		
	}

	private void OnGUI() {

		if (GUILayout.Button("Open") || fileContents == null) {
			LoadTextAsset();
		}

		EditorGUILayout.LabelField(fileContents);

		EditorGUILayout.LabelField("Character count: " + fileContents.Length);

		if (LanguageBuilder.LoadedLanguage() == null)
			return;

		EditorGUILayout.LabelField("Loaded language: " + LanguageBuilder.LoadedLanguage().Name);

		if (formattedContents == null)
			formattedContents = LanguageBuilder.LoadedLanguage().Format(fileContents);

		EditorGUILayout.LabelField("Processed:" + formattedContents);


		foreach (char c in fileContents) {
			EditorGUILayout.LabelField("0x" + ((ulong)c).ToString("X") + " : " + c);
		}

		EditorGUILayout.Space();

		foreach (char c in formattedContents) {
			EditorGUILayout.LabelField("0x" + ((ulong)c).ToString("X") + " : " + c);
		}

	}
}

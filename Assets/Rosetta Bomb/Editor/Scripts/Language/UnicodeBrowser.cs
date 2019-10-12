using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class UnicodeBrowser : EditorWindow {
	
	private static GUIStyle characterStyle;
	private static float idealHeight = 70;
	private static float idealWidth = 70;

	private int gridX = 5;
	private int gridY = 10;

	private string searchTerm = "0x0000";

	private ulong startingUnicode = 0;
	private static ulong selectedChar = 0;

	private bool useSearchTerm = false;

	[MenuItem("Languages/Unicode Browser")]
	public static void ShowWindow() {
		GetWindow<UnicodeBrowser>().Show();
	}

	public static void ShowWindow(ulong startingCode) {
		UnicodeBrowser window = GetWindow<UnicodeBrowser>();
		window.SetStartingUnicode(startingCode);
		window.Show();
	}

	public static ulong GetSelectedChar() {
		return selectedChar;
	}

	public void SetStartingUnicode(ulong startingCode) {
		startingUnicode = startingCode - (startingCode % (ulong)gridX);
	}

	private void OnGUI() {
		titleContent = new GUIContent("Unicode Browswer");
		characterStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
		characterStyle.richText = true;

		// detect scroll events
		if (Event.current.isScrollWheel) {

			if (Event.current.delta.y < 0) {
				// scrolled up
				SetStartingUnicode(startingUnicode - (ulong)(gridX));
			}

			if (Event.current.delta.y > 0) {
				// scrolled down
				SetStartingUnicode(startingUnicode + (ulong)(gridX));
			}

			useSearchTerm = false;

			Repaint();

		}

		// Search Toolbar
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		EditorGUI.BeginChangeCheck();
		searchTerm = EditorGUILayout.TextField(searchTerm, GUI.skin.GetStyle("SearchTextField"), GUILayout.ExpandWidth(true));
		if (EditorGUI.EndChangeCheck()) {
			useSearchTerm = true;
		}
		EditorGUILayout.EndHorizontal();

		if (useSearchTerm && searchTerm != "") {
			if (searchTerm.StartsWith("0x") && searchTerm.Length >= 3) {
				// search by unicode value
				try {
					ulong searchCode = Convert.ToUInt64(searchTerm, 16);

					selectedChar = searchCode;
					SetStartingUnicode(selectedChar);
				}
				catch (OverflowException ofe) {
					// do nothing, just not formatted right
				}
				catch (FormatException fe) {
					// do nothing, same as above
				}
			}

			if (searchTerm.Length == 1) {
				// search for this character
				selectedChar = searchTerm[0];
				SetStartingUnicode(selectedChar);
			}
		}

		GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);

		EditorGUILayout.BeginVertical(GUI.skin.GetStyle("CN Box"));

		// decide how many to render in X and Y axis
		gridX = (int)(position.width / idealWidth);
		gridY = (int)(position.height / idealHeight);

		bool stopDrawing = false;

		Color defaultColor = GUI.color;

		GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);

		for (int i = 0; i < gridY && !stopDrawing; i++) {
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < gridX && !stopDrawing; j++) {
				ulong unicode = (ulong)(i * gridX + j) + startingUnicode;

				if (selectedChar == unicode) {
					GUI.color = Color.cyan;
				}

				if (GUILayout.Button("0x" + unicode.ToString("X") + "\n<color=#0000a0ff><size=30>" + (char)unicode + "</size></color>", characterStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
					GUI.FocusControl(null);
					selectedChar = unicode;
					useSearchTerm = false;

					if (Event.current.button == 1) {
						// show generic menu when right clicked
						GenericMenu menu = new GenericMenu();
						menu.AddItem(new GUIContent("Copy character"), false, delegate () {
							EditorGUIUtility.systemCopyBuffer = (char)selectedChar + "";
						});
						menu.AddItem(new GUIContent("Copy Unicode value"), false, delegate () {
							EditorGUIUtility.systemCopyBuffer = "0x" + selectedChar.ToString("X");
						});


						menu.ShowAsContext();
					}
				}

				GUI.color = defaultColor;
			}
			EditorGUILayout.EndHorizontal();

		}
		
		EditorGUILayout.EndVertical();
	}

}

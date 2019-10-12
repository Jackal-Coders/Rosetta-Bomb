using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(ROSApp), true)]
public class ROSAppEditor : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		ROSApp app = (ROSApp)target;

		// just assist in finding references if we can
		if (GUILayout.Button("Find References")) {
			Transform windowControl = app.transform.Find("Window Control Panel");

			if (windowControl == null) {
				Debug.LogError("Could not find the Window Control Panel");
			} else {
				Transform appIcon = windowControl.Find("App Icon");
				if (appIcon == null) {
					Debug.LogError("Could not find the App Icon in the control panel.");
				} else
					app.appIconImage = appIcon.GetComponent<Image>();

				Transform appTitle = windowControl.Find("App Title");
				if (appTitle == null) {
					Debug.LogError("Could not find the App Title in the control panel.");
				} else
					app.appTitleText = appTitle.GetComponent<Text>();

				Transform exitImage = windowControl.Find("App Close Icon");
				if (exitImage == null) {
					Debug.LogError("Could not find the App Close Icon in the control panel.");
				}
			}
			Transform resize = app.transform.Find("Resize Image");
			if (resize == null) {
				Debug.LogError("Could not find the Resize Image.");
			} else {
				app.resizeImage = resize.GetComponent<Image>();
			}

			Transform scrollRect = app.transform.Find("Content View");
			if (scrollRect == null) {
				Debug.LogError("Could not find the Content View scroll rect.");
			} else {
				app.scrollRect = scrollRect.GetComponent<ScrollRect>();

				Transform content = scrollRect.GetChild(0).GetChild(0);
				if (content.name != "Content") {
					Debug.LogError("Could not find the Content under the Content View->Viewport");
				} else {
					app.scrollContentPane = (RectTransform)content;
				}
			}


		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Transform))]
public class CustomTransformEditor : Editor {

	static float labelWidth = 75f;
	static int roundPlaces = 3;

	public enum RelativeState {
		Relative,
		Global
	}

	public RelativeState positionState, rotationState, scaleState = RelativeState.Relative;

	public override void OnInspectorGUI() {
		Transform transform = (Transform)target;

		// Make this Undo-able
		Undo.RecordObject(transform, "Transform Custom Update");

		EditorGUILayout.BeginVertical();
		// position
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Position", GUILayout.MaxWidth(labelWidth));
		positionState = (RelativeState)EditorGUILayout.EnumPopup(positionState);
		if (positionState == RelativeState.Global)
			transform.position = EditorGUILayout.Vector3Field("", CMath.RoundToPoint(transform.position, roundPlaces));
		else
			transform.localPosition = EditorGUILayout.Vector3Field("", CMath.RoundToPoint(transform.localPosition, roundPlaces));
		EditorGUILayout.EndHorizontal();
		// rotation
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Rotation", GUILayout.MaxWidth(labelWidth));
		rotationState = (RelativeState)EditorGUILayout.EnumPopup(rotationState);
		if (rotationState == RelativeState.Global)
			transform.eulerAngles = EditorGUILayout.Vector3Field("", CMath.RoundToPoint(transform.eulerAngles, roundPlaces));
		else
			transform.localEulerAngles = EditorGUILayout.Vector3Field("", CMath.RoundToPoint(transform.localEulerAngles, roundPlaces));
		EditorGUILayout.EndHorizontal();
		// scale
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Scale", GUILayout.MaxWidth(labelWidth));
		scaleState = (RelativeState)EditorGUILayout.EnumPopup(scaleState);
		if (scaleState == RelativeState.Global) {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Vector3Field("", CMath.RoundToPoint(transform.lossyScale, roundPlaces)); // can't set global scale
			EditorGUI.EndDisabledGroup();
		} else
			transform.localScale = EditorGUILayout.Vector3Field("", CMath.RoundToPoint(transform.localScale, roundPlaces));
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class DynamicGUISize : MonoBehaviour {

	/// <summary>
	/// Can be unassigned. Uses direct parent by default.
	/// </summary>
	public RectTransform parent;
	public bool adjustWidth = true;
	public bool adjustHeight = true;
	[Range(0f, 1f)]
	public float widthPercent = 0.5f;
	[Range(0f, 1f)]
	public float heightPercent = 0.5f;

	/// <summary>
	/// Used to include a margin on the left/right (not scaled)
	/// </summary>
	public float widthOffset = 0f;
	/// <summary>
	/// Used to include a margin on the top/bottom (not scaled)
	/// </summary>
	public float heightOffset = 0f;

	public bool fixLocalScale = false;

	private RectTransform selfTrans;

	// Use this for initialization
	void Awake () {
		if (!parent && transform.parent.GetComponent<RectTransform>() != null) {
			parent = (RectTransform)transform.parent;
		}
		if (!selfTrans)
			selfTrans = (RectTransform)transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (!parent) return;

		float scaleX = widthPercent;
		float scaleY = heightPercent;

		if (fixLocalScale) {
			scaleX *= (1f / selfTrans.localScale.x);
			scaleY *= (1f / selfTrans.localScale.y);
		}

		selfTrans.sizeDelta = new Vector2((adjustWidth ? (parent.rect.width + widthOffset) * scaleX : selfTrans.sizeDelta.x), (adjustHeight ? (parent.rect.height + heightOffset) * scaleY : selfTrans.sizeDelta.y));
	}
}

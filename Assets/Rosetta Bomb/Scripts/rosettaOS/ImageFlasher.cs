using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class ImageFlasher : MonoBehaviour {

	public float flashSpeed = 5f;

	private CanvasRenderer img;

	private void Awake() {
		img = GetComponent<CanvasRenderer>();
	}

	// Update is called once per frame
	void Update () {
		img.SetAlpha((Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f);
	}
}

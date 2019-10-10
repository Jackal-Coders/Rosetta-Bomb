using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class KOSButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
	
	public Color baseColor;
	public float highlightMult = 1.5f;

	public UnityAction clickAction;

	private Image image;

	private void Awake() {
		image = GetComponent<Image>();
	}

	private void Update() {
		if (Application.isEditor && !Application.isPlaying) {
			image.color = baseColor;
		}
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (clickAction != null)
			clickAction();
		OnPointerExit(eventData);
	}

	public void OnPointerEnter(PointerEventData eventData) {
		image.color = baseColor * highlightMult;
	}

	public void OnPointerExit(PointerEventData eventData) {
		image.color = baseColor;
	}
}

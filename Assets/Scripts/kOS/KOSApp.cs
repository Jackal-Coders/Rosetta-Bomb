using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class KOSApp : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public string appName;
	public Sprite appIcon;
	public Vector2 position;
	public Vector2 size;

	protected KOSDesktop desktop;

	private Vector2 oldPosition;
	private Vector2 oldSize;

	private bool maximized = false;

	public void Open(KOSDesktop desktop) {
		this.desktop = desktop;
		this.gameObject.SetActive(true);
	}

	public void Resize(float width, float height) {
		size.x = Mathf.Clamp(width, 60f, desktop.size.x - position.x);
		size.y = Mathf.Clamp(height, 60f, desktop.size.y - position.y);
	}

	public void Resize(Vector2 size) {
		this.size = new Vector2(Mathf.Clamp(size.x, 60f, desktop.size.x - position.x), Mathf.Clamp(size.y, 60f, desktop.size.y));
	}

	public void Move(float x, float y) {
		position.x = Mathf.Clamp(x, 0, desktop.size.x - size.x);
		position.y = Mathf.Clamp(y, 0, desktop.size.y - size.y);
	}

	public void Move(Vector2 pos) {
		position = new Vector2(Mathf.Clamp(pos.x, 0, desktop.size.x - size.x), Mathf.Clamp(pos.y, 0, desktop.size.y - size.y));
	}

	public void Maximize() {
		oldPosition = new Vector2(position.x, position.y);
		oldSize = new Vector2(size.x, size.y);
		maximized = true;
		Move(0f, 0f);
		Resize(desktop.size.x, desktop.size.y);
	}

	public bool IsMaximized() {
		return maximized;
	}

	public void Close() {
		this.gameObject.SetActive(false);
	}

	public void OnBeginDrag(PointerEventData data) {
		if (maximized) {
			maximized = false;
			Move(oldPosition);
			Resize(oldSize);
		}
	}

	public void OnDrag(PointerEventData data) {
		if (!data.dragging)
			return;
		Move(position + desktop.dragSpeed * data.delta);
	}

	public void OnEndDrag(PointerEventData data) {

	}

	public void OnExitClicked(BaseEventData data) {

	}
}

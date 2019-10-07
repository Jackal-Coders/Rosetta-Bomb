using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class ResponseEvent : UnityEvent<Response> {
}

public abstract class KOSApp : MonoBehaviour {

	public string appName;
	public Sprite appIcon;

	public bool isDialogue = false;
	public bool canMove = true;
	public bool canResize = true;

	private Vector2 position;
	private Vector2 size;

	[Header("GUI References")]
	public RectTransform scrollContentPane;
	public ScrollRect scrollRect;
	public Image resizeImage;
	public Image exitImage;

	protected KOSDesktop desktop;

	private Vector2 oldPosition;
	private Vector2 oldSize;
	private bool maximized = false;
	private RectTransform selfTrans;

	[Header("Animations")]
	private bool useAnimations = false;
	public float animationSpeed = 10f;

	protected ResponseEvent responseEvent = new ResponseEvent();

	public void RegisterToDesktop(KOSDesktop desktop) {
		this.desktop = desktop;
		this.desktop.RegisterApp(this);
	}

	private void Awake() {
		selfTrans = (RectTransform)transform;
		SetCanMove(canMove); // make sure any GUI elements reflect our ability status
		SetCanResize(canResize);
		if (isDialogue) {
			exitImage.enabled = false;
		}
		
	}

	/** Apps will override these methods */
	public virtual void AppStart() {
	}

	public virtual void AppOpen() {
	}

	public virtual void AppUpdate() {
	}

	public virtual void AppClose() {
	}

	/* -- End Override Methods -- */

	/* These are used to create response forms (dialogs) */
	public void AddResponseAction(UnityAction<Response> action) {
		responseEvent.AddListener(action);
	}

	public void OnResponseSend() {
		// sends out the response object and event
		// TODO Load app data as response to requesting apps
		Response r = new Response(Response.Type.CANCEL);
		responseEvent.Invoke(r);
		responseEvent.RemoveAllListeners();
	}

	public bool IsDialogue() {
		return isDialogue;
	}

	public void SetCanMove(bool canMove) {
		this.canMove = canMove;
	}

	public bool CanMove() {
		return canMove;
	}

	public void SetCanResize(bool canResize) {
		this.canResize = canResize;
		if (!this.canResize) 
			resizeImage.enabled = false;
	}

	public bool CanResize() {
		return canResize;
	}

	private void Update() {
		if (useAnimations) {
			if (canMove)
				selfTrans.anchoredPosition = Vector2.Lerp(selfTrans.anchoredPosition, position, Time.deltaTime * animationSpeed);
			if (canResize)
			selfTrans.sizeDelta = Vector2.Lerp(selfTrans.sizeDelta, size, Time.deltaTime * animationSpeed);
		}


		AppUpdate();
	}

	public bool IsOpen() {
		return gameObject.activeSelf;
	}

	public void Open() {
		gameObject.SetActive(true);
		// update size and position
		position = selfTrans.anchoredPosition;
		size = selfTrans.rect.size;

		Resize(size);
		Move(position);
		AppOpen();
	}

	public void Resize(float width, float height) {
		size.x = Mathf.Clamp(width, 120f, desktop.size.x - position.x);
		size.y = Mathf.Clamp(height, 60f, position.y);
		if (!useAnimations && canResize)
			selfTrans.sizeDelta = size;
	}

	public void Resize(Vector2 size) {
		Resize(size.x, size.y);
	}

	public void Move(float x, float y) {
		position.x = Mathf.Clamp(x, 0, desktop.size.x - size.x);
		position.y = Mathf.Clamp(y, size.y, desktop.size.y);
		if (!useAnimations && canMove)
			selfTrans.anchoredPosition = position;
	}

	public void Move(Vector2 pos) {
		Move(pos.x, pos.y);
	}

	public void Maximize() {
		if (!canMove || !canResize)
			return;
		oldPosition = selfTrans.anchoredPosition;
		oldSize = new Vector2(size.x, size.y);
		maximized = true;
		Move(0f, desktop.size.y); // move to top left
		Resize(desktop.size.x, desktop.size.y);
	}

	public void Restore() {
		if (!maximized)
			return;
		maximized = false;
		Resize(oldSize);
		Move(oldPosition);
	}

	public bool IsMaximized() {
		return maximized;
	}

	public void Close() {
		AppClose();

		gameObject.SetActive(false);
	}

	public void OnHeaderClick(BaseEventData data) {
		PointerEventData pData = (PointerEventData)data;
		// bring this window to front
		transform.SetAsLastSibling();

		if (pData.clickCount == 2) {
			useAnimations = true;
			if (!maximized)
				Maximize();
			else
				Restore();

		}
	}

	public void OnBeginDrag(BaseEventData data) {
		useAnimations = false;
		if (maximized) {
			Restore();
		}
	}

	public void OnDrag(BaseEventData data) {
		PointerEventData pData = (PointerEventData)data;
		if (!pData.dragging)
			return;
		Move(position + desktop.dragSpeed * pData.delta);
	}

	public void OnEndDrag(BaseEventData data) {
		PointerEventData pData = (PointerEventData)data;
	}

	public void OnResizeDrag(BaseEventData data) {
		PointerEventData pData = (PointerEventData)data;
		if (!pData.dragging)
			return;
		if (maximized)
			maximized = false;
		useAnimations = false;
		// need to reverse y mouse delta
		pData.delta = new Vector2(pData.delta.x, -pData.delta.y);

		Resize(size + desktop.dragSpeed * pData.delta);
	}

	public void OnExitClicked(BaseEventData data) {
		Close();
	}

	public override string ToString() {
		return name;
	}
}

public class Response {

	public Type responseType;

	public Response(Type rType) {
		responseType = rType;
	}

	public enum Type {
		ACCEPT, CANCEL
	}

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class ResponseEvent : UnityEvent<Response> {
}

public abstract class ROSApp : MonoBehaviour {

	public string appName;
	public Sprite appIcon;

	public bool isDialogue = false;
	public bool canMove = true;
	public bool canResize = true;
	public bool isSingleton = false;

	private Vector2 position;
	private Vector2 size;

	[Header("GUI References")]
	public Text appTitleText;
	public Image appIconImage;
	public RectTransform scrollContentPane;
	public ScrollRect scrollRect;
	public Image resizeImage;
	public Image exitImage;

	private Vector2 oldPosition;
	private Vector2 oldSize;
	private bool maximized = false;
	private RectTransform selfTrans;

	[Header("Animations")]
	private bool useAnimations = false;
	public float animationSpeed = 10f;

	protected ResponseEvent responseEvent = new ResponseEvent();

	private void Awake() {
		selfTrans = (RectTransform)transform;
		appTitleText.text = appName;
		appIconImage.sprite = appIcon;
		SetCanMove(canMove); // make sure any GUI elements reflect our ability status
		SetCanResize(canResize);
		if (isDialogue) {
			exitImage.enabled = false;
		}

		// set default size and position
		size = selfTrans.sizeDelta;
		position = selfTrans.anchoredPosition;
	}

	/** Apps will override these methods */
	public virtual void AppInit() {
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

	public void OnSuccessResponseClick() {
		OnResponseSend(Response.Type.ACCEPT);
	}

	public void OnCancelResponseClick() {
		OnResponseSend(Response.Type.CANCEL);
	}


	public void OnResponseSend(Response.Type type) {
		// sends out the response object and event
		Response r = new Response(type);
		responseEvent.Invoke(r);
		responseEvent.RemoveAllListeners();
		Close();
	}

	public bool IsDialogue() {
		return isDialogue;
	}

	public void SetTitle(string title) {
		appTitleText.text = title;
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

	public bool IsSingleton() {
		return isSingleton;
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
		if (!IsDialogue()) {
			transform.SetAsLastSibling();
			Move(selfTrans.anchoredPosition);
		} else {
			Center();
		}
		Resize(selfTrans.sizeDelta);
		AppOpen();
	}

	public void Hide() {
		gameObject.SetActive(false);
	}

	/// <summary>
	/// This doesn't check for can move since it isn't a natural event and is called from code.
	/// </summary>
	public void Center() {
		position.x = (ROSDesktop.Desktop.size.x - size.x) / 2f;
		position.y = (ROSDesktop.Desktop.size.y + size.y) / 2f; // 0 is bottom, height is top
		selfTrans.anchoredPosition = position;
	}

	public void Resize(float width, float height) {
		size.x = Mathf.Clamp(width, 120f, ROSDesktop.Desktop.size.x - position.x);
		size.y = Mathf.Clamp(height, 60f, position.y);
		if (!useAnimations && canResize)
			selfTrans.sizeDelta = size;
	}

	public void Resize(Vector2 size) {
		Resize(size.x, size.y);
	}

	public void Move(float x, float y) {
		position.x = Mathf.Clamp(x, 0, ROSDesktop.Desktop.size.x - size.x);
		position.y = Mathf.Clamp(y, size.y, ROSDesktop.Desktop.size.y);
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
		Move(0f, ROSDesktop.Desktop.size.y); // move to top left
		Resize(ROSDesktop.Desktop.size.x, ROSDesktop.Desktop.size.y);
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

		ROSDesktop.Desktop.AppClosed(this);

		if (!IsSingleton()) {
			Destroy(gameObject);
		}
	}

	public void OnHeaderClick(BaseEventData data) {
		PointerEventData pData = (PointerEventData)data;
		// bring this window to front (if not dialogue)
		if (!IsDialogue())
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
		Move(position + ROSDesktop.Desktop.dragSpeed * pData.delta);
	}

	public void OnEndDrag(BaseEventData data) {
		
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

		Resize(size + ROSDesktop.Desktop.dragSpeed * pData.delta);
	}

	public void OnExitClicked(BaseEventData data) {
		Close();
	}

	public override string ToString() {
		return appName;
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
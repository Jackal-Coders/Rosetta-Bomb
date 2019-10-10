using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

public class KOSDesktop : MonoBehaviour {

	[Header("OS GUI References")]
	public RectTransform appDisplayArea;
	public Text timeText;
	public Sprite defaultAppIcon;
	public Text messageBarText;
	public Transform appIconParent;
	public GameObject appIconTemplate;
	public Color appIconBackground = new Color(0f, 0.55f, 0.95f, 0.4f);
	public CanvasGroup lockScreenRend;
	public float lockScreenFadeSpeed = 2f;
	public Text lockScreenTimeText;
	public Image lockNotificationImage;
	public Sprite warningIcon;
	public Sprite errorIcon;
	public Color warningColor;
	public Color errorColor;

	[Header("KTANE References")]
	public KMSelectable selectable;
	public KMHoldable holdable;

	internal Vector2 size = new Vector2(322f, 210f);
	[Header("Desktop Settings")]
	public float dragSpeed = 1f;
	public Font textFont;
	
	public Dictionary<int, KOSApp> apps = new Dictionary<int, KOSApp>();

	private float lastFrameTime;

	private bool isPickedUp = false;

	private void Awake() {
		// Add pick up / put down detection
		selectable.OnFocus += OnPickedUp;
		selectable.OnDefocus += OnPutDown;

		Application.logMessageReceivedThreaded += HandleLog;

		lastFrameTime = Time.time;
		size = appDisplayArea.rect.size;

		// try loading all the apps under appDisplayArea (this ensures apps like the console are setup before any log messages go that way)
		for (int c = 0; c < appDisplayArea.childCount; c++) {
			KOSApp app = appDisplayArea.GetChild(c).GetComponent<KOSApp>();
			if (app != null) {
				app.RegisterToDesktop(this);
			}
			// make sure each object is disabled by default
			app.gameObject.SetActive(false);
		}

		// load all app icons
		foreach(int id in apps.Keys) {
			KOSApp app = apps[id];

			GameObject iconObj = Instantiate(appIconTemplate, appIconParent, false);
			iconObj.name = "App Icon:" + id;

			iconObj.transform.GetChild(0).GetComponent<Image>().color = Color.clear;

			iconObj.transform.GetChild(1).GetComponent<Image>().sprite = (app.appIcon != null ? app.appIcon : defaultAppIcon);
		
			iconObj.transform.GetChild(2).GetComponent<Text>().text = app.appName;

			iconObj.SetActive(true);
		}

		UpdateTimeTexts();
	}

	/// <summary>
	/// Return whether it should try to drill into children (we need it to, there is a dummy child to make the tablet stop flashing)
	/// </summary>
	/// <returns></returns>
	private void OnPickedUp() {
		isPickedUp = true;
	}

	/// <summary>
	/// Returns whether it should keep the selectable up (I think)
	/// </summary>
	/// <returns></returns>
	private void OnPutDown() {
		isPickedUp = false;
	}

	/// <summary>
	/// This is required to not have double logs when entering the game scene (and back to menu too)
	/// </summary>
	private void OnDestroy() {
		Application.logMessageReceivedThreaded -= HandleLog;
	}

	private void Start() {
		Debug.Log(SceneManager.GetActiveScene().name);
	}

	public bool IsPickedUp() {
		return isPickedUp;
	}

	public bool TrySafeRegisterApp(KOSApp app) {
		if (apps.ContainsValue(app))
			return false;
		apps.Add(apps.Count, app);
		app.AppStart(); // Started by OS
		Debug.Log("App registered: " + app);
		return true;
	}

	public void RegisterApp(KOSApp app) {
		if (!TrySafeRegisterApp(app)) {
			Debug.LogError("App already registered: " + app);
		}
	}

	public void HandleLog(string message, string stack, LogType type) {
		KOSConsole.Print(message + "\n" + stack, type);
	}

	public void UpdateMessageBar() {
		KOSConsole.Message msg = KOSConsole.GetLastMessage();
		messageBarText.text = msg.GetTitle();
		messageBarText.color = msg.GetColor() * 2f;

		if (msg.GetMessageType() == LogType.Warning) {
			lockNotificationImage.enabled = true;
			lockNotificationImage.sprite = warningIcon;
			lockNotificationImage.color = warningColor;
		}
		if (msg.GetMessageType() == LogType.Error) {
			lockNotificationImage.enabled = true;
			lockNotificationImage.sprite = errorIcon;
			lockNotificationImage.color = errorColor;
		}
	}

	public void UpdateTimeTexts() {
		string timeStr = DateTime.Now.ToShortTimeString();
		timeText.text = timeStr;
		lockScreenTimeText.text = timeStr; 
	}

	private void Update() {
		// update time text every new second
		if ((int)Time.time != (int)lastFrameTime) {
			UpdateTimeTexts();
		}

		// show/hide lockscreen as necessary
		lockScreenRend.alpha = Mathf.Lerp(lockScreenRend.alpha, (IsPickedUp() || Application.isEditor ? 0f : 1f), Time.deltaTime * lockScreenFadeSpeed);
		if (lockScreenRend.alpha < 0.01f) {
			lockScreenRend.alpha = 0f;
			lockScreenRend.blocksRaycasts = false;
		} else {
			lockScreenRend.blocksRaycasts = true;
		}
	}

	public void AppHoverEnter(Image selectionBackground) {
		selectionBackground.color = appIconBackground;
	}

	public void AppHoverExit(Image selectionBackground) {
		selectionBackground.color = Color.clear;
	}

	public void AppClicked(GameObject appIconParent) {
		int id = int.Parse(appIconParent.name.Split(':')[1]);
		if (!apps[id].IsOpen())
			apps[id].Open();
	}


}

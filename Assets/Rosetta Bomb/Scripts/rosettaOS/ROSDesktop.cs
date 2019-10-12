using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

public class ROSDesktop : MonoBehaviour {

	[Header("OS GUI References")]
	public RectTransform appBuildArea;
	public RectTransform appDisplayArea;
	public RectTransform dialogueBuildArea;
	public CanvasGroup blockingArea;
	public RectTransform dialogueDisplayArea;
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

	[Header("Apps and Dialogues")]
	public List<ROSApp> appsToLoad = new List<ROSApp>();
	public List<ROSApp> dialoguesToLoad = new List<ROSApp>();

	public Dictionary<GameObject, ROSApp> appIconLookup = new Dictionary<GameObject, ROSApp>();
	public Dictionary<Type, ROSApp> apps = new Dictionary<Type, ROSApp>();
	public Dictionary<Type, ROSApp> dialogues = new Dictionary<Type, ROSApp>();

	private Dictionary<Type, ROSApp> openSingletonApps = new Dictionary<Type, ROSApp>();
	private Stack<ROSApp> openDialogueStack = new Stack<ROSApp>();
	private float lastFrameTime;
	private bool isPickedUp = false;

	public static ROSDesktop Desktop;

	private void Awake() {
		Desktop = this;

		// Add pick up / put down detection
		selectable.OnInteract += OnInteract;
		//selectable.OnFocus += OnPickedUp; // this seems broken for now
		selectable.OnDefocus += OnPutDown;

		Application.logMessageReceivedThreaded += HandleLog;

		lastFrameTime = Time.time;
		size = appDisplayArea.rect.size;

		// try loading all the apps under appDisplayArea (this ensures apps like the console are setup before any log messages go that way)
		foreach (ROSApp app in appsToLoad) {
			if (app.IsDialogue()) {
				Debug.LogError("ROSApp marked as dialogue but put in appsToLoad: " + app);
				continue;
			}
			RegisterApp(app);
		}
		appsToLoad.Clear();
		foreach (ROSApp dia in dialoguesToLoad) {
			if (!dia.IsDialogue()) {
				Debug.LogError("ROSApp not marked as dialogue but put in dialoguesToLoad: " + dia);
				continue;
			}
			if (dia.IsSingleton()) {
				Debug.LogError("ROSApp marked as dialogue is marked singleton, this isn't allowed: " + dia);
				continue;
			}
			RegisterApp(dia);
		}
		dialoguesToLoad.Clear();

		// load all app icons
		foreach (Type type in apps.Keys) {
			ROSApp app = apps[type];

			GameObject iconObj = Instantiate(appIconTemplate, appIconParent, false);
			iconObj.name = "App Icon:" + type;

			iconObj.transform.GetChild(0).GetComponent<Image>().color = Color.clear;

			iconObj.transform.GetChild(1).GetComponent<Image>().sprite = (app.appIcon != null ? app.appIcon : defaultAppIcon);
		
			iconObj.transform.GetChild(2).GetComponent<Text>().text = app.appName;

			iconObj.SetActive(true);

			appIconLookup.Add(iconObj, app);
		}

		dialogueBuildArea.gameObject.SetActive(false);
		appBuildArea.gameObject.SetActive(false);

		UpdateTimeTexts();
	}

	private void Start() {
		ShowDialogue<ROSMessageDialogue>().SetContent("Set Working Directory", "A working directory needs to be set so that config and language files can be stored where you can find them later.");
	}


	public static T LaunchApp<T>() where T : ROSApp {
		return Desktop.ShowApp<T>();
	}

	public ROSApp LaunchApp(ROSApp app) {
		return Desktop.ShowApp(app);
	}

	public T ShowApp<T>() where T : ROSApp {
		return (T)ShowApp(apps[typeof(T)]);
	}

	public ROSApp ShowApp(ROSApp app) {
		if (app.IsSingleton()) {
			if (openSingletonApps.ContainsKey(app.GetType())) {
				ROSApp singletonApp = openSingletonApps[app.GetType()];
				singletonApp.Open();
				return singletonApp;
			} else {
				// move to display area (to make sure we don't duplicate
				app.transform.SetParent(appDisplayArea, true);
				app.Open();
				openSingletonApps.Add(app.GetType(), app);
				return app;
			}
		}
		
		// duplicate gameobject and put under working area
		GameObject appObj = Instantiate(app.gameObject, appDisplayArea);
		ROSApp newApp = appObj.GetComponent<ROSApp>();
		newApp.Open();
		return newApp;
	}

	/// <summary>
	/// Called from ROSApp no matter how the app is closed
	/// </summary>
	/// <param name="app"></param>
	public void AppClosed(ROSApp app) {
		if (app.IsSingleton() && openSingletonApps.ContainsKey(app.GetType())) {
			openSingletonApps.Remove(app.GetType());
			app.transform.SetParent(appBuildArea, true);
		}
	}

	public static T OpenDialogue<T>() where T : ROSApp {
		return Desktop.ShowDialogue<T>();
	}

	public static ROSApp OpenDialogue(ROSApp app) {
		return Desktop.ShowDialogue(app);
	}

	public T ShowDialogue<T>() where T : ROSApp {
		return (T)ShowDialogue(dialogues[typeof(T)]);
	}

	public ROSApp ShowDialogue(ROSApp app) {
		// duplicate gameobject and put under working area
		GameObject appObj = Instantiate(app.gameObject, dialogueDisplayArea);
		ROSApp newApp = appObj.GetComponent<ROSApp>();

		// hide old one
		if (openDialogueStack.Count != 0)
			openDialogueStack.Peek().Hide();

		openDialogueStack.Push(newApp);
		newApp.AddResponseAction(DialogueClosed);

		blockingArea.alpha = 1f;
		blockingArea.blocksRaycasts = true;

		newApp.Open();
		return newApp;
	}

	private void DialogueClosed(Response r) {
		openDialogueStack.Pop();
		if (openDialogueStack.Count == 0) {
			blockingArea.alpha = 0f;
			blockingArea.blocksRaycasts = false;
		} else {
			openDialogueStack.Peek().Open();
		}
	}

	/// <summary>
	/// Called when interacted with (for picking up detection)
	/// </summary>
	/// <returns></returns>
	private bool OnInteract() {
		isPickedUp = true;
		return true;
	}

	/// <summary>
	/// Called when focus is called
	/// </summary>
	/// <returns></returns>
	private void OnPickedUp() {
		isPickedUp = true;
	}

	/// <summary>
	/// Called when Defocus is called
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

	public bool IsPickedUp() {
		return isPickedUp;
	}

	public bool TrySafeRegisterApp(ROSApp app) {
		if (app.IsDialogue()) {
			if (dialogues.ContainsValue(app))
				return false;
			dialogues.Add(app.GetType(), app);
			app.AppInit();
			Debug.Log("Dialogue registered: " + app);
			return true;
		} else {
			if (apps.ContainsValue(app))
				return false;
			apps.Add(app.GetType(), app);
			app.AppInit();
			Debug.Log("App registered: " + app);
			return true;
		}
	}

	public void RegisterApp(ROSApp app) {
		if (!TrySafeRegisterApp(app)) {
			Debug.LogError("App already registered: " + app);
		}
	}

	public void HandleLog(string message, string stack, LogType type) {
		ROSConsole.Print(message + "\n" + stack, type);
	}

	public void UpdateMessageBar() {
		ROSConsole.Message msg = ROSConsole.GetLastMessage();
		messageBarText.text = msg.GetTitle();
		messageBarText.color = msg.GetColor() * 2f;

		LogType logType = ROSConsole.GetHighestLogType();
		if (logType == LogType.Warning) {
			lockNotificationImage.enabled = true;
			lockNotificationImage.sprite = warningIcon;
			lockNotificationImage.color = warningColor;
		} else if (logType != LogType.Log) {
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
			lastFrameTime = Time.time;
		}

		// show/hide lockscreen as necessary
		lockScreenRend.alpha = Mathf.Lerp(lockScreenRend.alpha, ((IsPickedUp() || Application.isEditor) ? 0f : 1f), Time.deltaTime * lockScreenFadeSpeed);
		if (lockScreenRend.alpha < 0.01f) {
			lockScreenRend.alpha = 0f;
			lockScreenRend.blocksRaycasts = false;
		} else {
			lockScreenRend.blocksRaycasts = true;
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			ROSFileChooser fileChooser = ShowDialogue<ROSFileChooser>();
			fileChooser.SetContext(ROSFileChooser.ChooseType.File, ROSFileChooser.Operation.Save);
			fileChooser.SetFileName(".settings", false);
		}
	}

	public void AppHoverEnter(Image selectionBackground) {
		selectionBackground.color = appIconBackground;
	}

	public void AppHoverExit(Image selectionBackground) {
		selectionBackground.color = Color.clear;
	}

	public void AppClicked(GameObject appIconParent) {
		ShowApp(appIconLookup[appIconParent]);
	}

	public void OnMessageBarClick() {
		ShowApp<ROSConsole>();
	}

}

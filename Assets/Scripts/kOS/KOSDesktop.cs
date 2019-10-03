using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class KOSDesktop : MonoBehaviour {

	[Header("OS GUI References")]
	public Text timeText;
	public Image exitImage;
	public float exitImageHighlightMult = 1.1f;
	public Text messageBarText;
	public string infoColor = "white";
	public string warningColor = "orange";
	public string errorColor = "red";
	public Sprite defaultAppIcon;
	public KOSApp defaultApp;

	[Header("KTANE References")]
	public KMSelectable selectable;
	public KMHoldable holdable;

	[Header("Desktop Settings")]
	public Vector2 size = new Vector2(322f, 210f);
	public float dragSpeed = 1f;


	public List<KOSApp> apps;

	private float lastFrameTime;
	private Color exitImageBaseColor;
	private Queue<Message> consoleMessages = new Queue<Message>();

	private void Awake() {
		lastFrameTime = Time.time;
		exitImageBaseColor = exitImage.color;
	}


	private void Update() {

		// update time text every new second
		if ((int)Time.time != (int)lastFrameTime) {
			timeText.text = DateTime.Now.ToShortTimeString();
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			defaultApp.Open(this);
		}

	}

	private void UpdateMessageBar(Message msg) {
		messageBarText.text = "<color=" + (msg.type == MessageType.WARNING ? warningColor : (msg.type == MessageType.ERROR ? errorColor : infoColor)) + ">" + msg.type.ToString() + ": " + msg.message + "</color>";
	}

	public void LogMessage(string message) {
		LogMessage(message, MessageType.INFO);
	}

	public void LogMessage(string message, MessageType type) {
		Message msg = new Message(message, type);
		consoleMessages.Enqueue(msg);
		UpdateMessageBar(msg);
	}

	public void ExitMouseEnter(BaseEventData data) {
		exitImage.color = exitImageBaseColor * exitImageHighlightMult;
	}

	public void ExitMouseExit(BaseEventData data) {
		exitImage.color = exitImageBaseColor;
	}

	public void ExitMouseClick(BaseEventData data) {
		selectable.OnDeselect.Invoke();
	}

	private class Message {

		public string message;
		public MessageType type;

		public Message(string message, MessageType type) {
			this.message = message;
			this.type = type;
		}

	}

	public enum MessageType {
		INFO,
		WARNING,
		ERROR
	}
}

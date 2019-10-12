using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class ROSConsole : ROSApp {

	public Sprite downArrow;
	public Sprite upArrow;
	public GameObject messagePrefab;

	private Dictionary<int, Message> messages = new Dictionary<int, Message>();

	private static ROSConsole self;

	public override void AppInit() {
		self = this;
	}

	public void OnMessageArrowClick(GameObject obj) {
		if (!obj.name.StartsWith("Msg Obj:"))
			return;

		int id = int.Parse(obj.name.Split(':')[1]);
		Message msg = messages[id];

		Image arrow = obj.transform.GetChild(0).GetChild(0).GetComponent<Image>();
		Text msgText = obj.transform.GetChild(0).GetComponent<Text>();

		if (arrow.sprite == downArrow) {

			// change the text object to include title and message
			msgText.text = msg.GetTitle() + "\n" + msg.GetMessage();
			arrow.sprite = upArrow;

		} else {
			msgText.text = msg.GetTitle();
			arrow.sprite = downArrow;
		}
	}

	public static bool IsConsoleOpen() {
		return self.IsOpen();
	}

	public static void OpenConsole() {
		self.Open();
	}

	public static void Log(string message) {
		Print(message, LogType.Log);
	}

	public static void Warn(string message) {
		Print(message, LogType.Warning);
	}

	public static void Error(string message) {
		Print(message, LogType.Error);
	}

	public static void Exception( string message) {
		Print(message, LogType.Exception);
	}

	public static void Print(string message, LogType type) {
		float oldScrolLPos = self.scrollRect.verticalNormalizedPosition;

		// add to queue
		Message msg = new Message(message, type);
		self.messages.Add(self.messages.Count, msg);

		// add text object
		GameObject newTextObj = Instantiate(self.messagePrefab, self.scrollContentPane, false);
		newTextObj.name = "Msg Obj:" + (self.messages.Count - 1);

		// adjust its image component for color support
		newTextObj.GetComponent<Image>().color = msg.GetColor();

		// update text
		Text text = newTextObj.transform.GetChild(0).GetComponent<Text>();
		text.text = msg.GetTitle();

		// disable arrows if no message
		if (!msg.HasMessage()) {
			newTextObj.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
		}

		newTextObj.SetActive(true);

		ROSDesktop.Desktop.UpdateMessageBar();

		// try to keep old position
		self.scrollRect.verticalNormalizedPosition = oldScrolLPos;
	}

	public static LogType GetHighestLogType() {
		LogType type = LogType.Log;
		foreach (Message msg in self.messages.Values) {
			if (msg.GetMessageType() == LogType.Log)
				continue;
			if (msg.GetMessageType() == LogType.Warning) {
				type = LogType.Warning;
			} else {
				return LogType.Error;
			}
		}
		return type;
	}

	public static Message GetLastMessage() {
		return self.messages.Values.Last();
	}

	public sealed class Message {

		private string title = "";
		private string message = "";
		private LogType type = LogType.Log;

		public static Dictionary<LogType, Color> typeColors = new Dictionary<LogType, Color>() {
			{ LogType.Warning, new Color(1f, 0.75f, 0f, 0.5f) },
			{ LogType.Error, new Color(1f, 0f, 0f, 0.5f) },
			{ LogType.Exception, new Color(0.7f, 0f, 0f, 0.5f) },
			{ LogType.Log, new Color(0f, 0.8f, 1f, 0.5f) },
			{ LogType.Assert, new Color(0.8f, 0.4f, 0f, 0.5f) }
		};

		public static Color GetTypeColor(LogType type) {
			return (typeColors.ContainsKey(type) ? typeColors[type] : new Color(1f, 1f, 1f, 0.5f));
		}

		public Message(string message, LogType type) {
			message = message.Trim();
			if (message.Contains("\n")) {
				this.title = message.Split('\n')[0].Trim();
				this.message = message.Substring(message.IndexOf('\n')).Trim();
			} else
				this.title = message;
			this.type = type;
		}

		public Color GetColor() {
			return GetTypeColor(type);
		}

		public string GetTitle() {
			return title;
		}

		public bool HasMessage() {
			return message.Length != 0;
		}

		public string GetMessage() {
			if (!HasMessage())
				return title;
			else
				return message;
		}

		public LogType GetMessageType() {
			return type;
		}

		public override string ToString() {
			return title;
		}
	}

	
}

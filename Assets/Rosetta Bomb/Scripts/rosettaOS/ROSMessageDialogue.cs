using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ROSMessageDialogue : ROSApp {

	[Header("Local References")]
	public Text messageText;

	public void SetMessage(string message) {
		messageText.text = message;
	}

	public void SetContent(string title, string message) {
		SetMessage(message);
		SetTitle(title);
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class ROSFileChooser : ROSApp {

	public enum Operation {
		Save,
		Open
	}

	public enum ChooseType {
		Directory,
		File
	}

	[Header("Local GUI References")]
	public Text opButtonText;
	public InputField fileNameInput;

	private Operation op;
	private ChooseType type;

	private string currentDir;

	public void SetCurrentDirectory(string path) {
		if (!Directory.Exists(path)) {
			ROSDesktop.OpenDialogue<ROSMessageDialogue>().SetContent("Directory Invalid", "The provided directory isn't valid:\n" + path);
			return;
		}
		currentDir = path;
	}

	public void SetFileName(string filename, bool canEdit) {
		fileNameInput.text = filename;
		fileNameInput.interactable = canEdit;
	}

	public void OperationButtonClicked() {
		OnSuccessResponseClick();
	}

	public void CancelButtonClicked() {
		OnCancelResponseClick();
	}

	public void SetContext(ChooseType choose, Operation op) {
		this.type = choose;
		this.op = op;

		// change GUI
		opButtonText.text = (this.op == Operation.Open ? "Open" : "Save");
	}

	public override void AppInit() {

	}

	public override void AppOpen() {

	}

	public override void AppUpdate() {

	}

	public override void AppClose() {

	}
}

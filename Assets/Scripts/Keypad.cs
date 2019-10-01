// every .5 seconds
// if current question has been answered, check if it is right
// if not, give them a strike
// if all questions answered,
// pass module
// else, load next question
// if not, do nothing

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keypad : MonoBehaviour {

	// module object
	public KMBombModule module;

	public TextMesh questionText;

	// buttons objects. Custom classes can't be used here.
	public KMSelectable[] buttonSelectables;
	public MeshRenderer[] lightRenderers;
	public TextMesh[] buttonTexts;

	public KMSelectable submitButton;

	// selected light indicators objects
	private bool selected = false;
	public Material selectedLightOn;
	public Material selectedLightOff;

	private MeshRenderer lastIndicator;

	private int passageNumber = -1;
	private int questionNumber = -1;
	private int[] answers;

	public static string[] symbols = new string[] {
	"☢",
	"☵",
	"↗",
	"♃",
	"♟",
	"☄"
	};

	// Use this for initialization
	void Start() {

		if (!Tablet.chosenCourse.HasConfig("SymbolKeypad")) {
			questionText.text = "CNFG";
			return;
		}

		string config = Tablet.chosenCourse.GetConfig("SymbolKeypad");


		if (config.Length == 0 || config == "") {
			questionText.text = "ERR0";
			return;
		}
		string[] passages = config.Trim().Split('\n');
		if (passages.Length == 0) {
			questionText.text = "ERR1";
			return;
		}
		passageNumber = UnityEngine.Random.Range(0, passages.Length);
		string[] answerStrs = passages[passageNumber].Trim().Split(',');
		if (answerStrs.Length == 0) {
			questionText.text = "ERR2";
			return;
		}
		answers = new int[answerStrs.Length];
		for (int i = 0; i < answerStrs.Length; i++) {
			if (!int.TryParse(answerStrs[i].Trim(), out answers[i])) {
				questionText.text = "ERR3";
				return;
			}
		}

		questionNumber = 0;

		// randomize order of button symbols
		string[] newSymbols = new string[symbols.Length];
		Array.Copy(symbols, newSymbols, symbols.Length);
		Queue<string> remainingSymbols = new Queue<string>(Utilities.RandomizeArray(newSymbols));
		
		// assign symbols to text renderers
		foreach (TextMesh textMesh in buttonTexts) {
			textMesh.text = remainingSymbols.Dequeue();
		}

		int index = 0;
		foreach (KMSelectable selectable in buttonSelectables) {
			// using index counter, establish "final" variables to be used in the delegate
			// For loop variables, counters (such as index here) cannot be used in delegates
			// since by the time the delegate runs, that counter is still at the last value.
			// but these two references cannot change since being used in the delegate
			// therefore their status, presence, and validity stay
			TextMesh buttonText = buttonTexts[index];
			MeshRenderer light = lightRenderers[index];

			selectable.OnInteract += delegate {
				if (buttonText.text == symbols[answers[questionNumber] - 1]) {
					selected = true;
				} else {
					selected = false;
				}

				if (lastIndicator != null) {
					lastIndicator.sharedMaterial = selectedLightOff;
				}

				light.sharedMaterial = selectedLightOn;
				lastIndicator = light;

				return false;
			};
			index++;
		}

		// make module pass if the correct button is selected
		submitButton.OnInteract += delegate {

			if (lastIndicator == null) {
				return false;
			}
				
			if (questionText.text == "Win!")
				return false;

			if (selected == true) {
				if (questionNumber == answers.Length - 1) {
					module.HandlePass();
					questionText.text = "Win!";
				} else {
					questionNumber++;
					lastIndicator.sharedMaterial = selectedLightOff;
					questionText.text = (passageNumber + 1) + "." + (questionNumber + 1);
				}
			} else {
				lastIndicator.sharedMaterial = selectedLightOff;
				module.HandleStrike();
			}
			selected = false;
			lastIndicator = null;
			return false;
		};

		questionText.text = (passageNumber + 1) + "." + (questionNumber + 1);


	}
}

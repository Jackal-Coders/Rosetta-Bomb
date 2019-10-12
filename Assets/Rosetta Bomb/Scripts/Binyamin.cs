using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Binyamin : MonoBehaviour {

	public KMNeedyModule module;
    public KMBombInfo bombInfo;
	public KMSelectable dial;
    public TextMesh positionText;
    public TextMesh displayText;
    public WLCConfig configComponent;

	public Transform knobParent;
	public Transform selectionText;
	public float knobSmoothSpeed = 2f;

    private int position = 0;
    private string message;
    private string[] answers;
    private int currentAnswer;
    private string[] answer;
    private int index = -1;

	private int largestPosition = -1;

    // select new 
    private string GetNextAnswer() {
        index++;
        if(index == 0) {
            Utilities.RandomizeArray(answers);
        }
        return answers[index % answers.Length];
    }

    // Use this for initialization
    void Start () {

		if (!Tablet.chosenCourse.HasConfig("Binyamin")) {
			displayText.text = "CONFG";
			return;
		}

		string config = Tablet.chosenCourse.GetConfig("Binyamin");

        //answers = new int[] { 3, 5, 6, 3, 7, 4, 2, 1, 2, 5, 7 };

        // checks that config is not empty
        if(config.Length == 0 || config == "") {
            displayText.text = "ERR0";
            return;
        }
        answers = config.Trim().Split('\n');

        // checks that questions are defined
        if(answers.Length == 0)
        {
            displayText.text = "ERR1";
            return;
        }

		// setup options
		largestPosition = -1;
		foreach (string answer in answers) {
			try {
				int number = int.Parse(answer.Split(',')[0]);
				if (number > largestPosition) {
					largestPosition = number;
				}
			} catch (Exception e) {
				displayText.text = "ERR5";
				return;
			}
		}
		if (largestPosition != -1) {
			for (int i = 0; i < largestPosition; i++) {
				GameObject newSelection = Instantiate(selectionText.gameObject, selectionText.parent);
				newSelection.transform.localEulerAngles = new Vector3(0f, 0f, (i + 1) * (360f / (largestPosition + 1)));
				newSelection.transform.GetChild(0).GetComponent<TextMesh>().text = (i + 1) + "";
			}

		} else {
			displayText.text = "ERR4";
			return;
		}

        // runs when new round is activated
        module.OnNeedyActivation += delegate {

            // reset selected answer text object
            position = 0;
            positionText.text = position.ToString();

            // select a random question from the list
            answer = GetNextAnswer().Trim().Split(',');
            bool parse = int.TryParse(answer[0], out currentAnswer);
            if(parse) {
				//displayText.text = answer[1];
				displayText.text = Tablet.chosenCourse.language.Format(answer[1]);
                //displayText.text = language.SendText(textFormatter, language, answer[1]);
                Debug.Log(currentAnswer);
            }
            else {
                currentAnswer = -1;
                displayText.text = "ERR2";
            }
        };

        // controls dial position/answer selection
        dial.OnInteract += delegate {
            if(position < largestPosition) {
                position++;
            }
            else {
                position = 0;
            }

            Debug.Log(position);

            // rotate dial to next position
            positionText.text = position.ToString();
            return false;
        };

        // controls whether strike is given or not
        module.OnTimerExpired += delegate {
            if(currentAnswer != -1) {
                if(position != currentAnswer) {
                    module.HandleStrike();
                }
            }
        };

    }

	private void Update() {

		knobParent.localRotation = Quaternion.Lerp(knobParent.localRotation, Quaternion.Euler(new Vector3(-90f, 0f, position * (360f / (largestPosition + 1)))), Time.deltaTime * knobSmoothSpeed);

	}
}

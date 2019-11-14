using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Dial : MonoBehaviour {

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
    private Word[] words;
    private int currentAnswer;
    private string[] answer;
    private int index = -1;

	private int largestPosition = -1;

    // selects new word
    private Word GetNextWord() {
        index++;
        //return words[index % words.Length];
        return words[Random.Range(0, words.Length)];
    }

    // Use this for initialization
    void Start() {
		if(!Tablet.chosenCourse.HasConfig("Dial")) {
			displayText.text = "CONFG";
			return;
		}

        //string config = Tablet.chosenCourse.GetConfig("Binyamin");
        string config = Tablet.chosenCourse.GetConfig("Dial");
        Config jsonConfig = JsonConvert.DeserializeObject<Config>(config);
        words = jsonConfig.words;

        // checks that the config contains words to use
        if(jsonConfig.words.Length == 0) {
            displayText.text = "ERR0";
            return;
        }
        
        largestPosition = -1;
        foreach (Word word in words) {
            if(word.answer > largestPosition) {
                largestPosition = word.answer;
            }
        }
		if(largestPosition != -1) {
			for (int i = 0; i < largestPosition; i++) {
				GameObject newSelection = Instantiate(selectionText.gameObject, selectionText.parent);
				newSelection.transform.localEulerAngles = new Vector3(0f, 0f, (i + 1) * (360f / (largestPosition + 1)));
				newSelection.transform.GetChild(0).GetComponent<TextMesh>().text = (i + 1) + "";
			}
		} else {
			displayText.text = "ERR1";
			return;
		}

        // runs when new round is activated
        module.OnNeedyActivation += delegate {

            // reset selected answer text object
            position = 0;
            positionText.text = position.ToString();
            Word currentWord = GetNextWord();
            displayText.text = Tablet.chosenCourse.language.Format(currentWord.word);
            currentAnswer = currentWord.answer;
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

    // represents a word from the config
    private class Word {
        public string word;
        public int answer;
    }

    // represents the config that the module receives
    private class Config {
        public Word[] words;
    }
}

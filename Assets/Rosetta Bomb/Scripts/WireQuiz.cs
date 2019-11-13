using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class WireQuiz : MonoBehaviour {

	// Generic bomb info and references (not necessary)
	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMAudio KMAudio;
	public KMSelectable selfSelectable;

	public WLCConfig configSettings;

	// Used in actual module operation
	public TextMesh manualSectionText;
	public Material indicatorActive;
	public Material indicatorCorrect;

	/// <summary>
	/// Put these in order that they will light up in phases
	/// </summary>
	public MeshRenderer[] indicators;

	public Transform[] wireConnections;
	public Mesh[] wireMeshes;
	public Mesh[] wireCutMeshes;
	/// <summary>
	/// Used to convert colors into another list and to remove color innaccuricies. 
	/// </summary>
	public List<string> colorStrs = new List<string>();
	private List<Color> availableColors = new List<Color>();

	private int phaseCount;
	private int phase = 1;

	// Questions loaded, one per phase
    private List<Question> questions = new List<Question>();
    private Question currentQuestion;

    // List that holds the remaining wires left to cut (when size is 1, should be correct answer, next phase)
    private List<Color> uncutColors = new List<Color>();

	/// <summary>
	/// The following keeps track at which index is the color/mesh used to handle detection/change
	/// </summary>
	private List<Color> wireColorIndex = new List<Color>() {Color.black, Color.black, Color.black, Color.black, Color.black };
	/// <summary>
	/// Input the wire mesh index for each wire (to switch to cut version)
	/// </summary>
	public List<int> wireMeshIndex = new List<int>() { 0, 0, 0, 0, 0 };

	bool solved = false;
	bool waitForTimer = false;
	float firstFailTimer = 0f;

	private void Start() {
		// bomb start
		BombInfo.OnBombExploded += OnBombExploded;

		// load available colors (before config to make sure config colors match available colors)
		foreach (string color in colorStrs) {
			Color newColor;
			if (!ColorUtility.TryParseHtmlString(color, out newColor)) {
				manualSectionText.text = "ERR0";
				return;
			}
			availableColors.Add(newColor);
		}

		// get config
		if (!Tablet.chosenCourse.HasConfig("WireElimination")) {
			manualSectionText.text = "CNFG";
			return;
		}
        string config = Tablet.chosenCourse.GetConfig("WireElimination");
        Config jsonConfig = JsonConvert.DeserializeObject<Config>(config);

        // checks that the config contains at least five questions
        if(jsonConfig.questions.Length < 5) {
            manualSectionText.text = "ERR1";
            return;
        }

        // get number of questions
        phaseCount = Random.Range(3, 6);

        // picks random questions, verifies them, then adds them to the list
        for(int i = 0; i < phaseCount; i++) {
            Question question;
            do {
                question = jsonConfig.questions[Random.Range(0, jsonConfig.questions.Length)];
            } while(questions.Contains(question));
            //Color answer;

            // checks that the question is not empty
            if(question.question.Length == 0) {
                manualSectionText.text = "ERR2";
                return;
            }

            // checks that each option is a valid color
            foreach(string option in question.options) {
                Color optionColor;
                if (!ColorUtility.TryParseHtmlString(question.answer, out optionColor)) {
                    manualSectionText.text = "ERR3";
                    return;
                } else {
                    if(!availableColors.Contains(optionColor)) {
                        manualSectionText.text = "ERR3.1";
                        return;
                    }
                }
            }

            // checks that the answer is a valid color
            if(!ColorUtility.TryParseHtmlString(question.answer, out question.answerColor)) {
                manualSectionText.text = "ERR3";
                return;
            } else {
                if(!availableColors.Contains(question.answerColor)) {
                    manualSectionText.text = "ERR3.1";
                    return;
                }
            }
            questions.Add(question);
        }

		// fix phase count to questions if need be
		if (phaseCount > questions.Count) {
			phaseCount = questions.Count;
		}

		// setup event handling
		for (int i = 0; i < selfSelectable.Children.Length; i++) {
			int index = i;
			selfSelectable.Children[4 - i].OnInteract += delegate {
				HandleWireCut(index);
				return false;
			};
		}

		// setup first question
		SetPhase(phase);
    }

	private void Update() {
		if (waitForTimer) {
			firstFailTimer -= Time.deltaTime;
			manualSectionText.text = ((int)firstFailTimer + 1) + "";
			if (firstFailTimer <= 0f) {
				waitForTimer = false;
				SetPhase(1);
			}
		}
	}

	private void SetPhase(int phase) {
		this.phase = phase;
		
		// set indicator lights (do this before win for effect)
		for (int i = 0; i < phaseCount; i++) {
			if (i + 1 < phase) {
				// litten up
				indicators[i].sharedMaterial = indicatorCorrect;
			} else {
				// just active
				indicators[i].sharedMaterial = indicatorActive;
			}
		}
		if (this.phase == phaseCount + 1) {
			// they win the module
			BombModule.HandlePass();
			solved = true;
			return;
		}

        // set question text
        currentQuestion = questions[phase - 1];
		manualSectionText.text = Tablet.chosenCourse.language.Format(currentQuestion.question);

		// setup wires
		List<int> availableConnections = new List<int>() { 0, 1, 2, 3, 4 };
        List<Color> optionColors = new List<Color>();
        foreach(string colorString in currentQuestion.options) {
            Color colorObject;
            ColorUtility.TryParseHtmlString(colorString, out colorObject);
            if(availableColors.Contains(colorObject)) {
                optionColors.Add(colorObject);
            }
        }
		uncutColors.Clear();
        // go until we have satisfied all answers or ran out of available colors
        foreach (Color optionColor in optionColors) {
            int connectionIndex = availableConnections[Random.Range(0, availableConnections.Count)];
            availableConnections.Remove(connectionIndex);

            // Get wire at that connection
            GameObject wire = wireConnections[connectionIndex].GetChild(0).gameObject;
            wire.SetActive(true);

            // replace mesh back to full wire
            wire.GetComponent<MeshFilter>().sharedMesh = wireMeshes[wireMeshIndex[connectionIndex]];
            wireColorIndex[connectionIndex] = optionColor;
            MeshRenderer mr = wire.GetComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(mr.sharedMaterial);
            mr.sharedMaterial.SetColor("_Color", optionColor);
            uncutColors.Add(optionColor);
        }
		foreach (int c in availableConnections) {
			wireConnections[c].GetChild(0).gameObject.SetActive(false);
		}
	}

    // runs whenever a wire gets cut
	public void HandleWireCut(int wireIndex) {
		if (waitForTimer)
			return;

		// switch meshes
		selfSelectable.Children[4 - wireIndex].GetComponent<MeshFilter>().sharedMesh = wireCutMeshes[wireMeshIndex[wireIndex]];

		if (solved)
			return;

		// react to the wire cut
		Color wireColor = wireColorIndex[wireIndex];

		if (wireColor == questions[phase - 1].answerColor) {
			if (phase == 1) {

				// they failed on their first time...timer enable!
				waitForTimer = true;
				firstFailTimer = 15f;
			} else {
				SetPhase(phase - 1);
			}
			BombModule.HandleStrike();
		} else {
			uncutColors.Remove(wireColor);
			if (uncutColors.Count == 1) {
				SetPhase(phase + 1);
			}
		}
	}

    // runs when the bomb explodes
	public void OnBombExploded() {
		solved = true;
	}

    // represents a question from the config
    private class Question {
        public string question;
        public string[] options;
        public string answer;
        public Color answerColor;
    }

    // represents the config that the module receives
    private class Config {
        public Question[] questions;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	// All questions we will choose from
	private static List<QuizQuestion> availableQuestions = new List<QuizQuestion>();
	// Questions loaded, one per phase
	private List<QuizQuestion> questions = new List<QuizQuestion>();

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
		if (!Tablet.chosenCourse.HasConfig("WireQuiz")) {
			manualSectionText.text = "CNFG";
			return;
		}

		string config = Tablet.chosenCourse.GetConfig("WireQuiz");


        string[] lines = config.Split('\n');
		if (config == "" || lines.Length == 0) {
			// error
			manualSectionText.text = "ERR1";
			return;
		}
		foreach (string line in lines) {
			// try to load a question
			string dataStr = line.Trim();
			string[] data = dataStr.Split(',');
			if (data.Length != 3) {
				manualSectionText.text = "ERR2";
				return;
			} else {
				// first is question title, second is correct color, third is number of answers (wires)
				data[0] = data[0].Trim();
				data[1] = data[1].Trim();
				data[2] = data[2].Trim();

				Color color;
                /*if (!ColorUtility.TryParseHtmlString(data[1], out color) || !availableColors.Contains(color)) {
					manualSectionText.text = "ERR3";
					return;
				}*/
                if (!ColorUtility.TryParseHtmlString(data[1], out color))
                {
                    manualSectionText.text = "ERR3";
                    return;
                }
                else
                {
                    if (!availableColors.Contains(color))
                    {
                        manualSectionText.text = "ERR3.1";
                        return;
                    }
                }

                int answerCount;
				if (!int.TryParse(data[2], out answerCount)) {
					manualSectionText.text = "ERR4";
					return;
				}
				if (answerCount < 1) {
					answerCount = 1;
				}
				if (answerCount > 5) {
					answerCount = 5;
				}

				QuizQuestion question = new QuizQuestion(data[0], answerCount, color);
				availableQuestions.Add(question);
			}
		}

		// get number of questions
		phaseCount = Random.Range(3, 6);

		// choose questions (no repeats)
		for (int i = 0; i < phaseCount && availableQuestions.Count > 0; i++) {
			QuizQuestion selection = availableQuestions[Random.Range(0, availableQuestions.Count)];
			questions.Add(selection);
			availableQuestions.Remove(selection);
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




        ConditionParser conditionParser = new ConditionParser(BombInfo);
        Condition condition = conditionParser.Parse("serial.even");
        List<string> widgets = conditionParser.Check(condition);
        //manualSectionText.text = widgets[0];
        //Debug.Log(widgets);
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
		manualSectionText.text = questions[phase - 1].title;

		// setup wires
		List<int> availableConnections = new List<int>() { 0, 1, 2, 3, 4 };
		List<Color> remainingColors = new List<Color>(availableColors);
		uncutColors.Clear();
		// go until we have satisfied all answers or ran out of available colors
		for (int i = 0; i < questions[phase - 1].answerCount && remainingColors.Count > 0; i++) {
			int connectionIndex = availableConnections[Random.Range(0, availableConnections.Count)];
			availableConnections.Remove(connectionIndex);

			// Get wire at that connection
			GameObject wire = wireConnections[connectionIndex].GetChild(0).gameObject;
			wire.SetActive(true);

			// replace mesh back to full wire
			wire.GetComponent<MeshFilter>().sharedMesh = wireMeshes[wireMeshIndex[connectionIndex]];
			
			// change color to desired color
			Color color;
			if (i == 0) {
				// do correct answer color first to make sure it's done
				color = questions[phase - 1].correctColor;
			} else {
				color = remainingColors[Random.Range(0, remainingColors.Count)];
			}
			wireColorIndex[connectionIndex] = color;
			MeshRenderer mr = wire.GetComponent<MeshRenderer>();
			mr.sharedMaterial = new Material(mr.sharedMaterial);
			mr.sharedMaterial.SetColor("_Color", color);
			remainingColors.Remove(color);
			uncutColors.Add(color);			
		}

		foreach (int c in availableConnections) {
			wireConnections[c].GetChild(0).gameObject.SetActive(false);
		}
	}

	public void HandleWireCut(int wireIndex) {
		if (waitForTimer)
			return;

		// if we cut the correct wire, strike. 
		// if after cutting this wire, there is only one wire left, pass.

		// switch meshes
		selfSelectable.Children[4 - wireIndex].GetComponent<MeshFilter>().sharedMesh = wireCutMeshes[wireMeshIndex[wireIndex]];

		if (solved)
			return;

		// react to the wire cut
		Color wireColor = wireColorIndex[wireIndex];

		if (wireColor == questions[phase - 1].correctColor) {
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

	public void OnBombExploded() {
		solved = true;
	}

	private class QuizQuestion {

		public string title;
		public int answerCount;
		public Color correctColor;

		public QuizQuestion(string title, int answerCount, Color correctColor) {
			this.title = title;
			this.answerCount = answerCount;
			this.correctColor = correctColor;
		}
		
		public override string ToString() {
			return "Question: " + title + ", Answers: " + answerCount + ", Color: " + correctColor;
		}

	}
}


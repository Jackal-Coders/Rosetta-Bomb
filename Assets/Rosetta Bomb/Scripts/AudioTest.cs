using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour {

    public KMBombModule module;
    public KMSelectable passButton;
    public KMSelectable button;

    public new KMAudio audio;
    public Transform point;

    public WLCConfig configComponent;
    private string audioFile;
    private string[] files;

    // Use this for initialization
    void Start () {

		// get config from course if we can
		if (Tablet.chosenCourse.HasConfig("AudioTest")) {
			string config = Tablet.chosenCourse.GetConfig("AudioTest");
			if (config.Length == 0 || config == "") {
				//displayText.text = "ERR0";
				//audioFile = "doublebeep";
			} else {
				//audioFile = config;
				files = config.Trim().Split('\n');
				if (files.Length == 0) {
					//questionText.text = "ERR1";
					//return;
				} else {
					Utilities.RandomizeArray(files);
					audioFile = files[0];
				}
			}

			button.OnInteract += delegate {
				audio.PlaySoundAtTransform(audioFile, point);
				return false;
			};

			passButton.OnInteract += delegate {
				module.HandlePass();
				return false;
			};
		}
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.UI;
using System.IO;
using System;

public class Tablet : MonoBehaviour {

	public KMSelectable selfSelectable;
	public KMHoldable selfHoldable;
    //public KMMissionTableOfContents tableOfContents;
    //public KMModSettings settings;

    public Dropdown module;
    public Dropdown courseDropdown;
	public InputField input;
	public Button saveBtn;
	public Button revertBtn;

	public static Dictionary<string, Language> languages = new Dictionary<string, Language>();

    public static Dictionary<string, Course> courses = new Dictionary<string, Course>();
    public static Course chosenCourse;

    private string configFolderPath;
	private static string settingsFolderPath;

	bool lastEditable = false;

	private void Start() {
		settingsFolderPath = Application.persistentDataPath + "/Modsettings/";
		configFolderPath = Application.persistentDataPath + "/Configs/";

        //gameObject.SetActive(Application.platform == RuntimePlatform.WindowsPlayer && !VRSettings.enabled);
        if (!gameObject.activeSelf) {
			return;
		}

		Init();
	}

	private void Update() {

#if !UNITY_EDITOR
		bool editable = Vector3.Dot(transform.up, Vector3.up) < 0.5f;

		module.interactable = editable;
		courseDropdown.interactable = editable;
		input.interactable = editable;
		saveBtn.interactable = editable;
		revertBtn.interactable = editable;

		// this enables the tablet to be used after returning to main scene
		// it effectivley refreshes the entire tablet each time it's picked up
		if (editable) {
			if (editable != lastEditable) {
				Init();
			}
		}
		lastEditable = editable;

#endif
	}

	void Init() {

		// load all languages (must happen before courses load so they can pull from the dictionary)
		DirectoryInfo languageDirInfo = new DirectoryInfo(configFolderPath + "Languages/");
		if (!languageDirInfo.Exists || languageDirInfo.GetFiles().Length == 0) {
			input.text = "No languages were found.";
			return;
		}

		languages.Clear();
		foreach (FileInfo file in languageDirInfo.GetFiles()) {
			if (file.Extension == ".lang") {
				try {
					string contents = File.ReadAllText(file.FullName);
					Language newLang = Language.ParseConfig(contents);
					input.text += newLang.Name + '\n';
					languages.Add(newLang.Name, newLang);
				} catch (Exception e) {
					// includes language parse errors
					input.text = "Error on file: " + file.FullName + "\n" + e.ToString();
					return;
				}
			}
		}
		

		courses.Clear();
        courseDropdown.ClearOptions();
        List<string> courseOptions = new List<string>();

        DirectoryInfo info = new DirectoryInfo(configFolderPath + "Courses/");
        DirectoryInfo[] dirInfo = info.GetDirectories();

		if (dirInfo.Length == 0) {
			input.text = "No courses were found.";
			return;
		}

        // loops through course directories
        foreach(DirectoryInfo directory in dirInfo) {
            try {
                Course course = new Course(directory.Name);
                courseOptions.Add(course.Name);
                courses.Add(course.Name, course);
            } catch(Exception e) {
                courseOptions.Add(e.Message);
				print(e.ToString());
            }
        }
		courseDropdown.AddOptions(courseOptions);

		// load default config
		OnCourseDropDownChange();
	}

	// replace the text with the now selected config
	public void OnCourseDropDownChange() {
		if (courses.Count == 0)
			return;

		//input.text = configs[module.captionText.text];
        chosenCourse = courses[courseDropdown.captionText.text];

		// update the module drop down options
		module.ClearOptions();
		module.interactable = chosenCourse.configs.Count != 0;
		if (!module.enabled)
			return;

		List<String> moduleOptions = new List<string>();
		foreach (string mod in chosenCourse.configs.Keys) {
			moduleOptions.Add(mod);
		}
		module.AddOptions(moduleOptions);
		OnModuleDropDownChange();
		
    }

	public void OnModuleDropDownChange() {
		if (chosenCourse.configs.Count == 0)
			return;
		input.text = chosenCourse.configs[module.captionText.text];
	}

	public void OnSave() {
		//configs[module.captionText.text] = input.text;
		chosenCourse.configs[module.captionText.text] = input.text;
		chosenCourse.SaveConfigs();
    }

	public void OnRevert() {
		//input.text = configs[module.captionText.text];
		OnModuleDropDownChange();
	}
}

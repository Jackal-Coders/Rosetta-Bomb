using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;

public class Course {
    
    public Language language;
    //public string language;
    public Dictionary<string, string> configs = new Dictionary<string, string>();

	private string coursePath;

    public string Name {
        get;
        private set;
    }

    // creates a Course from a folder containing configs and metadata
    public Course(string folderName) {
		coursePath = Application.persistentDataPath + "/Configs/Courses/" + folderName;

        // checks that course exists and loads resources
        if(Directory.Exists(coursePath)) {
            if(File.Exists(coursePath + "/meta.txt")) {
                string metadata = File.ReadAllText(coursePath + "/meta.txt");

                // parses meta data
                string[] lines = metadata.Split('\n');
                foreach(string line in lines) {
                    string[] parts = line.Split(':');
                    if(parts.Length == 2) {
                        if(parts[0].ToLower().Trim() == "name") {
                            this.Name = parts[1].Trim();
                        } else if(parts[0].ToLower().Trim() == "language") {
							// load language by name
							//this.language = parts[1].Trim();
							this.language = Tablet.languages[parts[1].Trim()];
                        }
                    } else {
                        throw new Exception("meta.txt improperly formatted.");
                    }
                }

                // adds configs to config list
                if(Directory.Exists(coursePath + "/Configs/")) {
                    DirectoryInfo info = new DirectoryInfo(coursePath + "/Configs/");
                    FileInfo[] fileInfo = info.GetFiles();
                    foreach(FileInfo file in fileInfo) {
                        string configPath = coursePath + "/Configs/" + file.Name;
                        configs.Add(Path.GetFileNameWithoutExtension(configPath), File.ReadAllText(configPath));
                    }
                } else {
                    throw new Exception("Course at '" + coursePath + "' does not contain a Config directory.");
                }
            } else {
                throw new Exception("No meta.txt file was found in the course at '" + coursePath + "'.");
            }
        } else {
            throw new Exception("No course at '" + coursePath + "' could be found.");
        }
    }

	public bool HasConfig(string moduleName) {
		return configs.ContainsKey(moduleName);
	}

	public string GetConfig(string moduleName) {
		if (!HasConfig(moduleName))
			return null;
		return configs[moduleName];
	}

	public void SaveConfigs() {

		foreach (string configName in configs.Keys) {
			File.WriteAllText(coursePath + "/Configs/" + configName + ".txt", configs[configName]);
		}

	}
}
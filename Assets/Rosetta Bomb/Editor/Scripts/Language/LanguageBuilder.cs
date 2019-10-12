using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;

public class LanguageBuilder : EditorWindow {

	private string langDirPath = "";
	private string langFilePath = "";
	private static Language language;
	private Letter selectedLetter;

	private Vector2 letterScrollPos = Vector2.zero;

	private Dictionary<Letter, bool> foldoutStatus = new Dictionary<Letter, bool>();

	[MenuItem("Languages/Builder")]
	public static void ShowWindow() {
		GetWindow<LanguageBuilder>().Show();
	}

	[MenuItem("Assets/Create/Language File")]
	public static void CreateLanguageFile() {
		string newPath = EditorUtility.SaveFilePanel("New Language File", AssetDatabase.GetAssetPath(Selection.activeObject), "myLanguage.lang", "lang");
		if (newPath != "") { // checks if they clicked cancel
			using (StreamWriter writer = File.CreateText(newPath)) {
				writer.Write(GenerateEnglishConfig());
			}
			AssetDatabase.Refresh();
		}
	}

	public static Language LoadedLanguage() {
		return language;
	}

	private void OnGUI() {
		if (language != null && foldoutStatus.Count != language.Alpha.Count) {
			Dictionary<Letter, bool> newStatus = new Dictionary<Letter, bool>();
			for (int i = 0; i < language.Alpha.Count; i++) {
				if (foldoutStatus.ContainsKey(language.Alpha[i]))
					newStatus.Add(language.Alpha[i], foldoutStatus[language.Alpha[i]]);
				else
					newStatus.Add(language.Alpha[i], false);
			}
			foldoutStatus = newStatus;
		}

		titleContent = new GUIContent("Language Builder");

		// toolbar at top
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

		if (GUILayout.Button("New", EditorStyles.toolbarButton)) {
			// ask for new language
			string path = EditorUtility.SaveFilePanel("New Language File", langDirPath, "english.lang", "lang");
			if (path != "") {
				language = GenerateEnglishLanguage();

				File.WriteAllText(path, language.GenerateConfig());

				langFilePath = path;
				langDirPath = Path.GetDirectoryName(langFilePath);

			}
		}

		if (GUILayout.Button("Load", EditorStyles.toolbarButton)) {
			// ask for loaded language

			string path = EditorUtility.OpenFilePanel("Open Language File", langDirPath, "lang");
			if (path != "") {
				language = Language.ParseConfig(File.ReadAllText(path));
				langFilePath = path;
				langDirPath = Path.GetDirectoryName(langFilePath);
			}
		}

		GUILayout.FlexibleSpace();

		if (language != null && GUILayout.Button("Save", EditorStyles.toolbarButton)) {
			// save the language to config
			string path = EditorUtility.SaveFilePanel("Save Language File", langDirPath, Path.GetFileName(langFilePath), "lang");

			if (path != "") {

				File.WriteAllText(path, language.GenerateConfig());

				langFilePath = path;
				langDirPath = Path.GetDirectoryName(langFilePath);
			}
		}

		EditorGUILayout.EndHorizontal();
		// end toolbar

		if (language == null) {
			EditorGUILayout.HelpBox("Create a new language with the \"New\" button above", MessageType.Info);
			return;
		}

		EditorGUILayout.LabelField("Language loaded at " + langFilePath + "\nin " + langDirPath, EditorStyles.helpBox, GUILayout.ExpandWidth(true));

		language.Name = EditorGUILayout.TextField("Language Name:", language.Name);

		language.TextOrder = (Language.TextOrdering)EditorGUILayout.EnumPopup("Text Direction: ", language.TextOrder);


		// letters toolbar
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

		if (GUILayout.Button("Add", EditorStyles.toolbarButton)) {
			language.Alpha.Add(new Letter("New Letter", 'A'));
		}

		GUILayout.FlexibleSpace();

		EditorGUILayout.LabelField("Letters");

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Sort", EditorStyles.toolbarButton)) {
			language.SortLetters();
		}

		if (selectedLetter != null && GUILayout.Button("Remove Selected", EditorStyles.toolbarButton)) {
			language.Alpha.Remove(selectedLetter);
			selectedLetter = null;
		}

		EditorGUILayout.EndHorizontal(); // end letters toolbar

		// display warnings on letters here
		// duplicate name warning
		List<string> letterNames = new List<string>();
		List<string> duplicateNames = new List<string>();
		foreach (Letter l in language.Alpha) {
			if (letterNames.Contains(l.Name)) {
				duplicateNames.Add(l.Name);
			} else {
				letterNames.Add(l.Name);
			}
		}
		if (duplicateNames.Count != 0) {
			string names = "";
			foreach (string name in duplicateNames) {
				names += "\t" + name + "\n";
			}
			EditorGUILayout.HelpBox("There are multiple letters with the same name(s):\n" + names, MessageType.Warning);
		}

		// duplicate isolated warning
		List<char> letters = new List<char>();
		List<char> duplicateLetters = new List<char>();
		foreach (Letter l in language.Alpha) {
			if (letters.Contains(l.Isolated)) {
				duplicateLetters.Add(l.Isolated);
			} else {
				letters.Add(l.Isolated);
			}
		}
		if (duplicateLetters.Count != 0) {
			string chars = "";
			foreach (char c in duplicateLetters) {
				chars += "\t(0x" + ((ulong)c).ToString("X") + ") " + c + "\n";
			}
			EditorGUILayout.HelpBox("There are multiple letters with the same isolated character.\nThese should be unique.\n" + chars, MessageType.Warning);
		}

		// each letter gui
		letterScrollPos = EditorGUILayout.BeginScrollView(letterScrollPos);

		GUIStyle charLabelStyle = new GUIStyle(EditorStyles.label);
		charLabelStyle.alignment = TextAnchor.MiddleRight;
		Color defaultColor = GUI.backgroundColor;
		foreach (Letter letter in language.Alpha) {
			GUI.backgroundColor = (letter == selectedLetter) ? Color.cyan : defaultColor;

			// overall box outline
			EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"), GUILayout.ExpandWidth(true));

			// ensure letter is at least in the list 
			if (!foldoutStatus.ContainsKey(letter))
				foldoutStatus.Add(letter, false);

			// only continue if the foldoutstatus for this letter is true, toggled by the foldout method
			if (!(foldoutStatus[letter] = EditorGUILayout.Foldout(foldoutStatus[letter], letter.Name, true))) {
				EditorGUILayout.EndVertical();
				continue;
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Name", GUILayout.MaxWidth(40f));
			letter.Name = EditorGUILayout.TextField(letter.Name);
			EditorGUILayout.EndHorizontal();

			// character fields (setable by Unicode Browser)
			try {

				// combination settings
				if (letter.Combination != null) {
					bool remove = false; // used to prevent null errors
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Combination Settings");
					if (GUILayout.Button("Remove")) {
						remove = true;
					}
					EditorGUILayout.EndHorizontal();

					// first letter
					EditorGUILayout.BeginHorizontal();

					EditorGUILayout.LabelField("First Letter", GUILayout.MaxWidth(100f));
					EditorGUILayout.LabelField("0x", charLabelStyle, GUILayout.MaxWidth(20f));
					letter.Combination.FirstLetter = (char)Convert.ToUInt64(EditorGUILayout.TextField("0x" + ((ulong)letter.Combination.FirstLetter).ToString("X"), GUILayout.ExpandWidth(true)), 16);

					EditorGUILayout.LabelField("Char", charLabelStyle, GUILayout.MaxWidth(40f));
					letter.Combination.FirstLetter = EditorGUILayout.TextField(letter.Combination.FirstLetter + "", GUILayout.ExpandWidth(true))[0];

					if (GUILayout.Button("Pull from UB")) {
						GUI.FocusControl(null);
						letter.Combination.FirstLetter = (char)UnicodeBrowser.GetSelectedChar();
					}

					EditorGUILayout.EndHorizontal();

					// second
					EditorGUILayout.BeginHorizontal();

					EditorGUILayout.LabelField("Second Letter", GUILayout.MaxWidth(100f));
					EditorGUILayout.LabelField("0x", charLabelStyle, GUILayout.MaxWidth(20f));
					letter.Combination.SecondLetter = (char)Convert.ToUInt64(EditorGUILayout.TextField("0x" + ((ulong)letter.Combination.SecondLetter).ToString("X"), GUILayout.ExpandWidth(true)), 16);

					EditorGUILayout.LabelField("Char", charLabelStyle, GUILayout.MaxWidth(40f));
					letter.Combination.SecondLetter = EditorGUILayout.TextField(letter.Combination.SecondLetter + "", GUILayout.ExpandWidth(true))[0];

					if (GUILayout.Button("Pull from UB")) {
						GUI.FocusControl(null);
						letter.Combination.SecondLetter = (char)UnicodeBrowser.GetSelectedChar();
					}

					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();

					if (remove)
						letter.Combination = null;
				} else {
					if (GUILayout.Button("Add Combination")) {
						letter.Combination = new Combination('A', 'B');
					}
				}


				// isolated chars
				EditorGUILayout.BeginHorizontal();
				
				EditorGUILayout.LabelField("Isolated", GUILayout.MaxWidth(100f));
				EditorGUILayout.LabelField("0x", charLabelStyle, GUILayout.MaxWidth(20f));
				letter.Isolated = (char)Convert.ToUInt64(EditorGUILayout.TextField("0x" + ((ulong)letter.Isolated).ToString("X"), GUILayout.ExpandWidth(true)), 16);

				EditorGUILayout.LabelField("Char", charLabelStyle, GUILayout.MaxWidth(40f));
				letter.Isolated = EditorGUILayout.TextField(letter.Isolated + "", GUILayout.ExpandWidth(true))[0];

				if (GUILayout.Button("Pull from UB")) {
					GUI.FocusControl(null);
					letter.Isolated = (char)UnicodeBrowser.GetSelectedChar();
				}

				EditorGUILayout.EndHorizontal();

				// initial chars
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField("Initial", GUILayout.MaxWidth(100f));
				EditorGUILayout.LabelField("0x", charLabelStyle, GUILayout.MaxWidth(20f));
				letter.Initial = (char)Convert.ToUInt64(EditorGUILayout.TextField("0x" + ((ulong)letter.Initial).ToString("X")), 16);

				EditorGUILayout.LabelField("Char", charLabelStyle, GUILayout.MaxWidth(40f));
				letter.Initial = EditorGUILayout.TextField(letter.Initial + "", GUILayout.ExpandWidth(true))[0];

				if (GUILayout.Button("Pull from UB")) {
					GUI.FocusControl(null);
					letter.Initial = (char)UnicodeBrowser.GetSelectedChar();
				}

				EditorGUILayout.EndHorizontal();

				// medial
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField("Medial", GUILayout.MaxWidth(100f));
				EditorGUILayout.LabelField("0x", charLabelStyle, GUILayout.MaxWidth(20f));
				letter.Medial = (char)Convert.ToUInt64(EditorGUILayout.TextField("0x" + ((ulong)letter.Medial).ToString("X")), 16);

				EditorGUILayout.LabelField("Char", charLabelStyle, GUILayout.MaxWidth(40f));
				letter.Medial = EditorGUILayout.TextField(letter.Medial + "", GUILayout.ExpandWidth(true))[0];

				if (GUILayout.Button("Pull from UB")) {
					GUI.FocusControl(null);
					letter.Medial = (char)UnicodeBrowser.GetSelectedChar();
				}

				EditorGUILayout.EndHorizontal();

				// final
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField("Final", GUILayout.MaxWidth(100f));
				EditorGUILayout.LabelField("0x", charLabelStyle, GUILayout.MaxWidth(20f));
				letter.Final = (char)Convert.ToUInt64(EditorGUILayout.TextField("0x" + ((ulong)letter.Final).ToString("X")), 16);

				EditorGUILayout.LabelField("Char", charLabelStyle, GUILayout.MaxWidth(40f));
				letter.Final = EditorGUILayout.TextField(letter.Final + "", GUILayout.ExpandWidth(true))[0];

				if (GUILayout.Button("Pull from UB")) {
					GUI.FocusControl(null);
					letter.Final = (char)UnicodeBrowser.GetSelectedChar();
				}

				EditorGUILayout.EndHorizontal();

				

			} catch (OverflowException ofe) {
				EditorGUILayout.HelpBox("Character fields must be in hexidecimal (0xFFFF) format.", MessageType.Error);
			} catch (FormatException fe) {
				EditorGUILayout.HelpBox("Character fields must be in hexidecimal (0xFFFF) format.", MessageType.Error);
			} catch (ArgumentOutOfRangeException aofre) {
				// do nothing, this is when the field is blank, it's fine
			} catch (IndexOutOfRangeException ioore) {
				// do nothing, this happens when character text fields are empty
			}

			EditorGUILayout.EndVertical();

			if (Event.current.isMouse && Event.current.button == 0 && Event.current.type == EventType.MouseUp && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
				// this box was clicked
				if (selectedLetter == letter) {
					// deselect
					selectedLetter = null;
				} else {
					selectedLetter = letter;
				}
				GUI.FocusControl(null);
				Repaint();
				Event.current.Use();
			}
		}

		EditorGUILayout.EndScrollView();
		
	}


	private static Language GenerateEnglishLanguage() {
		string config = GenerateEnglishConfig();
		return Language.ParseConfig(config); 
	}

	/**
	 * This is only for initial language file creation, to fill it with English and have the user adjust it to their needs
	 * */
	private static string GenerateEnglishConfig() {
		StringBuilder str = new StringBuilder();

		str.Append("Name:English\n");
		str.Append("Order:LeftToRight\n");
		// add uppercase letters
		for (int i = 0x41; i < 0x41 + 26; i++) {
			str.Append("Letter:" + (char)i + "\n\tIsolated:0x" + i.ToString("X") + "\n");
		}

		return str.ToString();
	}

}

/**
 * public static List<Letter> GenerateArabicAlphabet() {
        List<Letter> arabicAlphabet = new List<Letter>();

        Letter alef = new Letter("Alef", '\u0627', new Dictionary<string, char>());
        alef.Tags.Add("isolated", '\uFE8D');
        alef.Tags.Add("final", '\uFE8E');
        arabicAlphabet.Add(alef);

        Letter beh = new Letter("Beh", '\u0628', new Dictionary<string, char>());
        beh.Tags.Add("isolated", '\uFE91');
        beh.Tags.Add("initial", '\uFE91');
        beh.Tags.Add("medial", '\uFE92');
        beh.Tags.Add("final", '\uFE90');
        arabicAlphabet.Add(beh);

        Letter teh = new Letter("Teh", '\u062A', new Dictionary<string, char>());
        teh.Tags.Add("isolated", '\uFE95');
        teh.Tags.Add("initial", '\uFE97');
        teh.Tags.Add("medial", '\uFE98');
        teh.Tags.Add("final", '\uFE96');
        arabicAlphabet.Add(teh);

        Letter theh = new Letter("Theh", '\u062B', new Dictionary<string, char>());
        theh.Tags.Add("isolated", '\uFE99');
        theh.Tags.Add("initial", '\uFE9B');
        theh.Tags.Add("medial", '\uFE9C');
        theh.Tags.Add("final", '\uFE9A');
        arabicAlphabet.Add(theh);

        Letter jeem = new Letter("Jeem", '\u062C', new Dictionary<string, char>());
        jeem.Tags.Add("isolated", '\uFE9D');
        jeem.Tags.Add("initial", '\uFE9F');
        jeem.Tags.Add("medial", '\uFEA0');
        jeem.Tags.Add("final", '\uFE9E');
        arabicAlphabet.Add(jeem);

        Letter hah = new Letter("Hah", '\u062D', new Dictionary<string, char>());
        hah.Tags.Add("isolated", '\uFEA1');
        hah.Tags.Add("initial", '\uFEA3');
        hah.Tags.Add("medial", '\uFEA4');
        hah.Tags.Add("final", '\uFEA2');
        arabicAlphabet.Add(hah);

        Letter khah = new Letter("Khah", '\u062E', new Dictionary<string, char>());
        khah.Tags.Add("isolated", '\uFEA5');
        khah.Tags.Add("initial", '\uFEA7');
        khah.Tags.Add("medial", '\uFEA8');
        khah.Tags.Add("final", '\uFEA6');
        arabicAlphabet.Add(khah);

        Letter dal = new Letter("Dal", '\u062F', new Dictionary<string, char>());
        dal.Tags.Add("isolated", '\uFEA9');
        dal.Tags.Add("final", '\uFEAA');
        arabicAlphabet.Add(dal);

        Letter reh = new Letter("Reh", '\u0631', new Dictionary<string, char>());
        reh.Tags.Add("isolated", '\uFEAD');
        reh.Tags.Add("final", '\uFEAE');
        arabicAlphabet.Add(reh);

        Letter zain = new Letter("Zain", '\u0632', new Dictionary<string, char>());
        zain.Tags.Add("isolated", '\uFEAF');
        zain.Tags.Add("final", '\uFEB0');
        arabicAlphabet.Add(zain);

        Letter seen = new Letter("Seen", '\u0633', new Dictionary<string, char>());
        seen.Tags.Add("isolated", '\uFEB1');
        seen.Tags.Add("initial", '\uFEB3');
        seen.Tags.Add("medial", '\uFEB4');
        seen.Tags.Add("final", '\uFEB2');
        arabicAlphabet.Add(seen);

        Letter sheen = new Letter("Sheen", '\u0634', new Dictionary<string, char>());
        sheen.Tags.Add("isolated", '\uFEB5');
        sheen.Tags.Add("initial", '\uFEB7');
        sheen.Tags.Add("medial", '\uFEB8');
        sheen.Tags.Add("final", '\uFEB6');
        arabicAlphabet.Add(sheen);

        Letter sad = new Letter("Sad", '\u0635', new Dictionary<string, char>());
        sad.Tags.Add("isolated", '\uFEB9');
        sad.Tags.Add("initial", '\uFEBB');
        sad.Tags.Add("medial", '\uFEBC');
        sad.Tags.Add("final", '\uFEBA');
        arabicAlphabet.Add(sad);

        Letter dad = new Letter("Dad", '\u0636', new Dictionary<string, char>());
        dad.Tags.Add("isolated", '\uFEBD');
        dad.Tags.Add("initial", '\uFEBF');
        dad.Tags.Add("medial", '\uFEC0');
        dad.Tags.Add("final", '\uFEBE');
        arabicAlphabet.Add(dad);

        Letter tah = new Letter("Tah", '\u0637', new Dictionary<string, char>());
        tah.Tags.Add("isolated", '\uFEC1');
        tah.Tags.Add("initial", '\uFEC3');
        tah.Tags.Add("medial", '\uFEC4');
        tah.Tags.Add("final", '\uFEC2');
        arabicAlphabet.Add(tah);

        Letter zah = new Letter("Zah", '\u0638', new Dictionary<string, char>());
        zah.Tags.Add("isolated", '\uFEC5');
        zah.Tags.Add("initial", '\uFEC7');
        zah.Tags.Add("medial", '\uFEC8');
        zah.Tags.Add("final", '\uFEC6');
        arabicAlphabet.Add(zah);

        Letter ain = new Letter("Ain", '\u0639', new Dictionary<string, char>());
        ain.Tags.Add("isolated", '\uFEC9');
        ain.Tags.Add("initial", '\uFECB');
        ain.Tags.Add("medial", '\uFECC');
        ain.Tags.Add("final", '\uFECA');
        arabicAlphabet.Add(ain);

        Letter ghain = new Letter("Ghain", '\u063A', new Dictionary<string, char>());
        ghain.Tags.Add("isolated", '\uFECD');
        ghain.Tags.Add("initial", '\uFECF');
        ghain.Tags.Add("medial", '\uFED0');
        ghain.Tags.Add("final", '\uFECE');
        arabicAlphabet.Add(ghain);

        Letter feh = new Letter("Feh", '\u0641', new Dictionary<string, char>());
        feh.Tags.Add("isolated", '\uFED1');
        feh.Tags.Add("initial", '\uFED3');
        feh.Tags.Add("medial", '\uFED4');
        feh.Tags.Add("final", '\uFED2');
        arabicAlphabet.Add(feh);

        Letter qaf = new Letter("Qaf", '\u0642', new Dictionary<string, char>());
        qaf.Tags.Add("isolated", '\uFED5');
        qaf.Tags.Add("initial", '\uFED7');
        qaf.Tags.Add("medial", '\uFED8');
        qaf.Tags.Add("final", '\uFED6');
        arabicAlphabet.Add(qaf);

        Letter kaf = new Letter("Kaf", '\u0643', new Dictionary<string, char>());
        kaf.Tags.Add("isolated", '\uFED9');
        kaf.Tags.Add("initial", '\uFEDB');
        kaf.Tags.Add("medial", '\uFEDC');
        kaf.Tags.Add("final", '\uFEDA');
        arabicAlphabet.Add(kaf);

        Letter lam = new Letter("Lam", '\u0644', new Dictionary<string, char>());
        lam.Tags.Add("isolated", '\uFEDD');
        lam.Tags.Add("initial", '\uFEDF');
        lam.Tags.Add("medial", '\uFEE0');
        lam.Tags.Add("final", '\uFEDE');
        arabicAlphabet.Add(lam);

        Letter meem = new Letter("Meem", '\u0645', new Dictionary<string, char>());
        meem.Tags.Add("isolated", '\uFEE1');
        meem.Tags.Add("initial", '\uFEE3');
        meem.Tags.Add("medial", '\uFEE4');
        meem.Tags.Add("final", '\uFEE2');
        arabicAlphabet.Add(meem);

        Letter noon = new Letter("Noon", '\u0646', new Dictionary<string, char>());
        noon.Tags.Add("isolated", '\uFEE5');
        noon.Tags.Add("initial", '\uFEE7');
        noon.Tags.Add("medial", '\uFEE8');
        noon.Tags.Add("final", '\uFEE6');
        arabicAlphabet.Add(noon);

        Letter heh = new Letter("Heh", '\u0647', new Dictionary<string, char>());
        heh.Tags.Add("isolated", '\uFEE9');
        heh.Tags.Add("initial", '\uFEEB');
        heh.Tags.Add("medial", '\uFEEC');
        heh.Tags.Add("final", '\uFEEA');
        arabicAlphabet.Add(heh);

        Letter yeh = new Letter("Yeh", '\u064A', new Dictionary<string, char>());
        yeh.Tags.Add("isolated", '\uFEF1');
        yeh.Tags.Add("initial", '\uFEF3');
        yeh.Tags.Add("medial", '\uFEF4');
        yeh.Tags.Add("final", '\uFEF2');
        arabicAlphabet.Add(yeh);

        Letter yehHamza = new Letter("Yeh", '\u0626', new Dictionary<string, char>());
        yehHamza.Tags.Add("isolated", '\uFE89');
        yehHamza.Tags.Add("initial", '\uFE8B');
        yehHamza.Tags.Add("medial", '\uFE8C');
        yehHamza.Tags.Add("final", '\uFE8A');
        arabicAlphabet.Add(yehHamza);

        Letter fatha = new Letter("Fatha", '\u064E', new Dictionary<string, char>());
        fatha.Tags.Add("type", 'a');
        arabicAlphabet.Add(fatha);

        return arabicAlphabet;
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;
using System.Text.RegularExpressions;

public class Language {

	public enum TextOrdering {
		LeftToRight,
		RightToLeft
	}

	private string languageName;
	private TextOrdering textOrder = TextOrdering.LeftToRight;
	private Dictionary<char, Letter> isolatedLookup;
	private Dictionary<string, Letter> nameLookup;
	
	public string Name {
		get {
			return languageName;
		}
		set {
			if (value != "" && value.Length > 0) {
				languageName = value;
			}
		}
	}

	public TextOrdering TextOrder {
		get {
			return textOrder;
		}
		set {
			textOrder = value;
		}
	}

	public List<Letter> Alpha { get;
		private set;
	}

	public Language(string name, TextOrdering textOrdering) : this(name, textOrdering, new List<Letter>()) {

	}

	public Language(string name, TextOrdering textOrdering, List<Letter> alphabet) {
		this.languageName = name;
		this.textOrder = textOrdering;
		this.Alpha = alphabet;

		isolatedLookup = new Dictionary<char, Letter>();
		nameLookup = new Dictionary<string, Letter>();

		// setup lookup dictionary
		UpdateLookup();
	}

	public void UpdateLookup() {
		isolatedLookup.Clear();
		nameLookup.Clear();
		foreach (Letter l in Alpha) {
			isolatedLookup.Add(l.Isolated, l);
			nameLookup.Add(l.Name, l);
		}
	}

	public string GenerateConfig() {
		StringBuilder str = new StringBuilder();

		str.Append("Name:" + languageName + "\n");
		str.Append("Order:" + textOrder.ToString() + "\n");
	
		foreach(Letter l in Alpha) {
			str.Append("Letter:" + l.Name + "\n");
			if (l.Combination != null) {
				str.Append("\tCombination:" + ((ulong)l.Combination.FirstLetter).ToString("X") + "," + ((ulong)l.Combination.SecondLetter).ToString("X") + "\n");
			}

			if (l.Isolated != '\0') {
				str.Append("\tIsolated:" + ((ulong)l.Isolated).ToString("X") + "\n");
			}

			if (l.Initial != '\0') {
				str.Append("\tInitial:" + ((ulong)l.Initial).ToString("X") + "\n");
			}

			if (l.Medial != '\0') {
				str.Append("\tMedial:" + ((ulong)l.Medial).ToString("X") + "\n");
			}

			if (l.Final != '\0') {
				str.Append("\tFinal:" + ((ulong)l.Final).ToString("X") + "\n");
			}
		}

		return str.ToString();
	}

	public string Format(string raw) {
		if (raw.Length <= 1)
			return raw;

		// because in code we work left to right in strings, we will look at the order and groupings first before reversing the string

		// we will also not consider multilines and do each line seperatley
		string[] lines = Regex.Split(raw, "\n+");

		for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++) {
			StringBuilder builder = new StringBuilder(lines[lineIndex]);

			// go through and check for multi-letter combinations first
			foreach (Letter l in Alpha) {
				if (l.Combination != null) {
					builder.Replace(l.Combination.GetPattern(), l.Isolated + "");
				}
			}

			// now check positionals
			for (int i = 0; i < raw.Length; i++) {

				if (!isolatedLookup.ContainsKey(raw[i]))
					continue; // skip any letter's not recognized

				Letter currentLetter = isolatedLookup[raw[i]];

				bool connectPrevious = false;
				bool connectNext = false;

				if (i != raw.Length - 1 && isolatedLookup.ContainsKey(raw[i + 1])) { // check for next letter
					Letter nextLetter = isolatedLookup[raw[i + 1]];
					connectNext = currentLetter.ConnectsToNext() && nextLetter.ConnectsToPrevious();
				}

				if (i != 0 && isolatedLookup.ContainsKey(raw[i - 1])) { // check previous letter
					Letter previousLetter = isolatedLookup[raw[i - 1]];
					connectPrevious = currentLetter.ConnectsToPrevious() && previousLetter.ConnectsToNext();
				}

				if (connectNext && connectPrevious) {
					builder[i] = currentLetter.Medial;
				} else if (connectPrevious && !connectNext) {
					builder[i] = currentLetter.Final;
				} else if (!connectPrevious && connectNext) {
					builder[i] = currentLetter.Initial;
				} else {
					builder[i] = currentLetter.Isolated; // this is a dumb line. it's included for ease of reading, logic flow, and to ensure the proper characters are shown
				}

			}

			// remove zero width non connectors
			builder = builder.Replace((char)(0x200C) + "", "");

			if (textOrder == TextOrdering.RightToLeft)
				lines[lineIndex] = Utilities.Reverse(builder.ToString());
			else
				lines[lineIndex] = builder.ToString();
		}

		string result = "";

		foreach (string line in lines) {
			result += line + "\n";
		}
		return result.Remove(result.Length - 1);
	}

	public void SortLetters() {
		Alpha = Alpha.OrderBy(l => l.Isolated).ToList();
	}

	public static Language ParseConfig(string config) {

		// removes leading and trialing whitepsace, removes empty lines, splits by new line, puts lines into a queue
		Queue<string> lines = new Queue<string>(Regex.Replace(config.Trim(), "\n\\s+", "\n").Split('\n'));

		// get name first -- (?i) turns off case matching
		string name = Regex.Replace(lines.Dequeue().Trim(), "(?i)Name:", "");
		if (name.Length == 0 || name == "") {
			throw new LanguageParseException("Name could not be found in config.");
		}

		// then text ordering
		TextOrdering order;
		try {
			order = (TextOrdering)Enum.Parse(typeof(TextOrdering), Regex.Replace(lines.Dequeue(), "(?i)Order:", ""));
		} catch (Exception e) {
			throw new LanguageParseException("Text Order could not be found in config for " + name + ".");
		}

		// load letters
		Language language = new Language(name, order);
		Letter letter = null;
		while (lines.Count > 0) {

			// if we come across "Letter" start a new letter and save the old one if there is one
			if (Regex.IsMatch(lines.Peek(), "(i?)Letter:")) {
				if (letter != null) {
					language.Alpha.Add(letter);
				}

				// letter name
				string letterName = Regex.Replace(lines.Dequeue().Trim(), "(?i)Letter:", "");
				if (letterName.Length == 0 || letterName == "") {
					throw new LanguageParseException("Letters could not be parsed for language " + name + ".");
				}
				letter = new Letter(letterName, '\0');
			} else {
				if (letter == null)
					continue;
				string line = lines.Dequeue().Trim();

				if (Regex.IsMatch(line, "(?i)Isolated:")) {
					try {
						letter.Isolated = (char)Convert.ToUInt64(Regex.Replace(line, "(?i)Isolated:", ""), 16);
					} catch (Exception e) {
						throw new LanguageParseException("Isolated form could not be parsed for letter " + letter.Name + " in language " + name + ".");
					}
				} else if (Regex.IsMatch(line, "(?i)Combination:")) {
					try {
						string[] tokens = Regex.Replace(line, "(?i)Combination", "").Split(',');
						if (tokens.Length != 2) {
							throw new LanguageParseException("Combination could not be parsed for letter " + letter.Name + " in language " + name + ".");
						}
						letter.Combination = new Combination((char)Convert.ToUInt64(tokens[0], 16), (char)Convert.ToUInt64(tokens[1], 16));
					}
					catch (Exception e) {
						throw new LanguageParseException("Combination could not be parsed for letter " + letter.Name + " in language " + name + ".");
					}
				} else if (Regex.IsMatch(line, "(?i)Initial:")) {
					try {
						letter.Initial = (char)Convert.ToUInt64(Regex.Replace(line, "(?i)Initial:", ""), 16);
					}
					catch (Exception e) {
						throw new LanguageParseException("Initial form could not be parsed for letter " + letter.Name + " in language " + name + ".");
					}
				} else if (Regex.IsMatch(line, "(?i)Medial")) {
					try {
						letter.Medial = (char)Convert.ToUInt64(Regex.Replace(line, "(?i)Medial:", ""), 16);
					}
					catch (Exception e) {
						throw new LanguageParseException("Medial form could not be parsed for letter " + letter.Name + " in language " + name + ".");
					}
				} else if (Regex.IsMatch(line, "(?i)Final:")) {
					try {
						letter.Final = (char)Convert.ToUInt64(Regex.Replace(line, "(?i)Final:", ""), 16);
					}
					catch (Exception e) {
						throw new LanguageParseException("Final form could not be parsed for letter " + letter.Name + " in language " + name + ".");
					}
				}

			}

		}
		// add the last letter that was created, if any
		if (letter != null)
			language.Alpha.Add(letter);

		language.UpdateLookup();

		return language;
	}

	public new string ToString() {
		return "Language: " + Name + ", Order: " + textOrder.ToString();
	}


	/** OLD CODE BELOW */

    public delegate string FormatText(Language lang, string text);

    public string SendText(FormatText function, Language lang, string text) {
        return function(lang, text);
    }
}

public class Letter {
	public Letter(string name, char isolated) : this(name, isolated, '\0', '\0', '\0') {

	}

	public Letter(string name, char isolated, char initial, char medial, char final) {
		this.Name = name;
		this.Isolated = isolated;
		this.Initial = initial;
		this.Medial = medial;
		this.Final = final;
	}

	public bool ConnectsToPrevious() {
		return (Final != '\0');
	}

	public bool ConnectsToNext() {
		return (Initial != '\0');
	}

	public bool ConnectsBothSides() {
		return (Medial != '\0');
	}

	public string Name { get;
		set; }

	public char Isolated { get;
		set; }

	public char Initial { get;
		set; }

	public char Medial { get;
		set; }

	public char Final { get;
		set; }

	public Combination Combination { get; set; }

}

public class Combination {

	public char FirstLetter {
		get; set;
	}
	public char SecondLetter {
		get; set;
	}

	public Combination(char first, char second) {
		FirstLetter = first;
		SecondLetter = second;
	}

	public string GetPattern() {
		return FirstLetter + "" + SecondLetter;
	}

}
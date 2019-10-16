using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Condition {
    public enum ConditionTypes {
        Batteries,
        Indicator,
        Ports,
        Serial
    }
    public enum ConditionStatuses {
        Even,
        Odd
    }
    public Condition.ConditionTypes Type;
    public Condition.ConditionStatuses Status;
    public Condition(ConditionTypes type, ConditionStatuses status) {
        Type = type;
        Status = status;
    }
}

public class ConditionParser {
    public KMBombInfo BombInfo;
    public ConditionParser(KMBombInfo bombInfo) {
        BombInfo = bombInfo;
    }

    // returns whether or not a condition is currently met
    //public bool Check(Condition condition) {
    public List<string> Check(Condition condition) {
        List<string> widgets = new List<string>();
        if(condition.Type == Condition.ConditionTypes.Serial) {
            widgets = BombInfo.QueryWidgets("serial", "");
        }
        foreach(string widget in widgets) {
            Debug.Log(widget);
        }
        return widgets;
    }

    // <summary>
    // parses a string written using our syntax to indicate whether or not a condition on a bomb is met
    // </summary>
    // <returns>Condition Object representing the condition to be checked</returns>
    public Condition Parse(string conditionString) {
        Condition.ConditionTypes conditionType;
        Condition.ConditionStatuses conditionStatus;
        string[] stringParts;

        // verifies that the string is formatted correctly
        stringParts = conditionString.Split('.');
        if(stringParts.Length != 2) {
            throw new Exception("String input must contain a single period. Either too many or too few were found.");
        }

        // parses and verifies the condition in the string
        switch(stringParts[0].Trim().ToLower()) {
            case "serial":
                conditionType = Condition.ConditionTypes.Serial;
                break;
            default:
                throw new Exception("First part of condition string must be one of the following: 'serial', 'ports', 'indicator', or 'batteries'.");
        }

        // parses and verifies the status in the string
        switch(stringParts[1].Trim().ToLower()) {
            case "even":
                conditionStatus = Condition.ConditionStatuses.Even;
                break;
            default:
                throw new Exception("Second part of condition string must be one of the following: 'even' or 'odd'.");
        }
        return new Condition(conditionType, conditionStatus);
    }
}
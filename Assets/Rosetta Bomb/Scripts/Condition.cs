using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Condition {
    public abstract bool Check(KMBombInfo bombInfo);
    //protected enum Statuses {};
    public static Condition Parse(string conditionString) {
        string[] stringParts;

        // verifies that the string is formatted correctly
        stringParts = conditionString.Split('.');
        if(stringParts.Length != 2) {
            throw new Exception("String input must contain a single period. Either too many or too few were found.");
        }

        // parses and verifies the condition in the string
        switch(stringParts[0].Trim().ToLower()) {
            case "serial":

                // parses and verifies the status in the string
                switch(stringParts[1].Trim().ToLower()) {
                    case "even":
                        return new SerialCondition(SerialCondition.Statuses.Even);
                    case "odd":
                        return new SerialCondition(SerialCondition.Statuses.Odd);
                    /*default:
                        throw new Exception("Second part of condition string must be one of the following: 'even' or 'odd'.");*/
                }
                break;
            default:
                throw new Exception("First part of condition string must be one of the following: 'serial', 'ports', 'indicator', or 'batteries'.");
        }
        throw new Exception("Second part of condition string must be one of the following: 'even' or 'odd'.");
    }
}
public class SerialCondition : Condition {
    public enum Statuses {
        Even,
        Odd
    };
    public Statuses Status;
    public SerialCondition(Statuses status) {
        Status = status;
    }
    public override bool Check(KMBombInfo bombInfo) {
        return true;
    }
}
public class BatteryCondition : Condition {
    protected enum Statuses {
        AA
    };
    public override bool Check(KMBombInfo bombInfo) {
        return true;
    }
}
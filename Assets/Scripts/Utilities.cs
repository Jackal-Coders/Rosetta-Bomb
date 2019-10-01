using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Utilities {
    public static string[] RandomizeArray(string[] arr) {
        for(var i = arr.Length - 1; i > 0; i--) {
            int r = UnityEngine.Random.Range(0, i);
            string tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
        return arr;
    }

    public static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

	
}

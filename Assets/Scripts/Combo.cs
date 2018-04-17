using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="Combo", menuName = "Combo", order = 1)]
public class Combo : ScriptableObject {

    public Buttons[] c;

    public bool Equals(Combo other)
    {
        // If lengths are different can't be the same
        if (c.Length != other.c.Length)
            return false;
        // Else iterate over checking each buttons value in the combo
        for (int i = 0; i < c.Length; i++)
        {
            // Fail if discrepancy found
            if (!c[i].Equals(other.c[i]))
                return false;
        }
        return true;
    }


    public override string ToString()
    {
        if (c.Length == 0)
            return "";
        string output = "";
        for (int i = 0; i < c.Length; i++)
        {
            output += c[i].ToString() + "_";
        }
        return output.Remove(output.Length-1);
    }

}

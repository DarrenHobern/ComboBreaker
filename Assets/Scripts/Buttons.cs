using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class Buttons
{
    public const int NUM_BUTTONS = 8;
    public const int MODIFIER_OFFSET = 2;

    public bool[] b = new bool[NUM_BUTTONS];

    public Buttons()
    {
        b = new bool[NUM_BUTTONS];
    }
    public Buttons(bool cross, bool circle, bool square, bool triangle, bool l1, bool r1, bool l2, bool r2)
    {
        b = new bool[NUM_BUTTONS];
        b[0] = cross;
        b[1] = circle;
        b[2] = square;
        b[3] = triangle;
        b[4] = l1;
        b[5] = r1;
        b[6] = l2;
        b[7] = r2;
    }
    public Buttons(Buttons other)
    {
        b = (bool[])other.b.Clone();
    }

    /// <summary>
    /// Logical OR operation on two button-inputs. Includes modifiers.
    /// </summary>
    /// <param name="other"></param>
    public void Merge(Buttons other)
    {
        for (int i = 0; i < NUM_BUTTONS; i++)
        {
            if (!b[i])
                b[i] = other.b[i];
        }
    }

    /// <summary>
    /// Returns true if no buttons are active. Ignores modifiers
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        for (int i = 0; i < NUM_BUTTONS-MODIFIER_OFFSET; i++)
        {
            if (b[i])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns the number of active buttons, exculding modifiers.
    /// </summary>
    /// <returns></returns>
    public int Count()
    {
        int count = 0;
        for (int i = 0; i < NUM_BUTTONS-MODIFIER_OFFSET; i++)
        {
            if (b[i])
                ++count;
        }
        return count;
    }

    /// <summary>
    /// Returns a bool[] of modifiers. L2, R2.
    /// </summary>
    /// <returns>L2=0, R2=1</returns>
    public bool[] GetModifiers()
    {
        int m = 0;
        bool[] modifiers = new bool[MODIFIER_OFFSET];
        for (int i = NUM_BUTTONS-MODIFIER_OFFSET; i < NUM_BUTTONS; i++)
        {
            modifiers[m] = b[i];
            m++;
        }
        return modifiers;
    }

    /// <summary>
    /// Returns true if same buttons are active in other.
    /// </summary>
    /// <param name="other">The buttons object to be compared to.</param>
    /// <returns>True if the buttons are equal.</returns>
    public bool Equals(Buttons other)
    {
        for (int i = 0; i < NUM_BUTTONS; i++)
        {
            if (b[i] != other.b[i])
                return false;
        }
        return true;
    }

    public override string ToString()
    {
        string output = "";
        if (b[0])
            output += "X-";
        if (b[1])
            output += "O-";
        if (b[2])
            output += "U-";
        if (b[3])
            output += "V-";
        if (b[4])
            output += "L1-";
        if (b[5])
            output += "R1-";
        if (b[6])
            output += "L2-";
        if (b[7])
            output += "R2-";
        
        if (output.Length > 0)
            return output.Remove(output.Length - 1);
        return output;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class Attackable : MonoBehaviour {


    public abstract HitType Hit(Buttons buttons);
    public abstract HitType Hit(Buttons buttons, Vector3 sourcePosition, float knockForce);
    public abstract int GetComboIndex();
    public abstract void RestoreCombo();

    public Combo combo;
    public ButtonIcons bIcons;

}

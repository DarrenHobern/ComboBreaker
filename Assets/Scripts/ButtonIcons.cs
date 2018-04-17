using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ButtonIcons", menuName = "Button Icons", order = 2)]
public class ButtonIcons : ScriptableObject {
    public Sprite[] s = new Sprite[Buttons.NUM_BUTTONS+1];  // +1 for when you have both L2 and R2 simultaneously
}

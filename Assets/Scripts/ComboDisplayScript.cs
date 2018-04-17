using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboDisplayScript : MonoBehaviour {

    private Combo activeCombo = null;  // The actively visualised combo in the combo panel
    private ButtonIcons bIcons = null; // The button icon skin selected.
    public Transform iconsBoarder;
    public Transform iconsSingle;
    public Transform iconsDouble;
    public Transform iconsTriple;
    public Transform iconsQuad;

    [SerializeField]
    private Color activeColour;
    [SerializeField]
    private Color inactiveColour;
    [SerializeField]
    private Color l2BoarderColour;
    [SerializeField]
    private Color r2BoarderColour;

    private Transform iconsBox;


    #region Setters
    /// <summary>
    /// Sets the skin for the buttons to be displayed
    /// </summary>
    /// <param name="icons">The skin to be displayed.</param>
    public void SetButtonIcons(ButtonIcons icons)
    {
        bIcons = icons;
    }

    /// <summary>
    /// Sets the combo visualiser to the given combo, usually the target of the player.
    /// </summary>
    /// <param name="combo">Combo to switch to.</param>
    public void SetCombo(Combo combo)
    {
        activeCombo = combo;
    }
    #endregion

    /// <summary>
    /// Clears the combo display.
    /// </summary>
    public void ClearComboIcons()
    {
        // Boarders
        foreach (Transform i in iconsBoarder)
        {
            i.gameObject.SetActive(false);
        }
        // Single
        foreach (Transform i in iconsSingle)
        {
            i.gameObject.SetActive(false);
        }
        // Double
        foreach (Transform i in iconsDouble)
        {
            i.gameObject.SetActive(false);
        }
        // Triple
        foreach (Transform i in iconsTriple)
        {
            i.gameObject.SetActive(false);
        }
        // Quad
        foreach (Transform i in iconsQuad)
        {
            i.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the images in the ui displaying the combo of the object currently targeted by the player.
    /// </summary>
    public void UpdateComboIcons(int comboIndex)
    {
        if (bIcons == null)
        {
            Debug.LogError("Icons not set");
            return;
        }
        if (activeCombo == null) {
            Debug.LogError("No active combo set.");
            return;
        }
        int comboLength = activeCombo.c.Length;

        for (int i = 0; i < comboLength; i++)
        {
            // Disable unused modifiers
            bool[] activeModifiers = activeCombo.c[i].GetModifiers();
            bool hasModifiers = (activeModifiers[0] || activeModifiers[1]);
            if (!hasModifiers)
                iconsBoarder.GetChild(i).gameObject.SetActive(false);
            else
                iconsBoarder.GetChild(i).gameObject.SetActive(true);

            // Disable unused buttons
            int numberOfActiveButtons = activeCombo.c[i].Count();
            iconsBox = iconsSingle.GetChild(i);
            switch (numberOfActiveButtons)
            {
                case 1:
                    iconsDouble.GetChild(i).gameObject.SetActive(false);
                    iconsTriple.GetChild(i).gameObject.SetActive(false);
                    iconsQuad.GetChild(i).gameObject.SetActive(false);
                    break;
                case 2:
                    iconsBox = iconsDouble.GetChild(i);
                    iconsSingle.GetChild(i).gameObject.SetActive(false);
                    iconsTriple.GetChild(i).gameObject.SetActive(false);
                    iconsQuad.GetChild(i).gameObject.SetActive(false);
                    break;
                case 3:
                    iconsBox = iconsTriple.GetChild(i);
                    iconsSingle.GetChild(i).gameObject.SetActive(false);
                    iconsDouble.GetChild(i).gameObject.SetActive(false);
                    iconsQuad.GetChild(i).gameObject.SetActive(false);
                    break;
                case 4:
                    iconsBox = iconsQuad.GetChild(i);
                    iconsSingle.GetChild(i).gameObject.SetActive(false);
                    iconsDouble.GetChild(i).gameObject.SetActive(false);
                    iconsTriple.GetChild(i).gameObject.SetActive(false);
                    break;
                default:
                    Debug.LogError("THAT'S TOO MANY BUTTONS!");
                    return;
            }
            iconsBox.gameObject.SetActive(true);
            Image[] buttonImages = iconsBox.GetComponentsInChildren<Image>();
            Image modifierImages = iconsBoarder.GetChild(i).GetComponentInChildren<Image>();

            int imageIndex = 0; // the index of the image in this iconBox. Up to four
            int buttonIndex = 0; // The index for the button, need to iterate over them all as we do no know which are active.

            while (buttonIndex < Buttons.NUM_BUTTONS)
            {
                if (activeCombo.c[i].b[buttonIndex])  // If the button at the current position in the combo is active
                {
                    if (buttonIndex < Buttons.NUM_BUTTONS - Buttons.MODIFIER_OFFSET)  // Regular buttons
                    {

                        buttonImages[imageIndex].sprite = bIcons.s[buttonIndex];  // Set the current button in this position in the combo to the active button icon
                        if (i == comboIndex)
                            buttonImages[imageIndex].color = activeColour;
                        else
                            buttonImages[imageIndex].color = inactiveColour;
                    }
                    else  // Modifiers
                    {
                        Sprite sprite = bIcons.s[buttonIndex];
                        Color colour = l2BoarderColour;
                        if (activeModifiers[0] && activeModifiers[1])  // If both L2 and R2
                        {
                            sprite = bIcons.s[Buttons.NUM_BUTTONS];  // There is one more button icon then there are buttons because there can be the modifier of both L2 and R2
                        }
                        else if (activeModifiers[1])  // only R2
                        {
                            colour = r2BoarderColour;
                        }
                        
                        modifierImages.sprite = sprite;
                        modifierImages.color = colour;

                    }
                    ++imageIndex;  // move to the next button at this position in the combo
                }
                //if (imageIndex >= numberOfActiveButtons)
                //    break;
                ++buttonIndex;  // Look at the next potential button
            }
        }

        // Clear the button icons that might be remaining from old targets. All icon sets have the same number of buttons.
        // For future me who is confused; Only clears the icons AFTER the current combo, its not resetting the whole thing constantly.
        for (int i = comboLength; i < iconsSingle.childCount; i++)
        {
            iconsBoarder.GetChild(i).gameObject.SetActive(false);
            iconsSingle.GetChild(i).gameObject.SetActive(false);
            iconsDouble.GetChild(i).gameObject.SetActive(false);
            iconsTriple.GetChild(i).gameObject.SetActive(false);
            iconsQuad.GetChild(i).gameObject.SetActive(false);
        }
    }
}

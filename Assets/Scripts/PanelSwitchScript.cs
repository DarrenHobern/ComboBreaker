using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSwitchScript : MonoBehaviour {

    [SerializeField]
    GameObject defaultPanel;
    [SerializeField]
    private GameObject[] panels;

    private GameObject activePanel;

    private void Awake()
    {
        SelectPanel(defaultPanel.name);
        EnableDefault();
    }

    /// <summary>
    /// Sets the active panel to the matching named panel if it exists.
    /// </summary>
    /// <param name="panelName">The case sensitive name of the panel to swap to.</param>
    /// <returns>True if the panel is not null.</returns>
    private bool SetActivePanel(string panelName)
    {
        // If its not null and equal to the new panel, do nothing.
        if (activePanel != null)
            if (activePanel.name.Equals(panelName))
                return true;
        // Else search for it
        foreach (GameObject p in panels)
        {
            if (p.name.Equals(panelName))
            {
                activePanel = p;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Disables the current active panel, swaps to the given panel, and activates it.
    /// </summary>
    /// <param name="panelName">The case sensitive name of the panel to swap to.</param>
    public void SelectPanel(string panelName)
    {
        if (activePanel != null)
        {
            // Do nothing if trying to swap to the same panel
            if (activePanel.name.Equals(panelName))
                return;

            activePanel.SetActive(false);
        }

        if (!SetActivePanel(panelName))
            return;
        activePanel.SetActive(true);
    }

    public void EnableDefault()
    {
        foreach (GameObject g in panels)
        {
            if (g.Equals(defaultPanel))
                continue;
            else
                g.SetActive(false);
        }
        SelectPanel(defaultPanel.name);
    }
}

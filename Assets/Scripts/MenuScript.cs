using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ButtonAnimation
{
    SLIDE,
    BOUNCE
}

public class MenuScript : MonoBehaviour {

    enum MenuState
    {
        IDLE,
        ATTACKING
    }

    [SerializeField]
    private bool playOnAwake = true;

    [SerializeField]
    private ButtonAnimation buttonAnim;

    [SerializeField]
    private ComboDisplayScript comboScript;
    [SerializeField]
    private PanelSwitchScript switchScript;

    [SerializeField]
    private GlobalAttackableScript[] targets;
    private int index = 0;

    private bool menuOpen = false;

    private MenuState state = MenuState.IDLE;
    private float attackSpeed = 0.05f;
    private bool attackReset = true;
    private float nextAttack = 0;
    private Buttons cumulativeButtons = new Buttons(); // When attacking, there is a small window which button presses are merged together to create the input which will be sent to the target
    private Buttons nextButtons = new Buttons();

    private Vector3 initialPosition;  // initial position of the text

    private void Awake()
    {
        initialPosition = targets[index].transform.position;
    }

    private void OnEnable()
    {
        if (playOnAwake)
            SetControl(true);
    }

    private void OnDisable()
    {
        SetControl(false);
        comboScript.ClearComboIcons();
    }

    private void RestorePositions()
    {
        foreach (GlobalAttackableScript t in targets)
        {
            t.ResetTriggers();
            t.transform.position = initialPosition;
        }
    }

    /// <summary>
    /// Sets the control state of the menu.
    /// </summary>
    /// <param name="menu">The menu is in control.</param>
    public void SetControl(bool menu)
    {

        RestorePositions();
        if (menu)
        {
            menuOpen = true;
            Restart();
        }
        else
        {
            menuOpen = false;
        }
    }

    
    /// <summary>
    /// Sets the animation type and the restores the combos of each of the navigation buttons.
    /// If the menu is open then show the first navigation button.
    /// </summary>
    private void Restart()
    {
        index = 0;
        foreach (GlobalAttackableScript t in targets)
        {
            t.SetAnimationType(buttonAnim);
            t.RestoreCombo();
        }
        if (menuOpen)
        {
            Enter(true);
            UpdateTarget();
        }
    }

    private void Update()
    {
        float select = Input.GetAxisRaw("Select");

        if (!menuOpen)
        {
            if (select > 0)
            {
                switchScript.EnableDefault();
                SetControl(true);
            }
            else
            {
                return;
            }
        }
            
        #region  inputs 
        float t = Time.time;
        // Left Stick
        float lhor = Input.GetAxis("LHorizontal");
        float lver = Input.GetAxis("LVertical");
        // Right Stick
        float rhor = Input.GetAxis("RHorizontal");
        float rver = Input.GetAxis("RVertical");
        // Buttons
        float cross = Input.GetAxisRaw("Cross");
        float square = Input.GetAxisRaw("Square");
        float circle = Input.GetAxisRaw("Circle");
        float triangle = Input.GetAxisRaw("Triangle");
        float l1 = Input.GetAxisRaw("L1");
        float r1 = Input.GetAxisRaw("R1");
        float l2 = Input.GetAxis("L2");
        float r2 = Input.GetAxis("R2");
        // D pad
        float dhor = Input.GetAxisRaw("DHorizontal");
        float dver = Input.GetAxisRaw("DVertical");
        #endregion

        nextButtons = new Buttons(cross > 0, circle > 0, square > 0, triangle > 0, l1 > 0, r1 > 0, l2 > 0, r2 > 0);

        switch (state)
        {
            case MenuState.IDLE:
                if (nextButtons.IsEmpty() && dhor == 0)
                {
                    attackReset = true;
                    break;
                }
                else if (attackReset)
                {
                    // Can either switch targets or attack the current target
                    if (SwitchTargets(dhor))
                    {
                        attackReset = false;
                        break;
                    }
                    state = MenuState.ATTACKING;
                    nextAttack = Time.time + attackSpeed;
                    cumulativeButtons = new Buttons();
                    attackReset = false;
                    cumulativeButtons.Merge(nextButtons);
                }
                break;
            case MenuState.ATTACKING:
                cumulativeButtons.Merge(nextButtons);
                if (Time.time > nextAttack)
                {
                    state = MenuState.IDLE;
                    if (targets[index] != null)
                    {
                        switch (targets[index].Hit(cumulativeButtons))
                        {
                            case HitType.Hit:
                                comboScript.UpdateComboIcons(targets[index].GetComboIndex());
                                break;
                            default:
                                break;
                        }
                    }
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Attempts to switch the target.
    /// Positive values will move right.
    /// Negative values will move left.
    /// Returns true if the target changed.
    /// </summary>
    /// <param name="input">User input to switch target.</param>
    /// <returns>True if the target changed.</returns>
    private bool SwitchTargets(float input)
    {
        if (input == 0)  // No input = no change.
        {
            return false;
        }
        //RestorePositions();
        targets[index].RestoreCombo();
        if (input > 0)  // right
        {
            Exit(false);
            index = (index + 1) % targets.Length;
            Enter(false);
        }
        else if (input < 0)  // left
        {
            Exit(true);
            index = (index - 1) % targets.Length;
            if (index < 0)
                index += targets.Length;
            Enter(true);
        }

        UpdateTarget();

        return true;
    }

    /// <summary>
    /// Plays the animation for entering.
    /// </summary>
    /// <param name="left">True if entering <b>to</b> the left.</param>
    private void Enter(bool left)
    {
        targets[index].Enter(left);
    }

    /// <summary>
    /// Plays the animation for exiting.
    /// </summary>
    /// <param name="left">True if exiting <b>to</b> the left.</param>
    private void Exit(bool left)
    {
        targets[index].Exit(left);
    }

    /// <summary>
    /// Updates the combo display for the new target.
    /// </summary>
    private void UpdateTarget()
    {
        comboScript.SetButtonIcons(targets[index].bIcons);
        comboScript.SetCombo(targets[index].combo);
        comboScript.UpdateComboIcons(targets[index].GetComboIndex());
    }

    public void QuitGame()
    {

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void SwitchScene(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}

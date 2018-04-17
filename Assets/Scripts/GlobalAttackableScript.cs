using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))][RequireComponent(typeof(Animator))]
public class GlobalAttackableScript : Attackable
{

    private int comboIndex = 0;
    private Button action;
    [SerializeField]
    private Animator animator;
    

    private void Awake()
    {
        action = GetComponent<Button>();
    }

    public override int GetComboIndex()
    {
        return comboIndex;
    }

    public override void RestoreCombo()
    {
        comboIndex = 0;
    }

    #region Animations

    public void SetAnimationType(ButtonAnimation buttonAnim)
    {
        animator.SetBool("Slide", buttonAnim.Equals(ButtonAnimation.SLIDE));
    }

    /// <summary>
    /// Resets all the animation triggers.
    /// </summary>
    public void ResetTriggers()
    {
        // Boy I wish unity had a built in way to do this... sure hope I don't have to change the triggers at all
        animator.ResetTrigger("EnterL");
        animator.ResetTrigger("EnterR");
        animator.ResetTrigger("ExitL");
        animator.ResetTrigger("ExitR");
    }

    public void Enter(bool left)
    {
        if (left)
            animator.SetTrigger("EnterL");
        else
            animator.SetTrigger("EnterR");
    }

    public void Exit(bool left)
    {
        if (left)
            animator.SetTrigger("ExitL");
        else
            animator.SetTrigger("ExitR");

    }

    #endregion

    public override HitType Hit(Buttons buttons)
    {
        if (combo.c[comboIndex].Equals(buttons))
        {
            if (DealDamage())
            {
                return HitType.Kill;
            }
            else
            {
                return HitType.Hit;
            }
        }

        return HitType.Miss;
    }

    private bool DealDamage()
    {
        ++comboIndex;
        if (comboIndex >= combo.c.Length)
        {
            action.onClick.Invoke();
            return true;
        }
        return false;
    }

    public override HitType Hit(Buttons buttons, Vector3 sourcePosition, float knockForce)
    {
        return Hit(buttons);
    }
}

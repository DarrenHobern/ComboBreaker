using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitType
{
    Kill,
    Hit,
    Miss,
    None
}


/**
 * Script for controlling damageable objects
 */
public class DamageScript : Attackable {

    // COMBAT
    private int comboIndex = 0;  // Acts as the health of this object, increasing this progresses the combo and therefore deals damage
    public bool vulnerable = true;  // Can this be hurt at the moment.
    public bool isDead = false;
    private State state = State.IDLE;
    private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        comboIndex = 0;
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Killed");
    }

    /// <summary>
    /// Returns the index in the combo that has been completed.
    /// </summary>
    /// <returns></returns>
    public override int GetComboIndex()
    {
        return comboIndex;
    }


    /// <summary>
    /// Returns the difference between the number of inputs in the combo and the comboIndex.
    /// </summary>
    /// <returns>The number of inputs remaining in the combo.</returns>
    public int GetHealth()
    {
        return combo.c.Length - comboIndex;
    }


    #region Damage
    public override HitType Hit(Buttons buttons)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Checks to see if the next input of the combo was correctly entred.
    /// If so, damage dealt to this will occur on the next beat (handled by the GameController) and a value of True will be returned.
    /// If the incorrect input is given, return False.
    /// </summary>
    /// <param name="buttons">The input given.</param>
    public override HitType Hit(Buttons buttons, Vector3 sourcePosition, float knockForce)
    {
        if (!vulnerable)
            return HitType.Miss;
        if (isDead)
            return HitType.None;

        // If the correct input was given, then cast the DealDamage function to OnBeat run by the GameController.
        if (combo.c[comboIndex].Equals(buttons))
        {
            if (DealDamage())
            {
                return HitType.Kill;
            }
            else
            {
                Knockback(sourcePosition, knockForce);
                return HitType.Hit;
            }
        }
      
        return HitType.Miss;
    }


    /// <summary>
    /// Applies a knockback to this object using via an explosive force originating at source with strength knockForce.
    /// stronger knockback is received if the source is closer.
    /// </summary>
    /// <param name="source">The position of the damage source.</param>
    /// <param name="knockForce">The force of the knockback.</param>
    private void Knockback(Vector3 source, float knockForce)
    {
        rb.AddExplosionForce(knockForce, source, 10f, 0f, ForceMode.Impulse);  // 10f is the player's attack range
    }


    /// <summary>
    /// Increments the combo by one point, thereby dealing one damage.
    /// Returns true if the hit kills this.
    /// </summary>
    /// <returns>True if this was the killing blow.</returns>
    private bool DealDamage()
    {
        ++comboIndex;
        if (CheckDeath())
        {
            return true;
        }
        
        return false;
    }

    #endregion


    #region Death
    public override void RestoreCombo()
    {
        comboIndex = 0;
    }

    /// <summary>
    /// Checks if the end of the combo has been reached.
    /// </summary>
    /// <returns>True if the object is dead.</returns>
    private bool CheckDeath()
    {
        if (GetHealth() <= 0)
        {
            isDead = true;
            GameController.OnBeat += Die;
            GetComponent<Rigidbody>().isKinematic = true;
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
            return true;
        }
        return false;
    }

    private void Die()
    {
        GameController.OnBeat -= Die;
        GameController.OnBeat += Dead;

        animator.SetTrigger("Killed");
    }


    private void Dead()
    {
        GameController.OnBeat -= Dead;        
        // sound effects here ***
        GameController.Instance.AddScore(combo.c.Length);
        transform.root.gameObject.SetActive(false);
    }
    #endregion
}

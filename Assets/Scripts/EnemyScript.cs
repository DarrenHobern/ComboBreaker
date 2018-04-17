using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class EnemyScript : MonoBehaviour {

    [SerializeField][Tooltip("The upward angle of force applied when moving toward the player.")]
    private float ANGLE = 1f;
    [SerializeField][Tooltip("The maximum force that can be applied to this enemy when moving toward the player.")]
    private float maxForce = 100f;
    [SerializeField][Tooltip("The minimum force that will be applied to this enemy when moving toward the player.")]
    private float minForce = 30f;
    [SerializeField]
    private int damage = 1;
    private PlayerScript player;
    private GameController gameController;
    private DamageScript ds;
    private Vector3 direction = Vector3.forward;
    private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
        rb = GetComponent<Rigidbody>();
        player = PlayerScript.Player;
        gameController = GameController.Instance;
        ds = GetComponentInChildren<DamageScript>();
    }

    private void OnEnable()
    {
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Killed");
        GameController.OnMeasure += Move;
    }

    private void OnDisable()
    {
        GameController.OnMeasure -= Move;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !ds.isDead)
        {
            ds.isDead = true;
            GameController.OnBeat += Attack;
        }
    }


    /// <summary>
    /// Moves the enemy toward the player.
    /// Force is not normalised so that enemies which are further from the player will move further.
    /// Because of this, the force is clamped between minforce and maxforce.
    /// </summary>
    private void Move()
    {
        direction = Vector3.ClampMagnitude(CalculateDirection()*minForce, maxForce);
        rb.AddForce(direction);
    }

    /// <summary>
    /// Returns a vector3 in the direction of the player from this object.
    /// Then Sets the Y component of the vector to ANGLE.
    /// </summary>
    /// <returns>The new direction to the player</returns>
    private Vector3 CalculateDirection()
    {
        Vector3 dir = player.transform.position - transform.position;
        dir.y = ANGLE;
        return dir;
    }

    /// <summary>
    /// Make an attack against the object in front of the enemy
    /// </summary>
    private void Attack()
    {
        GameController.OnBeat -= Attack;
        GameController.OnBeat += Dead;
        animator.SetTrigger("Attack");
    }

    /// <summary>
    /// Damages the player, removes some score
    /// </summary>
    private void Dead()
    {
        GameController.OnBeat -= Dead;
        gameController.AddScore(-ds.GetHealth());
        player.DealDamage(damage, gameObject);
        transform.root.gameObject.SetActive(false);
    }
}

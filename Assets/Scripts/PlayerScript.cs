using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

    static PlayerScript player;
    public static PlayerScript Player
    {
        get
        {
            if (player == null)
            {
                PlayerScript instance = FindObjectOfType<PlayerScript>();
                if (instance == null)
                {
                    Debug.LogError("Player not found");
                    return null;
                }
                player = instance;
                return instance;
            }
            return player;
        }
    }

    
    private GameController gameController;

    // ============================= INSPECTOR ======================================================
    [SerializeField]
    private Transform beamStart;  // The start point of the aiming beam, used to rotate about the player
    [SerializeField]
    private float beamAngle = 15f; // Half the width of the aiming beam
    [SerializeField]
    private float beamSpeed = 5f;  // How fast does the beam rotate
    [SerializeField]
    private float attackSpeed = 0.1f;  // How many seconds do we have to wait between attacks
    [SerializeField]
    private float camSpeed = 2f;  // How fast the camera rotates/moves
    [SerializeField]
    private float stunDuration = 0.5f;  // How long the player will be stunned for if they enter hitstun.

    // ============================= PRIVATES ======================================================
    private Attackable target = null;  // Object we are targeting with our attacks
    private Attackable oldTarget = null; // The target from the last frame, used so that we only update the hud when our target changed.
    private Transform beamCentre;  // The end point of the aiming centre beam
    private ParticleSystem ps;
    private Rigidbody rb;

    // Movement
    [SerializeField]
    private float moveForce = 50f;    // How much force gets applied when we land a killing blow
    [SerializeField]
    private float boostForce = 20f;  // the force applied each update during a boost.
    [SerializeField]
    private float maxBoostFuel = 2f;  // Maximum amount of time in seconds that we can boost for.
    [SerializeField]
    private float boostGain = 0.5f; // How much fuel do we gain by killing enemies.
    private float boostFuel = 0f;  // How much fuel do we actually have?



    // Combat
    [SerializeField]
    private float knockForce = 10f;  // How much force is applied to enemies we successfully hit
    private State state = State.IDLE;
    private bool vulnerable = true;
    private float nextAttack = 0; // Time in seconds which the current time must pass before the next attack may be performed.
    private float stunTime = 0f;  // Time in seconds which the current time must pass before we exit hitstun.
    private bool attackReset = true;  // Can we attack now?
    private Buttons cumulativeButtons = new Buttons(); // When attacking, there is a small window which button presses are merged together to create the input which will be sent to the target
    private Buttons nextButtons = new Buttons();

    [SerializeField]
    private LayerMask layerMask;
    private Vector3 beamLeft;   // End point of the left beam
    private Vector3 beamRight;  // End point of the right beam
    private Vector3 beamUp;     // End point of the up beam
    private Vector3 beamDown;     // End point of the down beam
    [SerializeField]
    private Transform targetPlane;  // Plane used to trigger particle explosions on the target. Moved to target position on

    [SerializeField]
    private GameObject PlayerModel;
    [SerializeField]
    private ParticleSystem attackParticleSystem;

    // Camera
    [SerializeField]
    private Transform cam;

    [SerializeField][HideInInspector]
    private Vector3 initOffset;
    [SerializeField][HideInInspector]
    private Quaternion initRotation;
    private Vector3 currOffset;

    [ContextMenu("Set Current Offset")]
    private void SetCurrentOffset()
    {
        initOffset = cam.position - transform.position;
        initRotation = cam.rotation;
    }

    #region Functions

    private void Awake()
    {
        gameController = GameController.Instance;
        beamCentre = beamStart.GetChild(0).transform;
        rb = GetComponent<Rigidbody>();

        currOffset = initOffset;

        cumulativeButtons = new Buttons();
        nextButtons = new Buttons();
        attackReset = true;
        state = State.IDLE;
    }

    /// <summary>
    ///  REFORMAT THIS CRAP
    /// </summary>
    public float spd = 2;
    public float rad = 1f;
    private void FixedUpdate()
    {
        PlayerModel.transform.localPosition = new Vector3(rad*(float)Math.Sin((Time.time*spd)%(2*Math.PI)), 0, rad*(float)Math.Cos((Time.time*spd)%(2*Math.PI)));
    }

    void Update() {
        if (!gameController.GState.Equals(GameState.PLAYING))
            return;

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

        // Boosting
        if (dhor != 0 || dver != 0)
        {
            if (boostFuel > 0)
            {
                boostFuel -= Time.deltaTime;
                Vector3 boost = new Vector3(dhor * boostForce, 0, dver * boostForce);
                rb.AddRelativeForce(boost);
                SetBoost();
            }
        }

        #region Aiming & Camera
        // AIMING BEAM
        if (lhor != 0 || lver != 0) {
            float aimDir = Mathf.Atan2(lhor, lver);
            // Make the direction relative to the camera's rotation.
            Quaternion camRot = Quaternion.Euler(0, cam.rotation.eulerAngles.y, 0);

            transform.rotation = Quaternion.Slerp(beamStart.rotation, (Quaternion.Euler(0f, aimDir * Mathf.Rad2Deg, 0f) * camRot), Time.deltaTime * beamSpeed);
        }

        FindClosestEnemy();

        // CAMERA ROTATION
        cam.position = transform.position + currOffset;
        if (rhor != 0 || rver != 0)
        {
            cam.RotateAround(transform.position, Vector3.up, rhor * camSpeed);
            currOffset = cam.position - transform.position;
        }
        #endregion
        

        nextButtons = new Buttons(cross > 0, circle > 0, square > 0, triangle > 0, l1 > 0, r1 > 0, l2 > 0, r2 > 0);
        
        switch (state)
        {
            case State.IDLE:
                if (nextButtons.IsEmpty())
                {
                    attackReset = true;
                    break;
                }
                else if (attackReset)
                {
                    state = State.ATTACKING;
                    nextAttack = Time.time + attackSpeed;
                    cumulativeButtons = new Buttons();
                    attackReset = false;
                    cumulativeButtons.Merge(nextButtons);
                }
                break;
            case State.ATTACKING:
                cumulativeButtons.Merge(nextButtons);
                if (Time.time > nextAttack)
                {
                    state = State.IDLE;
                    if (target != null)
                    {
                        
                        switch (target.Hit(cumulativeButtons, transform.position, knockForce))
                        {
                            case HitType.None:
                                break;
                            case HitType.Miss:
                                EnterHitStun();
                                break;
                            case HitType.Hit:
                                gameController.UpdateButtonHUD(target.GetComboIndex());
                                break;
                            case HitType.Kill:
                                boostFuel = Mathf.Clamp(boostFuel + boostGain, 0, maxBoostFuel);
                                SetBoost();
                                gameController.ScreenShake(HitType.Kill);
                                AttackMove(target.transform.position);
                                break;
                        }
                    }
                }

                break;
            case State.HITSTUN:
                if (Time.time >= stunTime)
                {
                    state = State.IDLE;
                }
                break;
        }
    }

    /// <summary>
    /// Resets the player. (Position, rotation, vulnerability, etc)
    /// </summary>
    public void ResetPlayer()
    {
        transform.position = Vector3.up;
        transform.rotation = Quaternion.identity;
        currOffset = initOffset;
        cam.rotation = initRotation;
        SetVulnerability(true);
        target = null;
        oldTarget = null;
        boostFuel = 0;
        SetBoost();
        state = State.IDLE;
    }

    private void SetBoost()
    {
        gameController.SetBoost(boostFuel / maxBoostFuel);
    }


    /// <summary>
    /// Puts the player into hitstun and sets the stunduration
    /// </summary>
    private void EnterHitStun()
    {
        state = State.HITSTUN;
        gameController.ScreenShake(HitType.Miss);
        stunTime = Time.time + stunDuration;
    }

    /// <summary>
    /// Sets the target to the closest attackable object in the aimBeam.
    /// </summary>
    private void FindClosestEnemy()
    {
        // Use TransformDirection to correctly adjust for local/world position when the player is not at 0,0
        // Left
        Quaternion aimAngle = Quaternion.AngleAxis(-beamAngle, Vector3.up);
        beamLeft = aimAngle * beamCentre.TransformDirection(beamCentre.localPosition);
        // Up
        aimAngle = Quaternion.AngleAxis(-beamAngle, Vector3.left);
        beamUp = aimAngle * beamCentre.TransformDirection(beamCentre.localPosition);
        // Right
        aimAngle = Quaternion.AngleAxis(beamAngle, Vector3.up);
        beamRight = aimAngle * beamCentre.TransformDirection(beamCentre.localPosition);
        // Down
        aimAngle = Quaternion.AngleAxis(beamAngle, Vector3.left);
        beamDown = aimAngle * beamCentre.TransformDirection(beamCentre.localPosition);

        // Debug lines
        Debug.DrawRay(beamStart.position, beamCentre.TransformDirection(beamCentre.localPosition), Color.yellow);  // Centre
        /**
         * 3x3 box of rays so that things close to the ground can be hit.
         * Rays are also parallel to the ground to avoid weird angle things.
         * Maybe the top row are angled up slightly
         */
        
        Debug.DrawRay(beamStart.position, beamLeft, Color.red);   // left
        Debug.DrawRay(beamStart.position, beamUp, Color.black);     // Up
        Debug.DrawRay(beamStart.position, beamRight, Color.blue);  // right
        Debug.DrawRay(beamStart.position, beamDown, Color.white);   // down


        // Look down all beams, then find the closest hit target.
        Attackable centreTarget = TargetBeam(beamCentre.TransformDirection(beamCentre.localPosition));
        Attackable leftTarget = TargetBeam(beamLeft);
        Attackable rightTarget = TargetBeam(beamRight);
        Attackable upTarget = TargetBeam(beamUp);
        Attackable downTarget = TargetBeam(beamDown);
        Attackable[] potentialTargets = { centreTarget, leftTarget, rightTarget, upTarget, downTarget };

        SelectTarget(potentialTargets);
        if (target != null)
        {
            targetPlane.position = target.transform.position;
            //targetPlane.rotation = transform.rotation;
        }
    }

    /// <summary>
    /// Set the target to the closest of the potentials.
    /// Or null if none are found.
    /// Also sets oldTarget to the target before updating, and if oldTarget and target are different updatse the HUD icons.
    /// </summary>
    /// <param name="potentialTargets"></param>
    private void SelectTarget(Attackable[] potentialTargets)
    {
        oldTarget = target;
        target = null;

        float closestDistance = float.MaxValue;
        foreach (Attackable t in potentialTargets)
        {
            if (t != null)
            {
                float tDist = (t.transform.position - transform.position).sqrMagnitude;
                if (tDist <= closestDistance)
                {
                    target = t;
                    closestDistance = tDist;
                }
            }
        }

        if (target != oldTarget)
        {
            //gameController.SetButtonHUD((DamageScript)target);
            gameController.SetButtonHUD(target);
        }
    }

    /// <summary>
    /// Casts a beam from the beam start position to the given end position.
    /// Returns an Attackable potential target if a target is found with an attackable script.
    /// </summary>
    /// <param name="endPosition"></param>
    /// <returns>The potential target.</returns>
    private Attackable TargetBeam(Vector3 endPosition)
    {
        RaycastHit hit;
        //if (Physics.Raycast(beamStart.position, endPosition, out hit, beamCentre.localPosition.z, layerMask, QueryTriggerInteraction.Ignore))
        if (Physics.SphereCast(beamStart.position, 0.5f, endPosition, out hit, beamCentre.localPosition.z, layerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform.gameObject.GetComponent<Attackable>();
        }
        return null;
    }

    /// <summary>
    /// Moves the player in the direction of the given position.
    /// </summary>
    /// <param name="target">The Location of the thing we're attacking.</param>
    public void AttackMove(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        position.y = 1f;
        rb.AddForce(direction * moveForce);
    }

    
    /// <summary>
    /// Deal damage to the player, from the source.
    /// </summary>
    /// <param name="damage">The amount of damage to be delt before defence.</param>
    /// <param name="source">The source of the damage</param>
    public void DealDamage(int damage, GameObject source)
    {
        if (vulnerable)
        {
            gameController.DealDamage(damage);
            state = State.HITSTUN;
            attackReset = false;
            nextAttack = Time.time + stunDuration;
        }
        else
        {
            // Blocked or something, enemy might go into hitstun
        }
    }

    public void SetVulnerability(bool vulnerable)
    {
        this.vulnerable = vulnerable;
    }
    #endregion
}

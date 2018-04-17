using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum State
{
    IDLE,
    ATTACKING,
    HITSTUN,
    REGEN
}

public enum GameState
{
    PLAYING,
    PAUSED,
    SCORESCREEN
}

public class GameController : MonoBehaviour {


    static GameController instance;
    public static GameController Instance
    {
        get
        {
            if (instance == null)
            {
                GameController gameController = FindObjectOfType<GameController>();
                if (gameController == null)
                {
                    Debug.LogError("GameController not found");
                    return null;
                }
                instance = gameController;
                return instance;
            }
            return instance;
        }
    }


    [SerializeField]
    private GameUIScript uiScript;
    [SerializeField]
    private PanelSwitchScript switchScript;

    private GameState gameState = GameState.PLAYING;
    public GameState GState
    {
        get
        {
            return gameState;
        }
    }

    // --- Audio sync things ---
    [SerializeField]
    private GameObject audioOutput;
    public delegate void SyncedAction();
    public static event SyncedAction OnBeat;
    public static event SyncedAction OnMeasure;
    public float bpm = 120f;
    public int timeSignature = 4;
    private float nextAttack = 0f;
    private float secondsPerBeat;
    private float beat = 0f;  // The time in seconds of the next beat
    private int beatCount = 0;  // The number of beats that have occurred in this measure. [0..timeSig]
    

    // --- Pause things ---
    private bool pauseReset = true;
    //private bool paused = false;

    // --- Player things ---
    [SerializeField]
    private int maxPlayerHealth = 10;
    private int playerHealth;
    private int score = 0;
    private PlayerScript player;

    // --- Camera ---
    [SerializeField]
    private float MissTrauma = 4f;
    [SerializeField]
    private float KillTrauma = 2f;
    CameraScript camScript;

    // --- Game Time ---
    private float gameTime = 0f;
    [SerializeField]
    private int gameDuration = 120;  // How long a game lasts in seconds
    [SerializeField]
    private int pregameDuration = 4;  // Time in seconds before spawning the first enemy.
    private bool pregame = true;  // Are we in the pregame?

    // --- Enemies and spawning ---
    [SerializeField][Tooltip("Index is used to determine the difficulty of enemy")]
    private GameObject[] enemyPrefabs;
    private Transform[] spawnPoints;
    private Combo[] allCombos;
    private List<GameObject> enemyPool = new List<GameObject>();
    public bool spawningEnemies = true;


    /**
     * ============  FUNCTIONS  ====================================================================================================
     */


    void Awake () {
        
        camScript = Camera.main.GetComponent<CameraScript>();
        allCombos = Resources.LoadAll<Combo>("Combos");
        player = PlayerScript.Player;
        spawnPoints = transform.GetChild(0).GetComponentsInChildren<Transform>();
        secondsPerBeat = 60f / bpm;
        uiScript.SetGameDuration(gameDuration, pregameDuration);
	}

    private void Start()
    {
        RestartGame();
    }


    private void Update()
    {
        if (gameState.Equals(GameState.SCORESCREEN))
            return;

        float start = Input.GetAxisRaw("Start");

        // Dev cheats
        float l3 = Input.GetAxisRaw("L3");
        float r3 = Input.GetAxisRaw("R3");

        if (l3 != 0 && r3 != 0)
            EndGame();

        #region pausing
        if (start != 0)
        {
            if (pauseReset)
            {
                pauseReset = false;
                switch (gameState)  // Check for scorescreen is already complete
                {
                    case GameState.PAUSED:
                        ResumeGame();
                        break;
                    case GameState.PLAYING:
                        PauseGame();
                        break;
                    default:
                        break;
                }
            }
        }
        else if (!pauseReset)  // Stop holding the start button causing repeated pause/unpause
        {
            if (start == 0)
            {
                pauseReset = true;
            }
        }
        #endregion

        #region Playing
        if (gameState.Equals(GameState.PLAYING))
        {
            gameTime += Time.deltaTime;
            if (pregame)
            {
                if (gameTime >= pregameDuration)
                {
                    pregame = false;
                    uiScript.SetTimerActive(true);
                }
            }
            else
            {
                if (gameTime >= gameDuration)
                {
                    EndGame();
                }
                // OnBeat & OnMeasure
                if (gameTime >= beat)
                {
                    beat = gameTime + secondsPerBeat;
                    // On Measure
                    if (beatCount == 0)
                    {
                        if (spawningEnemies)
                            SpawnEnemy();
                        if (OnMeasure != null)
                        {
                            OnMeasure(); // on measure is where we would like to increment the difficulty.
                        }
                    }
                    // On Beat
                    if (OnBeat != null)
                    {
                        OnBeat();  // on beat is where the game events will occur, such as enemies exploding or 
                    }

                    beatCount = (beatCount + 1) % (timeSignature);  // Increment the beat counter, but have it wrap within the time signature
                }


            }
            uiScript.SetTimerText(gameTime);
        }
        #endregion
    }


    #region Enemy Spawning
    /// <summary>
    /// Spawns an enemy at a random spawn exculding the closest one to the player's current position.
    /// </summary>
    private void SpawnEnemy()
    {
        // Choose the spawn point
        List<Transform> possSpawns = new List<Transform>(spawnPoints); // fix this
        possSpawns.Remove(FindClosestSpawnToPlayer());
        Vector3 spawnPosition = possSpawns[(int)Random.Range(0, possSpawns.Count)].position;
        // Choose the enemy type
        GameObject enemy = enemyPrefabs[(int)Random.Range(0, enemyPrefabs.Length)];
        // Spawn the enemy
        GameObject newEnemy = Instantiate(enemy, spawnPosition, Quaternion.identity);
        // Set the combo
        newEnemy.GetComponentInChildren<DamageScript>().combo = SelectRandomCombo();
        // Add to the collection
        enemyPool.Add(newEnemy);
    }

    /// <summary>
    /// Returns the transform of the spawn point that is closest to the player
    /// </summary>
    /// <returns></returns>
    private Transform FindClosestSpawnToPlayer()
    {
        float distance = float.MaxValue;
        Transform closestPoint = spawnPoints[0];
        foreach (Transform t in spawnPoints)
        {
            float spawnDist = (t.position - player.transform.position).sqrMagnitude;
            if (spawnDist < distance)
            {
                distance = spawnDist;
                closestPoint = t;
            } 
        }
        return closestPoint;
    }

    /// <summary>
    /// Returns a random combo to use.
    /// </summary>
    /// <returns></returns>
    private Combo SelectRandomCombo()
    {
        int index = 0;
        index = Random.Range(0, allCombos.Length);
        // Change to weight the randomness by difficulty and combo length
        return allCombos[index];
    }
    #endregion

    #region Game Control


    /// <summary>
    /// Starts a new game.
    /// </summary>
    public void RestartGame()
    {
        audioOutput.SetActive(false); // set false first so that the music is actually restarted.
        audioOutput.SetActive(true);
        gameState = GameState.PLAYING;
        pregame = true;
        spawningEnemies = true;
        // Enemies
        foreach (GameObject g in enemyPool)  // change this when we implement enemy pooling, just .setactive(false);
        {
            Destroy(g);
        }
        enemyPool.Clear();
        ResumeGame();

        // Player
        player.ResetPlayer();
        uiScript.ResetComboIcons();
        playerHealth = maxPlayerHealth;
        uiScript.UpdateHealthBar(playerHealth);
        // Timer
        gameTime = 0;
        beat = 0;
        beatCount = 0;
        uiScript.SetTimerText(gameTime);
        uiScript.SetTimerActive(false);
        // Score
        score = 0;
        uiScript.SetScoreText(score);
        //Boost
        SetBoost(0);
        //Delegates
        OnBeat = null;
        OnMeasure = null;
    }


    /// <summary>
    /// Ends the game, transferring to the score screen and revealing replay UI options.
    /// </summary>
    public void EndGame()
    {
        gameState = GameState.SCORESCREEN;
        spawningEnemies = false;
        audioOutput.SetActive(false);
        player.SetVulnerability(false);
        switchScript.SelectPanel("ScorePanel");
        uiScript.SetScorescreenScore(score);
    }

    /// <summary>
    /// Deals damage to the player
    /// </summary>
    /// <param name="damage">Amount of damage to be dealt</param>
    public void DealDamage(int damage)
    {
        playerHealth -= damage;
        UpdateHealthBar();
        CheckDeath();
    }

    /// <summary>
    /// Checks if the player is dead. If they are end the game.
    /// </summary>
    private void CheckDeath()
    {
        if (playerHealth <= 0)
        {
            EndGame();
        }
    }

    /// <summary>
    /// Shakes the screen based on the event type given.
    /// </summary>
    /// <param name="hitType"></param>
    public void ScreenShake(HitType hitType)
    {
        switch (hitType)
        {
            case HitType.Miss:
                camScript.InduceTrauma(MissTrauma);
                break;
            case HitType.Kill:
                camScript.InduceTrauma(KillTrauma);
                break;
            default:
                return;
        }
    }
    #endregion



    #region Menu & UI
    // ------ Buttons ------

    /// <summary>
    /// Sets the combo to be displayed by the UI.
    /// </summary>
    /// <param name="target">The target whose combo to be displayed.</param>
    public void SetButtonHUD(Attackable target)
    {
        if (target == null)
        {
            uiScript.ResetComboIcons();
        }
        else
        {
            uiScript.SetCombo(target.combo);
            uiScript.SetButtonIcons(target.bIcons);
            UpdateButtonHUD(target.GetComboIndex());
        }
    }

    /// <summary>
    /// Updates the index tracker of the displayed combo.
    /// </summary>
    /// <param name="index"></param>
    public void UpdateButtonHUD(int index)
    {
        uiScript.UpdateComboIcons(index);
    }

    // ------ Health ------

    public void UpdateHealthBar()
    {
        uiScript.UpdateHealthBar(playerHealth);
    }

    // ------ Score   ------
    /// <summary>
    /// Adds value to the score and updates the UI
    /// </summary>
    /// <param name="value"></param>
    public void AddScore(int value)
    {
        score += value;
        uiScript.SetScoreText(score);
    }

    /// <summary>
    /// Passes the boost percentage to the uiscript.
    /// </summary>
    /// <param name="boost">The boost percentage remaining.</param>
    public void SetBoost(float boost)
    {
        uiScript.SetBoostValue(boost);
    }
    
    /// <summary>
    /// Pauses the gameobjects in the scene by toggling the kinematic property of their rigidbodies.
    /// </summary>
    /// <param name="paused">True if the is game paused.</param>
    private void SetPause(bool paused)
    {
        foreach (GameObject g in enemyPool)
        {
            Rigidbody[] bodies = g.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody r in bodies)
            {
                r.isKinematic = paused;
            }
        }
        player.GetComponentInChildren<Rigidbody>().isKinematic = paused;
    }
    
    // ------ Pausing ------

    /// <summary>
    /// Pauses the game, timescale is set to 0 so animations and such cannot be used.
    /// Displays the score and time in the pause menu.
    /// </summary>
    public void PauseGame()
    {
        gameState = GameState.PAUSED;
        SetPause(true);
        
        // Set pause score and time
        uiScript.SetTimerActive(false);
        switchScript.SelectPanel("PausePanel");
        uiScript.SetScoreText(score);
        uiScript.SetTimerText(gameTime);  // Technically don't need this as it is called in Update, but if that code changes we don't want it to break.
    }

    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void ResumeGame()
    {
        uiScript.SetTimerActive(true);
        switchScript.SelectPanel("GamePanel");
        gameState = GameState.PLAYING;
        SetPause(false);
    }

    /// <summary>
    /// Changes the game scene to the mainmenu.
    /// </summary>
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

}

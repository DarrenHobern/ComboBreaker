using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIScript : MonoBehaviour {

    // Panels
    [SerializeField]
    private GameObject pausePanel;
    [SerializeField]
    private GameObject gamePanel;
    [SerializeField]
    private GameObject scorePanel;


    // Timers
    [SerializeField]
    private Text gameTimerText;
    [SerializeField]
    private Text pauseTimerText;
    [SerializeField]
    private Color activeTimeColour;
    [SerializeField]
    private Color inactiveTimeColour;
    private int gameDuration;
    private int pregameDuration;
    private string minutes;
    private string seconds;
    
    // Scores
    [SerializeField]
    private Text pauseScoreText;
    [SerializeField]
    private Text gameScoreText;

    // Boost
    [SerializeField]
    private Slider boostSlider;

    // Scripts
    [SerializeField]
    private PanelSwitchScript switchScript;
    [SerializeField]
    private HealthDisplayScript healthScript;
    [SerializeField]
    private ComboDisplayScript comboScript;
    [SerializeField]
    private HighscoreDisplayScript scoreScript;
    

    private void Awake()
    {
        switchScript.EnableDefault();
    }

    /// <summary>
    /// Chnage the colour of the game timer text to either active or inactive.
    /// </summary>
    /// <param name="active"></param>
    public void SetTimerActive(bool active)
    {
        if (active)
            gameTimerText.color = activeTimeColour;
        else
            gameTimerText.color = inactiveTimeColour;
    }

    /// <summary>
    /// Sets either the game timer or the pause timer to the given time depending on which panel is currently active.
    /// </summary>
    /// <param name="gameTime"></param>
    public void SetTimerText(float gameTime)
    {
        float time = (gameDuration+pregameDuration) - gameTime;
        minutes = Mathf.Floor(time / 60f).ToString("00");
        seconds = ((int)time % 60).ToString("00");
        string text = string.Format("{0:00}:{1:00}", minutes, seconds);
        if (gamePanel.activeInHierarchy)
        {
            gameTimerText.text = text;
        }
        else if (pausePanel.activeInHierarchy)
        {
            pauseTimerText.text = text;
        }
    }

    /// <summary>
    /// Sets the game duration and the pregame duration.
    /// </summary>
    /// <param name="gameDuration">The total duration of the game.</param>
    /// <param name="pregameDuration">The total duration of the pregame.</param>
    public void SetGameDuration(int gameDuration, int pregameDuration)
    {
        this.gameDuration = gameDuration;
        this.pregameDuration = pregameDuration;
    }

    #region Score
    public void SetScoreText(int score)
    {
        string text = score.ToString("000;-000");
        if (gamePanel.activeInHierarchy)
            gameScoreText.text = text;
        else if (pausePanel.activeInHierarchy)
            pauseScoreText.text = text;

        // Add fancy effects when score hits thresholds **
    }

    public void SetScorescreenScore(int score)
    {
        scoreScript.SetPlayerScore(score);
    }
    #endregion

    /// <summary>
    /// Sets the value of the boost slider from the given percentage of boost remaining.
    /// </summary>
    /// <param name="boost">The percentage of boost remaining.</param>
    public void SetBoostValue(float boost)
    {
        boostSlider.value = boost;
    }

    #region HealthDisplay
    /// <summary>
    /// Passes the new health to the health display bar.
    /// </summary>
    /// <param name="health">The new health value.</param>
    public void UpdateHealthBar(int health)
    {
        healthScript.SetHealth(health);
    }
    #endregion

    #region ComboDisplay
    // Passes information to the combo display script

    public void ResetComboIcons()
    {
        comboScript.ClearComboIcons();
    }

    public void SetButtonIcons(ButtonIcons icons)
    {
        comboScript.SetButtonIcons(icons);
    }

    public void SetCombo(Combo combo)
    {
        comboScript.SetCombo(combo);
    }

    public void UpdateComboIcons(int index)
    {
        comboScript.UpdateComboIcons(index);
    }
    #endregion
}

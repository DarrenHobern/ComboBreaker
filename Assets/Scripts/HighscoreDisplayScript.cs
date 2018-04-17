using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HighscoreDisplayScript : MonoBehaviour {

    private const int NAME_LENGTH = 3;
    private const int ALPHABET_LENGTH = 26;
    private const int ASCII_OFFSET = 65;
    private const string UNDERSCORE = "_";
    private const char TAC = '-';
    private const char PAD = ' ';
    private const int NUMBER_OF_SCORES = 10;

    public Text instructionText;
    public Text scoreText;
    public Text nameText;
    public Text nameCursorText;
    public Text highscoreNamesText;
    public Text highscoreScoresText;


    public string enteringText = "Enter Your name:";
    public string defaultText = "Your Score Was:";

    private int playerScore = 0;
    private int nameIndex = 0;
    private int alphabetIndex = 0;


    private char[] nameArray;
    private string nameString;

    private bool enteringName = false;
    private bool buttonReset = true;  // Are we accepting another button press?

    private MenuScript menu;
    private HighscoreManagementScript scoreScript;

    private void Awake()
    {
        menu = GetComponent<MenuScript>();
        scoreScript = GetComponent<HighscoreManagementScript>();
    }

    private void OnEnable()
    {
        nameArray = new char[NAME_LENGTH] { TAC, TAC, TAC };
        enteringName = false;
        alphabetIndex = 0;
        nameIndex = 0;

        // Avoid the editor crash which occurs when trying to load from json files
        if (!Application.isEditor)
        {
            DisplayHighscores();
        }
        else
        {
            DEBUGLOAD();
        }
        // Test the players score against the highscores table
        if (TestHighscore())
        {
            menu.SetControl(false);
            enteringName = true;
            instructionText.text = enteringText;
            UpdatePlayerName(true);
            UpdateNameCursorDisplay();
        }

    }

    private void Update()
    {
        if (enteringName)
        {
            float cross = Input.GetAxisRaw("Cross");
            float dhor = Input.GetAxisRaw("DHorizontal");
            float triangle = Input.GetAxisRaw("Triangle");

            if (buttonReset)
            {
                // Alphabet Movement
                if (dhor > 0)  // alphabet right
                {
                    buttonReset = false;
                    MoveAlphabetCursor(1);
                }
                else if (dhor < 0)  // alphabet left
                {
                    buttonReset = false;
                    MoveAlphabetCursor(-1);
                }
                // Name Movement
                if (cross != 0)
                {
                    buttonReset = false;
                    IncrementNameCursor();
                }
                else if (triangle != 0)
                {
                    buttonReset = false;
                    DecrementNameCursor();
                }
            }
            else
            {
                if (cross == 0 && dhor == 0 && triangle == 0) // Must release all buttons before we accept another input. ie can't hold down X
                {
                    buttonReset = true;
                }
            }
        }
    }

    public void SetPlayerScore(int score)
    {
        playerScore = score;
        scoreText.text = string.Format("{0:000}", score);
    }

    private void SaveScores()
    {
        scoreScript.AddScore(nameText.text, playerScore);
        enteringName = false;
        instructionText.text = defaultText;
        menu.SetControl(true);
        if (!Application.isEditor)
            DisplayHighscores();
    }

    private void DEBUGLOAD()
    {
        string[] namesList = new string[] { "zzz", "aaa" };
        string[] scoresList = new string[] { "111", "222" };
        highscoreNamesText.text = string.Join("\n", namesList);
        highscoreScoresText.text = string.Join("\n", scoresList);
    }


    private void DisplayHighscores()
    {
        //scoreScript.LoadScores();
        string[] namesList = new string[HighscoreManagementScript.SCORES_TO_DISPLAY];
        string[] scoresList = new string[HighscoreManagementScript.SCORES_TO_DISPLAY];
        ScoreEntry[] entries = scoreScript.GetScores();
        for (int i = 0; i < entries.Length && i < HighscoreManagementScript.SCORES_TO_DISPLAY; i++)
        {
            namesList[i] = entries[i].name;
            scoresList[i] = entries[i].score.ToString();
        }
        highscoreNamesText.text = string.Join("\n", namesList);
        highscoreScoresText.text = string.Join("\n", scoresList);
    }

    /// <summary>
    /// Moves the alphabet cursor by amount, wraps
    /// </summary>
    /// <param name="amount"></param>
    private void MoveAlphabetCursor(int amount)
    {
        alphabetIndex = (alphabetIndex + amount) % ALPHABET_LENGTH;  // Increment
        if (alphabetIndex < 0)
            alphabetIndex += ALPHABET_LENGTH;
        UpdatePlayerName(true);  // Update the player name at name cursor index
    }

    /// <summary>
    /// Updates the underscore position in the _NameText ui element.
    /// </summary>
    //private void UpdateAlphabetCursorDisplay()
    //{
    //    alphabetCursorText.text = underscore.PadLeft(alphabetIndex+1, pad);  // +1 because PadLeft wants the total width of the string.
    //}

    private void UpdateNameCursorDisplay()
    {
        nameCursorText.text = UNDERSCORE.PadLeft(nameIndex+1, PAD);  // +1 because PadLeft wants the total width of the string.
    }

    /// <summary>
    /// Moves the name cursor one space to the right.
    /// If the end of the name is reached submit the name.
    /// </summary>
    private void IncrementNameCursor()
    {
        if (nameIndex >= NAME_LENGTH - 1)
        {
            SaveScores();
            return;
        }

        alphabetIndex = 0;
        ++nameIndex;
        UpdateNameCursorDisplay();
        UpdatePlayerName(true);
    }

    /// <summary>
    /// Moves the name cursor one space to the left, clearing the current character's value.
    /// </summary>
    private void DecrementNameCursor()
    {
        if (nameIndex <= 0)
            return;
        
        nameArray[nameIndex] = TAC;
        --nameIndex;
        alphabetIndex = (int)nameArray[nameIndex] - ASCII_OFFSET;
        UpdateNameCursorDisplay();
        UpdatePlayerName(false);
    }


    /// <summary>
    /// Sets the nameText to the value of the name array.
    /// If changed is true then update the nameArray before setting the text.
    /// </summary>
    /// <param name="changed">The character at alphabetIndex has changed and should be set.</param>
    private void UpdatePlayerName(bool changed)
    {
        if (changed)
            nameArray[nameIndex] = (char)(alphabetIndex + ASCII_OFFSET);
        nameText.text = string.Format("{0}:", (new string(nameArray)));
    }

    

    /// <summary>
    /// Compares the player's score to the lowest score on the highscores list to see if it makes it on.
    /// </summary>
    private bool TestHighscore()
    {
        // get the highscores from the json file 
        // sort them? hopefully they will be in order already
        // compare this score to the lowest one and if it is greater than, return true.
        return true;
        
    }
}

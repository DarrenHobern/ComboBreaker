using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ScoresCollection
{
    public ScoreEntry[] entryArray;
}

public class ScoreComparer : IComparer<ScoreEntry>
{
    public int Compare(ScoreEntry x, ScoreEntry y)
    {
        return y.score - x.score;  // Sorted descending
    }
}

[System.Serializable]
public struct ScoreEntry
{
    public string name;
    public int score;
}

public class HighscoreManagementScript : MonoBehaviour {
    public const int SCORES_TO_DISPLAY = 10;
    [HideInInspector]
    private ScoresCollection scores;
    private List<ScoreEntry> hsList = new List<ScoreEntry>();
    private ScoreComparer sc = new ScoreComparer();

    private void Awake()
    {
        scores = new ScoresCollection();
        LoadScores();
    }

    /// <summary>
    /// Adds a new 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="score"></param>
    public void AddScore(string name, int score)
    {
        ScoreEntry entry = new ScoreEntry
        {
            name = name,
            score = score
        };
        hsList.Add(entry);
        SaveScores();
    }

    public ScoreEntry[] GetScores()
    {
        if (hsList != null)
            return hsList.ToArray();
        return new ScoreEntry[0];
    }

    private void SaveScores()
    {
        // Change this to insertion sort
        hsList.Sort(sc);
        scores.entryArray = hsList.ToArray();
        string filepath = Application.dataPath + "/StreamingAssets/data.json";
        string json = JsonUtility.ToJson(scores);
        File.WriteAllText(filepath, json);
    }

    private void LoadScores()
    {
        string filepath = Application.dataPath + "/StreamingAssets/data.json";
        if (File.Exists(filepath)) {
            string json = File.ReadAllText(filepath);
            scores = JsonUtility.FromJson<ScoresCollection>(json);
            hsList = new List<ScoreEntry>(scores.entryArray);
        }
    }
}

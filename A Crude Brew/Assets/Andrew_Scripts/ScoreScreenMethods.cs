using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

public struct HighScore : IComparable<HighScore>
{
    public string name;
    public int score;

    public HighScore(string name, int score)
    {
        this.name = name;
        this.score = score;
    }

    public int CompareTo(HighScore other)
    {
        int compValue = score - other.score;
        if (compValue == 0)
            compValue = name.CompareTo(other.name);

        return compValue;
    }
}

public class ScoreScreenMethods : MonoBehaviour
{
    public UnityEngine.Object sceneOnPlay;
    public UnityEngine.Object menuScene;

    private int score;
    private List<HighScore> scores;
    private List<GameObject> scoreObjs;
    private readonly string filePath = @"Assets\Andrew_Scripts\Scores\scores.csv";
    public GameObject listScorePrefab;
    public GameObject scoresPanel;
    public RectTransform panelTransform;

    private string nameToAdd;
    public GameObject nameForm;
    private InputField input;
    private Button submitButton;



    // Start is called before the first frame update
    void Start()
    {
        score = ScoreSystem.GetScore();
        scores = new List<HighScore>();
        scoreObjs = new List<GameObject>();
        scoresPanel = scoresPanel != null ? scoresPanel : GameObject.Find("Panel");
        panelTransform = scoresPanel.GetComponent<RectTransform>();
        ReadScores(filePath);

        nameForm = nameForm != null ? nameForm : GameObject.Find("Name Form");
        input = nameForm.GetComponentInChildren<InputField>();
        submitButton = nameForm.GetComponentInChildren<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // updates name from text field
    public void UpdateName(string name)
    {
        nameToAdd = name;
    }

    // adds score to list of scores and writes it to a file
    public void SubmitScore()
    {
        // if user didn't provide a name, don't do anything
        if (nameToAdd == null || nameToAdd.Length == 0)
            return;

        // add score
        scores.Add(new HighScore(nameToAdd, score));

        // handle sorting, and changing current score list
        RefreshScores();

        // write current list of scores to file
        WriteScores(filePath);

        // prevent user from double submitting their score by disabling the score saving inputs
        input.interactable = false;
        submitButton.interactable = false;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(sceneOnPlay.name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(menuScene.name);
    }

    // read scores from scores file
    public void ReadScores(string _path)
    {
        if (scores == null)
            scores = new List<HighScore>();

        StreamReader readStream = new StreamReader(_path);

        string line;

        while((line = readStream.ReadLine()) != null)
        {
            string[] csv = line.Split(',');
            scores.Add(new HighScore(csv[0], int.Parse(csv[1])));
        }

        readStream.Close();

        // organize scores and put them in the scene
        RefreshScores();
    }

    public void RefreshScores()
    {
        scores.Sort(new Comparison<HighScore>(
            // sorts in descending order for numbers, but alphabetical ascending if there's a tie
            (hs1, hs2) =>
            {
                int compValue = hs2.score - hs1.score;
                if (compValue == 0)
                    compValue = hs1.name.CompareTo(hs2.name);

                return compValue;
            }
        ));
        DeleteCurrentScores();
        PopulateScoresPanel();
    }

    // write scores to external files in csv format
    public void WriteScores(string _path)
    {
        if (scores == null)
            return;

        StreamWriter writeStream = new StreamWriter(_path);

        for(int i = 0; i < scores.Count; i++)
            writeStream.WriteLine($"{scores[i].name},{scores[i].score}");

        writeStream.Close();
    }


    // adds all the scores from the file into the scorespanel
    public void PopulateScoresPanel()
    {
        int count = Math.Min(scores.Count, 10); // scores panel has a max of ten scores, and shows less if scores list has less

        for(int i = 0; i < count; i++)
        {
            // create a new list score object
            scoreObjs.Add(
                Instantiate(
                    listScorePrefab,
                    Vector3.zero,
                    Quaternion.identity,
                    scoresPanel.transform
                )
            );

            // set position here instead, anchoredPosition is useful in this case for UI elements
            RectTransform itemTransform = scoreObjs[i].GetComponent<RectTransform>();
            itemTransform.anchoredPosition = new Vector2(0, -30 * i);

            // set text objects' text to the name of person and their score
            Text[] scoreObjTextElements = scoreObjs[i].GetComponentsInChildren<Text>();
            scoreObjTextElements[0].text = scores[i].name;
            scoreObjTextElements[1].text = $"{scores[i].score}";
        }
    }

    // deletes all current score objects in the scene
    public void DeleteCurrentScores()
    {
        for(int i = 0; i < scoresPanel.transform.childCount; i++)
            Destroy(scoresPanel.transform.GetChild(i).gameObject);

        scoreObjs.Clear();
    }
}

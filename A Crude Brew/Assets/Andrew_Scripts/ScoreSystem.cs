using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// attach this to score display
public class ScoreSystem : MonoBehaviour
{
    private int score;
    private Text text;

    public void AddScore(int score)
    {
        if (score < 0) return;
        score += score;

        text.text = $"Score: {score}";
        
    }

    public int GetScore()
    {
        return score;
    }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        text.text = "Score: 0";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

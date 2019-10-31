using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// attach this to score display
public class ScoreSystem : MonoBehaviour
{
    private static int score;
    private Text text;
    private Text scorePopupText; // reference to text component of the popup element
    // popup element says "+[insert score just added] here" every time player adds to their score

    // popup animation time fields
    private bool scoreAnimationRunning = false; // update loop checks this to see if it should lerp
    private float currentTimeInAnimation; // gets increased during update if scoreAnimationRunning is true
    public float fadeInTime = 0.1f; // amount of time that score popup takes to fade in - cannot be zero
    public float popupTime; // amount of time score popup stays up
    public float fadeOutTime = 0.1f; // amount of time score popup takes to fade out - cannot be zero
    private float totalAfterPopupTime; // calc'd total of fadeInTime + popupTime - in Start()
    private float totalAfterFadeOutTime; // calc'd total of fadeInTime + popupTime + fadeOutTime - in Start()

    // add to player's current total score and popup how much got added
    public void AddScore(int _score)
    {
        if (_score > 0) // only add scores, no score subtraction
        {
            score += _score;
            text.text = $"Score: {score}";
            PopUpScore(_score); // start up popup
        }
        
    }

    // sets text in popup and starts animation
    private void PopUpScore(int score)
    {
        scorePopupText.text = $"+{score}";
        scoreAnimationRunning = true;
        currentTimeInAnimation = 0;
    }

    // returns the player's current total score
    public static int GetScore()
    {
        return score;
    }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        text.text = "Score: 0";
        score = 0;

        scorePopupText = GetComponentsInChildren<Text>()[1];

        // calc'd here for small optimizations -> doesn't need to be calculated every frame
        totalAfterPopupTime = fadeInTime + popupTime;
        totalAfterFadeOutTime = fadeInTime + popupTime + fadeOutTime;
    }


    // helper method for setting alpha of added score popup, helps code readability lol
    private void SetPopupAlpha(float alpha)
    {
        // have to set the whole color, because color is a C# struct - cannot set individual fields
        scorePopupText.color = new Color(
            scorePopupText.color.r,
            scorePopupText.color.g,
            scorePopupText.color.b,
            alpha
            );
    }


    // Update is called once per frame
    void Update()
    {
        if (scoreAnimationRunning)
        {
            // add to timer
            currentTimeInAnimation += Time.deltaTime;

            // if currentTime is within the fade in range
            if (currentTimeInAnimation < fadeInTime)
                SetPopupAlpha(Mathf.Lerp(0.0f, 1.0f, currentTimeInAnimation / fadeInTime)); // fade in popup
            
            // if currentTime is within the popup staying up range
            else if (currentTimeInAnimation >= fadeInTime && currentTimeInAnimation < totalAfterPopupTime) // set popup alpha to 1 if not already 1
                SetPopupAlpha(1.0f);

            // if currentTime is within the fade out range
            else if (currentTimeInAnimation >= totalAfterPopupTime && currentTimeInAnimation < totalAfterFadeOutTime) 
                SetPopupAlpha(Mathf.Lerp(1.0f, 0.0f, (currentTimeInAnimation - totalAfterPopupTime) / fadeInTime)); // fade out popup

            else // if animation has completed
            {
                SetPopupAlpha(0.0f);  // set alpha of popup to 0 if it hasn't already gotten there
                scoreAnimationRunning = false; // set animation bool to false, prevents this from running next frame
            }

        }
    }
}

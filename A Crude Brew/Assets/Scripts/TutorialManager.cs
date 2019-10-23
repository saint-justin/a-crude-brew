using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    private int counter = 0;
    public List<Texture> images;
    public GameObject canvasImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // When the player presses D, cycle to the next slide
        if (Input.anyKeyDown)
        {
            counter++;
            if (counter < images.Count)
            {
                canvasImage.GetComponent<RawImage>().texture = images[counter];
            }
            else
            {
                SceneManager.LoadScene("justin_scene");
            }
        }
    }
}

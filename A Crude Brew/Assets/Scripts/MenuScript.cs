using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public Object sceneOnPlay;
    private bool optionsOpen = false;
    public GameObject options;

    private void Start()
    {
        options.SetActive(optionsOpen);
    }


    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneOnPlay.name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ToggleOptions()
    {
        optionsOpen = !optionsOpen;
        Debug.Log(optionsOpen);
        options.SetActive(optionsOpen);
    }
}

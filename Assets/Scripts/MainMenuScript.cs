using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    public GameObject main;
    public GameObject options;
    public GameObject howToPlay;
    public GameObject tutorial;
    bool howToPlayState = false;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, .15f, 0));

        if (howToPlayState && Input.anyKey)
        {
            howToPlay.SetActive(false);
            howToPlayState = false;
            main.SetActive(true);
        }
    }

    public void ChangeDifficulty(float difficulty)
    {
        PlayerPrefs.SetInt("Difficulty", (int)Mathf.Floor(difficulty));
        PlayerPrefs.Save();
    }

    public void ChangeQuality(bool value)
    {
        PlayerPrefs.SetInt("EnableHighQuality", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnClickMain(int i)
    {
        switch (i)
        {
            case 0:
                main.SetActive(false);
                tutorial.SetActive(true);
                break;
            case 1:
                main.SetActive(false);
                options.SetActive(true);
                break;
            case 2:
                main.SetActive(false);
                howToPlay.SetActive(true);
                howToPlayState = true;
                break;
            case 3:
                Application.Quit();
                break;
        }
    }

    public void OnClickOption(int i)
    {
        switch (i)
        {
            case 0:
                options.SetActive(false);
                main.SetActive(true);
                break;
        }
    }

    public void OnClickTutorial(int i)
    {
        switch (i)
        {
            case 0:
                PlayerPrefs.SetInt("ShouldTutorial", 1);
                break;
            case 1:
                PlayerPrefs.SetInt("ShouldTutorial", 0);
                break;
        }

        Debug.Log(i);
        SceneManager.LoadScene(1);
    }
}

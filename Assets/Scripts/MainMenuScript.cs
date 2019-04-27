using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    Transform transform;
    public GameObject main;
    public GameObject options;
    public GameObject howToPlay;
    bool howToPlayState = false;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
    }

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

    public void OnClickMain(int i)
    {
        switch (i)
        {
            case 0:
                SceneManager.LoadScene(1);
                break;
            case 1:
                options.SetActive(true);
                main.SetActive(false);
                break;
            case 2:
                howToPlay.SetActive(true);
                howToPlayState = true;
                main.SetActive(false);
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
}

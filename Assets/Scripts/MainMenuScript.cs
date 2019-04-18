using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    Transform transform;
    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, .15f, 0));
    }

    public void OnClick(int i)
    {
        switch (i)
        {
            case 0:
                SceneManager.LoadScene(1);
                break;
            case 1:
                break;
            case 2:
                Application.Quit();
                break;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class changeDifficultyScript : MonoBehaviour
{
    public Text text;
    // Start is called before the first frame update
    private void Awake()
    {
        gameObject.GetComponent<Slider>().value = PlayerPrefs.GetInt("Difficulty", 2);
    }

    private void Update()
    {
        text.text = gameObject.GetComponent<Slider>().value == 1 ? "In this difficulty the rythm will be at 75% speed" : "";
    }
}

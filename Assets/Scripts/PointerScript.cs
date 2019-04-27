using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointerScript : MonoBehaviour
{
    public bool isColliding;
    public string boxType;

    // Start is called before the first frame update
    void Start()
    {
        isColliding = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        isColliding = true;
        boxType = collision.tag;
        collision.gameObject.GetComponent<Image>().color = Color.blue;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isColliding = false;
        collision.gameObject.GetComponent<Image>().color = Color.red;
    }
}

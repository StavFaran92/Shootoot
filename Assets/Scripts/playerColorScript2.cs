using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerColorScript2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.blue;
    }
}

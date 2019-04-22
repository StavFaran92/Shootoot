using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerColorScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.red;// SetTexture(textureId, newTexture);
    }
}

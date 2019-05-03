using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pressBoxScript : MonoBehaviour
{
    float speed;
    bool canStart = false;

    private void Start()
    {
        canStart = true;
        speed = 15 * Screen.height / 1080;
    }
    // Update is called once per frame
    void Update()
    {
        if (canStart)
        {
            transform.position += new Vector3(0, -speed, 0) * 60 * Time.deltaTime;

            if (transform.position.y < -Screen.height)
            {
                ObjectPoolManager.Instance.Destroy(PoolType.beatBox, gameObject);
                canStart = false;
            }
        }
    }
}

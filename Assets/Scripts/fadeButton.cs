using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fadeButton : MonoBehaviour
{
    Button parent;
    public Canvas canvas;
    public Image pressBoxAnim;

    // Start is called before the first frame update
    void Start()
    {
        parent = GetComponentInParent<Button>();
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    if (parent)
    //        ChangeColorAlpha(parent.IsInteractable() ? 1 : .3f);
    //}

    public void IsPointerDown()
    {
        Image anim = Instantiate(pressBoxAnim, transform.position, transform.rotation);
        anim.transform.SetParent(canvas.transform);
        StartCoroutine("ButtonPressAnimActivate", anim);
    }

    IEnumerator ButtonPressAnimActivate(Image anim)
    {
        Vector3 scale = anim.transform.localScale * 2.5f;
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            anim.rectTransform.localScale = scale + new Vector3(i, i, 0) * 2;
            ChangeColorAlpha(1 - i, anim);
            yield return new WaitForEndOfFrame();
        }

        Destroy(anim.gameObject);
    }

    void ChangeColorAlpha(float a, Image anim)
    {
        Color tempColor = anim.color;
        tempColor.a = a;
        anim.color = tempColor;
    }
}

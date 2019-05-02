using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fadeButton : MonoBehaviour
{
    Button parent;
    public Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        parent = GetComponentInParent<Button>();
    }

    public void IsPointerDown(Color color)
    {
        GameObject instance = ObjectPoolManager.Instance.Spawn(PoolType.buttonPushAnim, transform.position);
        Image anim = instance.GetComponent<Image>();
        anim.rectTransform.localScale = Vector3.one * 2.5f;
        anim.transform.SetParent(canvas.transform);
        anim.color = color;
        StartCoroutine(ButtonPressAnimActivate(anim, instance) );
    }

    IEnumerator ButtonPressAnimActivate(Image anim, GameObject instance)
    {
        Vector3 scale = anim.transform.localScale;
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            anim.rectTransform.localScale = scale + new Vector3(i, i, 0) * 2;
            ChangeColorAlpha(1 - i, anim);
            yield return new WaitForEndOfFrame();
        }

        ObjectPoolManager.Instance.Destroy(PoolType.buttonPushAnim, instance);
    }

    void ChangeColorAlpha(float a, Image anim)
    {
        Color tempColor = anim.color;
        tempColor.a = a;
        anim.color = tempColor;
    }
}

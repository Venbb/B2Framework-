using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSharpCallbackListener : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ScrollCellIndex(int idx)
    {
        string name = "Cell " + idx.ToString();
        var text = transform.GetChild(0).GetComponent<Text>();
        var image = gameObject.GetComponent<Image>();
        if (text != null)
        {
            text.text = name;
        }
        if (image != null)
        {
            image.color = Rainbow(idx / 50.0f);
        }
        gameObject.name = name;
    }

    public static Color Rainbow(float progress)
    {
        progress = Mathf.Clamp01(progress);
        float r = 0.0f;
        float g = 0.0f;
        float b = 0.0f;
        int i = (int)(progress * 6);
        float f = progress * 6.0f - i;
        float q = 1 - f;

        switch (i % 6)
        {
            case 0:
                r = 1;
                g = f;
                b = 0;
                break;
            case 1:
                r = q;
                g = 1;
                b = 0;
                break;
            case 2:
                r = 0;
                g = 1;
                b = f;
                break;
            case 3:
                r = 0;
                g = q;
                b = 1;
                break;
            case 4:
                r = f;
                g = 0;
                b = 1;
                break;
            case 5:
                r = 1;
                g = 0;
                b = q;
                break;
        }
        return new Color(r, g, b);
    }
}

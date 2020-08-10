using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReddotEditItem : MonoBehaviour
{
    public bool shuldPassParam = true;
    public string luaScriptName = "";
    public string luaFunctionName = "";
    public List<string> paramList;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.parent.gameObject.GetComponent<Canvas>() == null)
        {
            Debug.DrawLine(transform.position + new Vector3(0, gameObject.GetComponent<RectTransform>().rect.height / 2, 0), 
                transform.parent.position - new Vector3(0, transform.parent.gameObject.GetComponent<RectTransform>().rect.height / 2, 0), Color.red);
        }
    }
}

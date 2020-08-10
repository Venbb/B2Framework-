using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class TypewriterEffect : MonoBehaviour
{
    public float speed = 0.2f;

    private string words;
    private bool isActive = false;
    private float timer;
    private int currentPos = 0;
    private Action EndCallback = null;
    private Text myText;

    void Start()
    {
        timer = 0;
        speed = Mathf.Min(0.2f, speed);
        myText = GetComponent<Text>();
        myText.text = "";
    }

    void Update()
    {
        if(isActive)
        {
            OnStartWriter();
        }
    }

    void OnStartWriter()
    {
        timer += Time.deltaTime;
        if (timer >= speed)
        {
            timer = 0;
            currentPos++;
            myText.text = words.Substring(0, currentPos);

            if (currentPos >= words.Length)
            {
                Finish();
            }
        }
    }

    public void Finish()
    {
        isActive = false;
        timer = 0;
        currentPos = 0;
        myText.text = words;
        EndCallback?.Invoke();
    }

    public void SetText(string s)
    {
        words = s;
        timer = 0;
        currentPos = 0;
        isActive = true;
    }

    public void BindEndCallback(Action action)
    {
        EndCallback = action;
    }
}
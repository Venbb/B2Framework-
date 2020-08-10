using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationHelper : MonoBehaviour
{
    Dictionary<string, List<Action>> AnimationCallbacks = new Dictionary<string, List<Action>>();
    private float animTime = 0;
    private bool paused = false;
    void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!paused)
        {
            animTime = GetNowPlayTime(false);
        }
    }

    public void AddListener(string key, Action action)
    {
        if (!AnimationCallbacks.ContainsKey(key))
        {
            AnimationCallbacks[key] = new List<Action>();
        }
        AnimationCallbacks[key].Add(action);
    }

    public void OnAnimationCallback(string key)
    {
        if(AnimationCallbacks.ContainsKey(key))
        {
            if(AnimationCallbacks[key] != null)
            {
                for (int i = 0; i < AnimationCallbacks[key].Count; ++i)
                {
                    AnimationCallbacks[key][i]();
                }
            }
        }
    }

    public float GetNowPlayTime(bool getRecordedTime)
    {
        if(getRecordedTime)
        {
            return this.animTime;
        }

        float t = 0;
        Animation anim = GetComponent<Animation>();
        if(anim != null)
        {
            t = anim[anim.clip.name].time;
        }
        return t;
    }

    public void Pause()
    {
        paused = true;
        animTime = GetNowPlayTime(false);
    }

    public void Resume()
    {
        Animation anim = GetComponent<Animation>();
        if (anim != null)
        {
            anim[anim.clip.name].time = animTime;
            anim.Play();
        }
    }
}

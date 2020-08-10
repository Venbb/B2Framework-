using UnityEngine;
using System.Collections;

public class ParticleAutoDestruction : MonoBehaviour
{
    private ParticleSystem[] particleSystems;
    private float totalTime = 0;
    void Start()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        totalTime += Time.deltaTime;
        bool allStopped = true;

        foreach (ParticleSystem ps in particleSystems)
        {
            if(ps.main.duration <= totalTime)
            {
                allStopped = true;
                break;
            }
            if (!ps.isStopped)
            {
                allStopped = false;
            }
        }

        if (allStopped)
            GameObject.Destroy(gameObject);
    }
}
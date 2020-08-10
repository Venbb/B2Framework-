using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamTag {
    team1,
    team2
}

public class Bullet : MonoBehaviour
{
    public float speed = 30;
    public float damage = 1;
    public float fireRange = 1000;
    public TeamTag team = TeamTag.team1;
    public int IgnoreId = 0;
    public bool isIgnoreTeam = false;
    public ParticleSystem parHit;

    private Vector3 startPosition = new Vector3(0, 0, 0);

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {

        Vector3 dir = transform.forward;
        transform.position += dir * speed * Time.deltaTime;

        
        if (Vector3.Distance(startPosition, transform.position) > fireRange)
        {
            GameObject.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SpaceCraft sc = other.gameObject.GetComponent<SpaceCraft>();
        Bullet blt = other.gameObject.GetComponent<Bullet>();
        if (sc != null && sc.id == IgnoreId)
        {
            return;
        }
        if(blt != null && blt.IgnoreId == this.IgnoreId)
        {
            return;
        }
        if (isIgnoreTeam && sc.team == team)
        {
            return;
        }

        ParticleSystem par = Instantiate<ParticleSystem>(parHit);
        par.transform.position = transform.position;
        par.Play();
        GameObject.Destroy(gameObject);
    }
}

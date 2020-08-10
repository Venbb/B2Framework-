using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceCraftEnemyAI : MonoBehaviour
{
    public GameObject target;
    public bool AIAttack = true;
    public bool AIMove = true;
    public bool AIAim = true;
    float totalTime = 0;
    bool moved = false;
    int fireCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        totalTime += Time.deltaTime;
        if (AIAim && target != null)
            Aim();
        if (AIAttack && target != null)
            Attack();
        if (AIMove && target != null)
            Move();
    }

    public void Aim()
    {
        float turnTime = (360 / gameObject.GetComponent<SpaceCraft>().turnSpeed);
        Vector3 v = Vector3.RotateTowards(transform.forward, target.transform.position - transform.position, Mathf.PI / turnTime * Time.deltaTime, 0);
        transform.rotation = Quaternion.LookRotation(v);
    }

    public void Attack()
    {
        float angle = Vector3.Angle(transform.forward, (target.transform.position - transform.position).normalized);
        if(angle < 5)
        {
            //这里不是每5个发一个，因为发射有时间间隔，有的是空调用。所以发射子弹种类不一定
            ++fireCount;
            if(fireCount >= 5)
            {
                fireCount = 0;
                gameObject.GetComponent<SpaceCraft>().Fire(2);
            }
            else
            {
                gameObject.GetComponent<SpaceCraft>().Fire(1);
            }
        }
    }

    public void Move()
    {
        if(!moved)
        {
            gameObject.GetComponent<SpaceCraft>().engineForward = true;
            moved = true;
        }
    }
}

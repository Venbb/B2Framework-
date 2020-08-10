using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleEnemyCreator : MonoBehaviour
{
    public GameObject enemy;
    public GameObject player;
    // Start is called before the first frame update
    public float createTime = 20f;
    public float totalTime = 20f;
    public int enemyCount = 0;
    public int maxEnemyCount = 10;
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 vector = new Vector2(Input.mousePosition.x / ((float)Screen.width), Input.mousePosition.y / ((float)Screen.height));

        if (maxEnemyCount > enemyCount)
        {
            totalTime += Time.deltaTime;
            if(totalTime > createTime)
            {
                totalTime = 0;
                enemyCount += 1;
                GameObject go = Instantiate<GameObject>(enemy);
                go.GetComponent<SpaceCraftEnemyAI>().target = player;
                go.transform.position = new Vector3(500, 80, 500);
            }
        }
    }

    public void CountDead(GameObject go)
    {
        if(go.GetComponent<SpaceCraftEnemyAI>() != null)
        {
            //中间的机器死了就是负数，可能有一波敌人很多，但是不改了
            --enemyCount;
            Debug.Log("enemyCount" + enemyCount);
        }
    }
}

using System;
using B2Framework.Unity;
using UnityEngine;

public class SpaceCraft : MonoBehaviour
{
    public int id = 0;
    public float speed = 0;
    public float maxSpeed = 30;
    public float enginePower = 6f;
    public float resistance = 3f;
    public float turnSpeed = 20;
    public float rotateSpeed = 2;
    public float fireSpeed = 0.3f;
    public float addFireFlySpeed = 0;
    public TeamTag team = TeamTag.team1;
    public float fireRange = 1000;
    public float blood = 10;
    public float totalBlood = 10;
    public bool isPlayer = false;
    public bool ignoreHit = false;

    public GameObject bullet1;
    public GameObject bullet2;

    public float bulletOffectX1 = 0.5f;
    public float bulletOffectY1 = 0.2f;
    public float bulletOffectZ1 = 0f;
    public float bulletScaleX1 = 1;
    public float bulletScaleY1 = 1;
    public float bulletScaleZ1 = 1;

    public float bulletOffectX2 = 0.5f;
    public float bulletOffectY2 = 0.2f;
    public float bulletOffectZ2 = 0f;
    public float bulletScaleX2 = 1;
    public float bulletScaleY2 = 1;
    public float bulletScaleZ2 = 1;

    public ParticleSystem explorer;
    public bool engineForward = false;

    public bool isBoss = false;

    bool engineBack = false;
    bool turnLeft = false;
    bool turnRight = false;
    bool isFire1 = false;
    bool isFire2 = false;
    bool isDead = false;
    float countTime = 0;
    Vector2 mouseOldPosition = Vector2.zero;
    Vector2 centerPosition;
    Vector2 mousePosition;
    CraftBlood cb;
    // Start is called before the first frame update
    void Start()
    {
        centerPosition = new Vector2(Screen.width / 2, Screen.height / 2);
        cb = new CraftBlood();
        cb.nowblood = blood;
        cb.maxblood = blood;
        if (isPlayer)
        {
            The.LuaMgr.Send("craftblood", JsonUtility.ToJson(cb));
        }
    }

    // Update is called once per frame
    void Update()
    {
        countTime += Time.deltaTime;
        if (countTime > 10)
        {
            countTime = 10;
        }
        if (isPlayer)
        {
            CheckMouseMove();
            CheckMouseClick();
            CheckKeyboard();
        }
        move();
    }

    public void CheckMouseMove()
    {
        if (gameObject != null)
        {
            mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 mouseDelta = mousePosition - mouseOldPosition;
            mouseOldPosition = mousePosition;
            Vector2 dir = mousePosition - centerPosition;
            float rotateSpeedX = Mathf.Sin(Mathf.Min(Mathf.Abs(dir.x * 2 / Screen.width), 1) * Mathf.PI / 2) * rotateSpeed;
            float rotateSpeedY = Mathf.Sin(Mathf.Min(Mathf.Abs(dir.y * 2 / Screen.height), 1) * Mathf.PI / 2) * rotateSpeed;
            dir = dir.normalized;
            gameObject.transform.Rotate(new Vector3(1, 0, 0), -dir.y * rotateSpeedY);
            gameObject.transform.Rotate(new Vector3(0, 0, -1), dir.x * rotateSpeedX);
            gameObject.transform.Rotate(new Vector3(0, 1, 0), mouseDelta.x * turnSpeed / Screen.width * 20 * Time.deltaTime);
        }
    }

    public void CheckMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isFire1 = true;
        }
        if (Input.GetMouseButtonDown(1))
        {
            isFire2 = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isFire1 = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isFire2 = false;
        }
        if (isFire1)
        {
            Fire(1);
        }
        if (isFire2)
        {
            Fire(2);
        }
    }

    public void CheckKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            engineForward = true;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            engineForward = false;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            engineBack = true;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            engineBack = false;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            turnLeft = true;
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            turnLeft = false;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            turnRight = true;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            turnRight = false;
        }
    }

    public void move()
    {
        if (engineForward)
        {
            speed += enginePower * Time.deltaTime;
            if (speed > maxSpeed)
                speed = maxSpeed;
        }
        else if (engineBack)
        {
            speed -= enginePower * Time.deltaTime;
            if (speed < -maxSpeed)
                speed = -maxSpeed;
        }
        else
        {
            if (speed > 0)
            {
                speed -= resistance * Time.deltaTime;
                if (speed < 0)
                {
                    speed = 0;
                }
            }
            else if (speed < 0)
            {
                speed += resistance * Time.deltaTime;
                if (speed > 0)
                {
                    speed = 0;
                }
            }
        }

        if (turnLeft)
        {
            gameObject.transform.Rotate(new Vector3(0, 1, 0), -turnSpeed * Time.deltaTime);
        }
        else if (turnRight)
        {
            gameObject.transform.Rotate(new Vector3(0, 1, 0), turnSpeed * Time.deltaTime);
        }

        Vector3 dir = transform.forward;
        transform.position += dir * speed * Time.deltaTime;
    }

    public void Dead()
    {
        if (isDead)
        {
            return;
        }
        isDead = true;
        if (isPlayer)
        {
            cb.nowblood = 0;
            The.LuaMgr.Send("craftblood", JsonUtility.ToJson(cb));
        }
        if(isBoss)
        {
            The.LuaMgr.Send("win", "");
        }
        GameObject.Find("LittleEnemyCreater").GetComponent<LittleEnemyCreator>().CountDead(gameObject);
        ParticleSystem par = Instantiate<ParticleSystem>(explorer);
        par.transform.position = transform.position;
        par.Play();
        Destroy(gameObject);
    }

    public void Fire(int tag)
    {
        if (countTime < fireSpeed)
        {
            return;
        }
        countTime = 0;
        GameObject bt = null;
        GameObject bullet;

        float offectX = 0;
        float offectY = 0;
        float offectZ = 0;
        float scaleX = 1;
        float scaleY = 1;
        float scaleZ = 1;

        if (tag == 1)
        {
            bt = bullet1;
            offectX = bulletOffectX1;
            offectY = bulletOffectY1;
            offectZ = bulletOffectZ1;
            scaleX = bulletScaleX1;
            scaleY = bulletScaleY1;
            scaleZ = bulletScaleZ1;
        }
        else if (tag == 2)
        {
            bt = bullet2;
            offectX = bulletOffectX2;
            offectY = bulletOffectY2;
            offectZ = bulletOffectZ2;
            scaleX = bulletScaleX2;
            scaleY = bulletScaleY2;
            scaleZ = bulletScaleZ2;
        }
        if (bt != null)
        {
            bullet = GameObject.Instantiate<GameObject>(bt);
            bullet.gameObject.SetActive(false);
            bullet.transform.position = new Vector3(transform.position.x - offectX, transform.position.y - offectY, transform.position.z + offectZ);
            bullet.transform.rotation = transform.rotation;
            bullet.GetComponent<Bullet>().IgnoreId = id;
            bullet.GetComponent<Bullet>().team = team;
            bullet.GetComponent<Bullet>().fireRange = fireRange;
            bullet.GetComponent<Bullet>().speed = bullet.GetComponent<Bullet>().speed + speed + addFireFlySpeed;
            bullet.transform.localScale = new Vector3(bullet.transform.localScale.x * scaleX, bullet.transform.localScale.y * scaleY, bullet.transform.localScale.z * scaleZ);
            bullet.SetActive(true);

            bullet = GameObject.Instantiate<GameObject>(bt);
            bullet.gameObject.SetActive(false);
            bullet.transform.position = new Vector3(transform.position.x + offectX, transform.position.y - offectY, transform.position.z + offectZ);
            bullet.transform.rotation = transform.rotation;
            bullet.GetComponent<Bullet>().IgnoreId = id;
            bullet.GetComponent<Bullet>().team = team;
            bullet.GetComponent<Bullet>().fireRange = fireRange;
            bullet.GetComponent<Bullet>().speed = bullet.GetComponent<Bullet>().speed + speed + addFireFlySpeed;
            bullet.transform.localScale = new Vector3(bullet.transform.localScale.x * scaleX, bullet.transform.localScale.y * scaleY, bullet.transform.localScale.z * scaleZ);
            bullet.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Bullet bullet = null;
        if (other != null && other.gameObject != null)
        {
            bullet = other.gameObject.GetComponent<Bullet>();
        }
        if (bullet != null)
        {
            if (!(bullet.IgnoreId == id || (bullet.isIgnoreTeam && bullet.team == team)))
            {
                blood -= bullet.damage;
                cb.nowblood = blood;
                if (isPlayer)
                {
                    The.LuaMgr.Send("craftblood", JsonUtility.ToJson(cb));
                }
                if (blood <= 0)
                {
                    Dead();
                }
            }
        }
        else
        {
            if(!ignoreHit)
            {
                Dead();
            }
        }
    }
}

[Serializable]
public class CraftBlood
{
    public float nowblood;
    public float maxblood;
}

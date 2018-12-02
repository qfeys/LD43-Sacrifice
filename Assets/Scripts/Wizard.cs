using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : MonoBehaviour {

    public float speed = 5.0f;
    public float jump = 5.0f;
    public float acc = 40.0f;

    public float health = 100;

    List<Ability> abilities;

    Ability abilityLeft;
    Ability abilityRight;
    Ability abilitySpace;

    float cooldownLeft;
    float cooldownRight;
    float cooldownSpace;
    bool shieldActive;

    Rigidbody2D myridg;
    GameObject shield;

    private void Awake()
    {
        myridg = GetComponent<Rigidbody2D>();
        shield = transform.Find("Shield").gameObject;
    }

    // Use this for initialization
    void Start () {
        ObjectPool op = GameObject.Find("ProjectileSpawner").GetComponent<ObjectPool>();
        abilityLeft = new Ability((Vector2 pos, Vector2 trgt) =>
        {
            GameObject go = op.GetNextObject();
            Rigidbody2D rgb = go.GetComponent<Rigidbody2D>();
            go.transform.position = pos;
            Vector2 dir = (trgt - pos).normalized;
            rgb.velocity = dir * go.GetComponent<Projectile>().velocity + myridg.velocity;
            health -= 2;
        });
        abilitySpace = new Ability((Vector2 pos, Vector2 trgt) =>
        {
            shieldActive = true;
            health -= Time.deltaTime * 2; // 2 per second
            shield.SetActive(true);
            Vector2 dir = (trgt - pos).normalized;
            shield.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 45);
        });
	}
	
	// Update is called once per frame
	void Update ()
    {
        float h_vel = myridg.velocity.x;
        if (Input.GetButton("Right"))
        {
            if (h_vel < speed)
            {
                h_vel += acc * Time.deltaTime;
                if (h_vel > speed)
                    h_vel = speed;
            }

        }
        else if (Input.GetButton("Left"))
        {
            if (h_vel > -speed)
            {
                h_vel -= acc * Time.deltaTime;
                if (h_vel < -speed)
                    h_vel = -speed;
            }
        } else
        {
            if (h_vel > 0)
            {
                h_vel -= acc / 2 * Time.deltaTime;
                if (h_vel < 0)
                    h_vel = 0;
            }
            else if (h_vel < 0)
            {
                h_vel += acc / 2 * Time.deltaTime;
                if (h_vel > 0)
                    h_vel = 0;
            }
        }
        myridg.velocity = new Vector2(h_vel, myridg.velocity.y);

        if (Input.GetButton("Up"))
        {
            if (myridg.IsTouchingLayers())
            {
                var points = new ContactPoint2D[2];
                Vector2 normal;
                if (2 == myridg.GetContacts(points))
                {
                    normal = (points[0].normal + points[1].normal) / 2;
                } else
                {
                    normal = points[0].normal;
                }
                myridg.velocity = new Vector2(myridg.velocity.x, jump / 2) + normal.normalized * (jump / 2);
            }
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetButtonDown("Fire1") && cooldownLeft <= 0)
        {
            abilityLeft.Fire(myridg.position, mousePos);
            cooldownLeft = abilityLeft.cooldown;
        }
        if (Input.GetButtonDown("Fire2") && cooldownLeft <= 0)
        {
            abilityRight.Fire(myridg.position, mousePos);
            cooldownLeft = abilityLeft.cooldown;
        }
        if (Input.GetButton("Fire3") && cooldownLeft <= 0)
        {
            abilitySpace.Fire(myridg.position, mousePos);
            cooldownLeft = abilityLeft.cooldown;
        }

        if (shieldActive == false)
            shield.SetActive(false);
        shieldActive = false;
    }

    internal void GetHit(float damage)
    {
        health -= damage;
    }

    internal void ShieldHit(float damage)
    {
        health -= damage / 10;
    }

    internal void AddHealth(float health)
    {
        this.health += health;
        if (this.health > 100)
            this.health = 100;
    }
}

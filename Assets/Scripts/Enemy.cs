using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float speed = 5.0f;
    public float jump = 5.0f;
    public float acc = 40.0f;

    public int projectiles = 0;
    float shotCooldown = 0;
    float timeBetweenShots = 1;

    enum Stance{DECIDING, CHARGING, SHOOTING, SHIELDING, SCOOTING }

    Stance stance = Stance.DECIDING;

    public Collider2D FrontBumper;
    public Collider2D BackBumper;
    bool isGrounded = false;

    Transform target;

    Rigidbody2D myridg;

    static ObjectPool projectileSpawner;
    static ObjectPool dropsSpawner;

    private void Awake()
    {
        if (dropsSpawner == null)
            dropsSpawner = GameObject.Find("DropsSpawner").GetComponent<ObjectPool>();
        if (projectileSpawner == null)
            projectileSpawner = GameObject.Find("EnemyProjectileSpawner").GetComponent<ObjectPool>();
        myridg = GetComponent<Rigidbody2D>();
    }

    // Use this for initialization
    void Start () {
        target = GameObject.Find("Wizard").transform;
	}
	
	// Update is called once per frame
	void Update () {

        int move = 0; // 1 = right, -1 = left
        switch(stance)
        {
        case Stance.DECIDING:
            if (projectiles != 0)
                stance = Stance.SHOOTING;
            else
                stance = Stance.CHARGING;
            break;

        case Stance.SHOOTING:
            shotCooldown -= Time.deltaTime;
            if (shotCooldown <= 0)    // try to shoot
            {
                var solution = CanYouHitTheWizard();
                if (solution != null)
                {
                    Shoot(solution.Item2);
                    shotCooldown = timeBetweenShots;
                    if (projectiles <= 0)
                        stance = Stance.CHARGING;
                    break;
                } else
                    stance = Stance.SCOOTING;
            }
            break;
        case Stance.SCOOTING:
        case Stance.CHARGING:
            if (target.position.x < transform.position.x)
                move = -1;
            else
                move = 1;
            break;
        }

        float h_vel = myridg.velocity.x;
        if (move == 1)
        {
            if (h_vel < speed)
            {
                h_vel += acc * Time.deltaTime;
                if (h_vel > speed)
                    h_vel = speed;
            }

        } else if (move == -1)
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
            } else if (h_vel < 0)
            {
                h_vel += acc / 2 * Time.deltaTime;
                if (h_vel > 0)
                    h_vel = 0;
            }
        }
        myridg.velocity = new Vector2(h_vel, myridg.velocity.y);

        if ((move == 1 && FrontBumper.IsTouchingLayers())|| (move == -1 && BackBumper.IsTouchingLayers()))
        {
            if (isGrounded)
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
        isGrounded = false;
    }

    private Tuple<float,float> CanYouHitTheWizard()
    {
        // Can you hit the wizard?
        float s = Projectile.BLOB_VELOCITY;
        float ss = s * s;
        float x = target.position.x - transform.position.x;
        float y = target.position.y - transform.position.y;
        float g = 9.81f;

        float root = ss * ss - g * (g * x * x + 2 * y * ss);
        if (root < 0)
            return null;

        float angle1 = Mathf.Atan2(ss - Mathf.Sqrt(root), g * x);   // Low angle
        float angle2 = Mathf.Atan2(ss + Mathf.Sqrt(root), g * x);   // High angle
        return new Tuple<float, float>(angle1, angle2);
    }

    private void Shoot(float angle)
    {
        GameObject go = projectileSpawner.GetNextObject();
        Rigidbody2D rgb = go.GetComponent<Rigidbody2D>();
        go.transform.position = transform.position;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        rgb.velocity = dir * go.GetComponent<Projectile>().velocity + myridg.velocity;
        projectiles--;
    }

    public void GetHit()
    {
        GameObject drop = dropsSpawner.GetNextObject();
        drop.transform.position = transform.position;
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Wizard"))
        {
            collision.collider.gameObject.GetComponent<Wizard>().GetHit(40);
            GetHit();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == 13)
            isGrounded = true;
    }
}

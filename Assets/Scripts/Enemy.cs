using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float speed = 5.0f;
    public float jump = 5.0f;
    public float acc = 40.0f;

    public Collider2D FrontBumper;
    public Collider2D BackBumper;

    Transform target;

    Rigidbody2D myridg;

    private void Awake()
    {
        myridg = GetComponent<Rigidbody2D>();
    }

    // Use this for initialization
    void Start () {
        target = GameObject.Find("Wizard").transform;
	}
	
	// Update is called once per frame
	void Update () {

        int move = 0; // 1 = right, -1 = left

        if (target.position.x < transform.position.x)
            move = -1;
        else
            move = 1;


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
    }

    public void GetHit()
    {
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
}

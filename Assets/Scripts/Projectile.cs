using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public enum projectileType { BLOB}
    projectileType type = projectileType.BLOB;

    public float lifetime = 2;
    public float damage = 10;
    float lifeLeft;

    public const float BLOB_VELOCITY = 10;

    public void Init(projectileType type)
    {
        this.type = type;
    }

    private void OnEnable()
    {
        lifeLeft = lifetime;
    }

    // Update is called once per frame
    void Update () {
        lifeLeft -= Time.deltaTime;
        if (lifeLeft < 0)
            EndOfLife();
	}

    public float velocity { get
        {
            switch (type)
            {
            case projectileType.BLOB:
                return BLOB_VELOCITY;
            }
            return -50;
        } }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            collision.collider.GetComponent<Enemy>().GetHit();
            EndOfLife();
        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Wizard"))
        {
            collision.collider.GetComponent<Wizard>().GetHit(damage);
            EndOfLife();
        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Shield"))
        {
            collision.collider.GetComponentInParent<Wizard>().ShieldHit(damage);
            EndOfLife();
        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("EnemyShield"))
        {
            collision.collider.GetComponentInParent<Enemy>().ShieldHit();
            EndOfLife();
        }
    }

    private void EndOfLife()
    {
        gameObject.SetActive(false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public enum projectileType { BLOB}
    projectileType type = projectileType.BLOB;

    public float lifetime;

    public void Init(projectileType type)
    {
        this.type = type;
    }

    private void OnEnable()
    {
        lifetime = 2;
    }

    // Update is called once per frame
    void Update () {
        lifetime -= Time.deltaTime;
        if (lifetime < 0)
            EndOfLife();
	}

    public float velocity { get
        {
            switch (type)
            {
            case projectileType.BLOB:
                return 10;
            }
            return -50;
        } }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            collision.collider.GetComponent<Enemy>().GetHit();
            EndOfLife();
        }
    }

    private void EndOfLife()
    {
        gameObject.SetActive(false);
    }
}

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
            gameObject.SetActive(false);
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
}

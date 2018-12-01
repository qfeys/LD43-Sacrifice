﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {


    ObjectPool terrainPool;

    List<Platform> platforms;

    const float MIN_PLATF_LENGTH = 2;
    const float MAX_PLATF_LENGTH = 8;
    const float MIN_PLATF_HEIGHT = 2;
    const float MAX_PLATF_HEIGHT = 4;


    void Awake()
    {
        terrainPool = GetComponent<ObjectPool>();
        Random.InitState(20);
    }

    // Use this for initialization
    void Start()
    {
        platforms = MakeFlatPath(new Vector2(-8, 0), 120);
        platforms.ForEach(p => p.Make(terrainPool));
    }

    [System.Obsolete]
    List<Platform> MakeHorizontalPath(Vector2 start, float length)
    {
        List<Platform> plfs = new List<Platform>();
        while(length > 0)
        {
            float x = Mathf.Round(Random.Range(MIN_PLATF_LENGTH, MAX_PLATF_LENGTH));
            float y = Mathf.Round(Random.Range(MIN_PLATF_HEIGHT, MAX_PLATF_HEIGHT));
            plfs.Add(new Platform(start, new Vector2(x, y)));
            float xx = Random.Range(0, 4);
            float yy = Random.Range(-1, 2);
            start += new Vector2(x + xx, yy);
            length -= (x + xx);
        }
        return plfs;
    }

    List<Platform> MakeFlatPath(Vector2 start, float length)
    {
        List<Platform> plfs = new List<Platform>();
        while (length > 0)
        {
            switch (Random.Range(0, 4))
            {
            case 0:     // flat land with a platform that sticks out (up or down)
                {
                    // There are 3 platforms, so determine the length of these platforms
                    int l1 = Random.Range(4, 10);
                    int l2 = Random.Range(4, 10);
                    int l3 = Random.Range(4, 10);
                    // Does the platform go up or down?
                    bool up = Random.value > 0.5f;
                    plfs.Add(new Platform(start, new Vector2(l1, up ? 2 : 3)));
                    plfs.Add(new Platform(start + new Vector2(l1 - 1, up ? 1 : -1), new Vector2(l2 + 1, up ? 4 : 2)));  // TODO: Extra random for asteathic positioning
                    plfs.Add(new Platform(start + new Vector2(l1 + l2, 0), new Vector2(l3, up ? 2 : 3)));
                    Debug.Log("Flat: " + l1 + l2 + l3 + up);
                    start += new Vector2(l1 + l2 + l3, 0);
                    length -= (l1 + l2 + l3);
                }
                break;
            case 1:     // a small pit you have to jump over, maybe with a hight difference
                {
                    // There are two platforms: at the bottom and on the other side. The starting platform is reused from the last template
                    // length of the pit and length of the landing
                    int l1 = Random.Range(1, 3);
                    int l2 = Random.Range(3, 8);
                    // up, flat or down?
                    int h = Random.Range(-1, 2);
                    plfs.Add(new Platform(start + new Vector2(0, Mathf.Min(-1, h - 1)), new Vector2(l1, 1)));
                    plfs.Add(new Platform(start + new Vector2(l1, h), new Vector2(l2, 3)));
                    Debug.Log("Small jump: " + l1 + l2 + h);
                    start += new Vector2(l1 + l2, h);
                    length -= (l1 + l2);
                }
                break;
            case 2:     // a wide pit you have to jump over
                {
                    // There are 3 platforms: one at the bottom, the landing and the recup. The recup is always 1 by 1. The hole is 2 deep.
                    // length of the pit and length of the landing
                    int l1 = Random.Range(3, 5);
                    int l2 = Random.Range(3, 8);
                    plfs.Add(new Platform(start + new Vector2(0, -2), new Vector2(l1, 2)));
                    plfs.Add(new Platform(start + new Vector2(0, -1), new Vector2(1, 1)));
                    plfs.Add(new Platform(start + new Vector2(l1, 0), new Vector2(l2, 3)));
                    Debug.Log("Big jump: " + l1 + l2);
                    start += new Vector2(l1 + l2, 0);
                    length -= (l1 + l2);
                }
                break;
            case 3:     // a wall you have to scale
                {
                    // 3 platforms: startpit, between (above start) and landing
                    // Length of the startpit, between and landing
                    int l1 = Random.Range(4, 8);
                    int l2 = Random.Range(2, l1 - 2);
                    int l3 = Random.Range(3, 5);
                    // startpit goes down
                    bool down = Random.value > 0.5f;
                    // Height of the wall and the between
                    int h1 = Random.Range(down ? 3 : 4, 6);
                    int h2 = Random.Range(down ? 2 : 3, h1);
                    plfs.Add(new Platform(start + new Vector2(0, down ? -1 : 0), new Vector2(l1, 2)));
                    plfs.Add(new Platform(start + new Vector2(l1 - l2 - 2, h2 + (down ? -1: 0)), new Vector2(l2, 1)));
                    plfs.Add(new Platform(start + new Vector2(l1, h1 + (down ? -1 : 0)), new Vector2(l3, h1 + 2)));
                    Debug.Log("Climb: " + l1 + l2 + l3 + down + h1 + h2);
                    start += new Vector2(l1 + l3, h1 + (down ? -1 : 0));
                    length -= (l1 + l3);
                }
                break;
            }
            if (Random.value > 0.75f)
                new Spawner(start + new Vector2(0, 1), 3, Spawner.EnemyType.CHARGER);
        }


        return plfs;
    }


    class Platform
    {
        /// <summary>
        /// Top left of the platform
        /// </summary>
        public Vector2 position;
        public Vector2 size;

        public Platform(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
        }
        public Platform(float posx, float posy, float sizex, float sizey)
        {
            this.position = new Vector2(posx, posy);
            this.size = new Vector2(sizex, sizey);
        }

        public void Make(ObjectPool terrainPool)
        {
            GameObject go = terrainPool.GetNextObject();
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.size = size;
            BoxCollider2D bc = go.GetComponent<BoxCollider2D>();
            bc.size = size;
            bc.offset = new Vector2(size.x / 2, -size.y / 2);
            go.transform.position = position;
        }
    }

    class Spawner
    {
        public Vector2 position;
        public int amount;
        public enum EnemyType { CHARGER, SHOOTER, SHIELD}
        public EnemyType type;
        public int difficulty;

        static ObjectPool enemyPool;

        public Spawner(Vector2 position, int amount, EnemyType type, int difficulty = 1)
        {
            if (enemyPool == null)
                enemyPool = GameObject.Find("EnemyPool").GetComponent<ObjectPool>();
            this.position = position; this.amount = amount; this.type = type; this.difficulty = difficulty;
            new GameObject("Spawner " + position.x).AddComponent<SpawnerGO>().parent = this;
        }


        class SpawnerGO : MonoBehaviour
        {
            const float DISTANCE_UNTILL_ACTIVATION = 25;
            const float TIME_TO_SPAWN_ALL = 8;

            public Spawner parent;
            Transform player;
            bool isactive = false;
            float timeUntillNext;
            float timeBetweenTwo;


            private void Awake()
            {
                player = GameObject.Find("Wizard").transform;
            }

            private void Update()
            {
                if (isactive == false)
                {
                    if ((player.position.x - parent.position.x) < DISTANCE_UNTILL_ACTIVATION)
                    {
                        isactive = true;
                        timeBetweenTwo = TIME_TO_SPAWN_ALL / parent.amount;
                    }
                } else
                {
                    timeUntillNext -= Time.deltaTime;
                    if(timeUntillNext < 0)
                    {
                        timeUntillNext = timeBetweenTwo;
                        GameObject enemy = enemyPool.GetNextObject();
                        enemy.transform.position = parent.position;
                        parent.amount--;
                        if (parent.amount <= 0)
                            Destroy(gameObject);
                    }
                }
            }
        }
    }

}

using System.Collections;
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
        platforms = MakeFlatPath(new Vector2(-8, 0), 80);
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
            switch (Random.Range(1, 3))
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

                break;
            }
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

}

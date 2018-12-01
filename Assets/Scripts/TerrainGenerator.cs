using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {


    ObjectPool terrainPool;

    List<Platform> platforms;

    const float MIN_PLATF_LENGTH = 2;
    const float MAX_PLATF_LENGTH = 8;
    const float MIN_PLATF_HEIGHT = 1;
    const float MAX_PLATF_HEIGHT = 4;


    void Awake()
    {
        terrainPool = GetComponent<ObjectPool>();
        Random.InitState(20);
    }

    // Use this for initialization
    void Start()
    {
        platforms = MakeHorizontalPath(new Vector2(-8, 0), 80);
        platforms.ForEach(p => p.Make(terrainPool));
    }

    List<Platform> MakeHorizontalPath(Vector2 start, float length)
    {
        List<Platform> plfs = new List<Platform>();
        while(length> 0)
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

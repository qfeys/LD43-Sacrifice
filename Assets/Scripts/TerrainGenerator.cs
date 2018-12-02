using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {


    ObjectPool terrainPool;
    public GameObject chest;

    List<Platform> platforms;

    delegate void Generator(ref Vector2 start, ref float length, List<Platform> plfs);
    enum Difficulty { V_EASY = 0, EASY = 1, MODERATE = 2, HARD = 3 }
    List<System.Tuple<Generator, Difficulty>> generators = new List<System.Tuple<Generator, Difficulty>>() {
            new System.Tuple<Generator, Difficulty>(FlatShort, Difficulty.V_EASY),
            new System.Tuple<Generator, Difficulty>(SmallPit, Difficulty.EASY),
            new System.Tuple<Generator, Difficulty>(WidePit, Difficulty.EASY),
            new System.Tuple<Generator, Difficulty>(ClimableWall, Difficulty.HARD),
            new System.Tuple<Generator, Difficulty>(NoReturnDrop, Difficulty.MODERATE),
            new System.Tuple<Generator, Difficulty>(StairsRight, Difficulty.MODERATE)
        };


    void Awake()
    {
        terrainPool = GetComponent<ObjectPool>();
        Random.InitState(20);
    }

    int currentLevel = 0;
    List<System.Tuple<int, Difficulty>> levels = new List<System.Tuple<int, Difficulty>>() {
        new System.Tuple<int, Difficulty>(70, Difficulty.V_EASY),
        new System.Tuple<int, Difficulty>(100, Difficulty.EASY),
        new System.Tuple<int, Difficulty>(200, Difficulty.V_EASY),
        new System.Tuple<int, Difficulty>(100, Difficulty.MODERATE),
        new System.Tuple<int, Difficulty>(200, Difficulty.EASY),
        new System.Tuple<int, Difficulty>(200, Difficulty.MODERATE),
        new System.Tuple<int, Difficulty>(100, Difficulty.HARD),
        new System.Tuple<int, Difficulty>(300, Difficulty.EASY),
        new System.Tuple<int, Difficulty>(200, Difficulty.HARD),
        new System.Tuple<int, Difficulty>(300, Difficulty.MODERATE)
    };

    public void GenerateNextLevel()
    {
        terrainPool.DeactivateAllObjects();

        platforms = MakeLevel(new Vector2(0, 0), levels[currentLevel].Item1, levels[currentLevel].Item2);
        platforms.ForEach(p => p.Make(terrainPool));
        currentLevel++;
        Debug.Log("Level is now: " + currentLevel);
    }

    public System.Tuple<string,string> InfoNextLevel()
    {
        switch (levels[currentLevel].Item2)
        {
        case Difficulty.V_EASY:
            return new System.Tuple<string, string>(levels[currentLevel].Item1.ToString(), "Very easy");
        case Difficulty.EASY:
            return new System.Tuple<string, string>(levels[currentLevel].Item1.ToString(), "Easy");
        case Difficulty.MODERATE:
            return new System.Tuple<string, string>(levels[currentLevel].Item1.ToString(), "Moderate");
        case Difficulty.HARD:
            return new System.Tuple<string, string>(levels[currentLevel].Item1.ToString(), "Hard");
        }
        throw new System.Exception("Bad Difficulty");
    }

    public int GetCurrentLevel()
    {
        return currentLevel - 1;
    }

    public void ResetLevel()
    {
        currentLevel = 0;
    }

    List<Platform> MakeLevel(Vector2 start, float length, Difficulty levelDifficulty)
    {
        List<Platform> plfs = new List<Platform>();
        StagingGround(ref start, ref length, plfs);

        int totalWeight = generators.Sum(tpl => GetDifficultyWeight(levelDifficulty, tpl.Item2));

        while(length > 0)
        {
            int randomNumber = Random.Range(0, totalWeight);

            Generator selectedGenerator = null;
            foreach (System.Tuple<Generator, Difficulty> generator in generators)
            {
                int generatorWeight = GetDifficultyWeight(levelDifficulty, generator.Item2);
                if (randomNumber <= generatorWeight)
                {
                    selectedGenerator = generator.Item1;
                    break;
                }
                randomNumber -= generatorWeight;
            }
            selectedGenerator(ref start, ref length, plfs);
            GenerateSpawner(start, levelDifficulty);
        }
        EndGround(ref start, ref length, plfs);
        return plfs;
    }

    int GetDifficultyWeight(Difficulty levelDifficulty, Difficulty generatorDifficulty)
    {
        return Mathf.Clamp(3 - Mathf.Abs(levelDifficulty - generatorDifficulty), 0, 3);
    }

    List<Platform> MakeFlatPath(Vector2 start, float length)
    {
        List<Platform> plfs = new List<Platform>();
        StagingGround(ref start, ref length, plfs);
        WidePit(ref start, ref length, plfs);
        while (length > 0)
        {
            switch (Random.Range(5, 6))
            {
            case 0:     // flat land with a platform that sticks out (up or down)
                FlatShort(ref start, ref length, plfs);
                break;
            case 1:     // a small pit you have to jump over, maybe with a hight difference
                SmallPit(ref start, ref length, plfs);
                break;
            case 2:     // a wide pit you have to jump over
                WidePit(ref start, ref length, plfs);
                break;
            case 3:     // a wall you have to scale
                ClimableWall(ref start, ref length, plfs);
                break;
            case 4:
                NoReturnDrop(ref start, ref length, plfs);
                break;
            case 5:
                StairsRight(ref start, ref length, plfs);
                break;
            }
            GenerateSpawner(start);
        }
        EndGround(ref start, ref length, plfs);


        return plfs;
    }

    private static void GenerateSpawner(Vector2 start, Difficulty difficulty = Difficulty.EASY)
    {
        float r = Random.value + (int)difficulty * .2f;
        if (r < 0.6f)
            new Spawner(start + new Vector2(0, 1), 3, Spawner.EnemyType.CHARGER);
        else if (r < 1.1f)
            new Spawner(start + new Vector2(0, 1), 3, Spawner.EnemyType.SHOOTER);
        else
            new Spawner(start + new Vector2(0, 1), 3, Spawner.EnemyType.SHIELD);
    }

    /// <summary>
    /// Long flat land with a wall on the front to start on
    /// </summary>
    private static void StagingGround(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 platforms: Starting wall, safe ground, and the bit lower starting ground
        plfs.Add(new Platform(start + new Vector2(-5, 10), new Vector2(5, 13)));
        plfs.Add(new Platform(start, new Vector2(8, 4)));
        plfs.Add(new Platform(start + new Vector2(8, -3), new Vector2(5, 2)));
        //Debug.Log("Flat: " + l1 + l2 + l3 + up);
        start += new Vector2(13, -3);
        length -= (13);
    }

    /// <summary>
    /// Long flat land with a wall on the front to start on
    /// </summary>
    private void EndGround(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 platforms: End platform, wall ad podium
        plfs.Add(new Platform(start, new Vector2(8, 2)));
        plfs.Add(new Platform(start + new Vector2(8, 10), new Vector2(5, 12)));
        plfs.Add(new Platform(start + new Vector2(3, 1), new Vector2(2, 1)));
        //Debug.Log("Flat: " + l1 + l2 + l3 + up);
        length -= (8);

        chest.transform.position = start + new Vector2(3.5f, 1);
    }

    /// <summary>
    /// flat land with a platform that sticks out (up or down)
    /// V Easy
    /// </summary>
    private static void FlatShort(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 platforms, so determine the length of these platforms
        int l1 = Random.Range(4, 9);
        int l2 = Random.Range(4, 7);
        int l3 = Random.Range(4, 9);
        // Does the platform go up or down?
        bool up = Random.value > 0.5f;
        plfs.Add(new Platform(start, new Vector2(l1, up ? 2 : 3)));
        plfs.Add(new Platform(start + new Vector2(l1 - 1, up ? 1 : -1), new Vector2(l2 + 1, up ? 4 : 2)));  // TODO: Extra random for asteathic positioning
        plfs.Add(new Platform(start + new Vector2(l1 + l2, 0), new Vector2(l3, up ? 2 : 3)));
        //Debug.Log("Flat: " + l1 + l2 + l3 + up);
        start += new Vector2(l1 + l2 + l3, 0);
        length -= (l1 + l2 + l3);
    }

    /// <summary>
    /// a small pit you have to jump over, maybe with a hight difference
    /// Easy
    /// </summary>
    private static void SmallPit(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are two platforms: at the bottom and on the other side. The starting platform is reused from the last template
        // length of the pit and length of the landing
        int l1 = Random.Range(1, 3);
        int l2 = Random.Range(3, 8);
        // up, flat or down?
        int h = Random.Range(-1, 2);
        plfs.Add(new Platform(start + new Vector2(0, Mathf.Min(-1, h - 1)), new Vector2(l1, 1)));
        plfs.Add(new Platform(start + new Vector2(l1, h), new Vector2(l2, 3)));
        //Debug.Log("Small jump: " + l1 + l2 + h);
        start += new Vector2(l1 + l2, h);
        length -= (l1 + l2);
    }

    /// <summary>
    /// a wide pit you have to jump over
    /// Easy
    /// </summary>
    private static void WidePit(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 platforms: one at the bottom, the landing and the recup. The recup is always 1 by 1. The hole is 2 deep.
        // length of the pit and length of the landing
        int l1 = Random.Range(3, 5);
        int l2 = Random.Range(3, 8);
        plfs.Add(new Platform(start + new Vector2(0, -2), new Vector2(l1, 2)));
        plfs.Add(new Platform(start + new Vector2(0, -1), new Vector2(1, 1)));
        plfs.Add(new Platform(start + new Vector2(l1, 0), new Vector2(l2, 3)));
        //Debug.Log("Big jump: " + l1 + l2);
        start += new Vector2(l1 + l2, 0);
        length -= (l1 + l2);
    }

    /// <summary>
    /// a wall you have to scale
    /// Hard
    /// </summary>
    private static void ClimableWall(ref Vector2 start, ref float length, List<Platform> plfs)
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
        plfs.Add(new Platform(start + new Vector2(l1 - l2 - 2, h2 + (down ? -1 : 0)), new Vector2(l2, 1)));
        plfs.Add(new Platform(start + new Vector2(l1, h1 + (down ? -1 : 0)), new Vector2(l3, h1 + 2)));
        //Debug.Log("Climb: " + l1 + l2 + l3 + down + h1 + h2);
        start += new Vector2(l1 + l3, h1 + (down ? -1 : 0));
        length -= (l1 + l3);
    }

    /// <summary>
    /// A drop that is hard to scale back up
    /// Moderate
    /// </summary>
    private static void NoReturnDrop(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // 3 platforms: The high one from wich to drop, the landing and the continuation, which may or not be the same hight.
        // Length of the drop, landing and continuation
        int l1 = Random.Range(2, 6);
        int l2 = Random.Range(2, 6);
        int l3 = Random.Range(4, 9);
        // The hight of the drop and of the step of the continuation
        int h1 = Random.Range(3, 9);
        int h2 = Random.Range(-1, 2);
        plfs.Add(new Platform(start, new Vector2(l1, h1 + 1)));
        plfs.Add(new Platform(start + new Vector2(l1, -h1), new Vector2(l2, 2)));
        plfs.Add(new Platform(start + new Vector2(l1 + l2, -h1 + h2), new Vector2(l3, h2 == -1 ? 1 : 2)));
        //Debug.Log("No return drop: " + l1 + l2 + l2 + h1 + h2);
        start += new Vector2(l1 + l2 + l3, -h1 + h2);
        length -= (l1 + l2 + l3);
    }

    /// <summary>
    /// A couple of stairs to climb to the right
    /// Moderate
    /// </summary>
    private static void StairsRight(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 fixed platforms, the starting pillar, the catcher and the back pillar. Other then that, there are between 2 and 5 steps
        int steps = Random.Range(2, 6);
        // The hight of the drop and of the step of the continuation
        plfs.Add(new Platform(start, new Vector2(2, 3)));
        int totalLength = 0;
        for (int i = 0; i < steps; i++)
        {
            int gap = Random.Range(1, 4);
            int l = Random.Range(2, 5);
            plfs.Add(new Platform(start + new Vector2(2 + totalLength + gap, i + 1), new Vector2(l, 1)));
            totalLength += (gap + l);
        }
        plfs.Add(new Platform(start + new Vector2(2, -1), new Vector2(totalLength + 2, 2)));
        plfs.Add(new Platform(start + new Vector2(2 + totalLength + 2, steps), new Vector2(2, steps + 3)));
        Debug.Log("Stars Right: " + steps + ", " + totalLength);
        start += new Vector2(totalLength + 6, steps);
        length -= (totalLength + 6);
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

    public class Spawner
    {
        public Vector2 position;
        public int amount;
        public enum EnemyType { CHARGER, SHOOTER, SHIELD}
        public EnemyType type;
        public int difficulty;

        static ObjectPool enemyPool;
        static List<SpawnerGO> spawners = new List<SpawnerGO>();

        public Spawner(Vector2 position, int amount, EnemyType type, int difficulty = 1)
        {
            if (enemyPool == null)
                enemyPool = GameObject.Find("EnemyPool").GetComponent<ObjectPool>();
            this.position = position; this.amount = amount; this.type = type; this.difficulty = difficulty;
            var spawnergo = new GameObject("Spawner " + position.x).AddComponent<SpawnerGO>();
            spawnergo.parent = this;
            spawners.Add(spawnergo);
        }

        public static void RemoveAllSpawners()
        {
            spawners.ForEach(go => Destroy(go.gameObject));
            spawners = new List<SpawnerGO>();
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
                    if (Mathf.Abs(player.position.x - parent.position.x) < DISTANCE_UNTILL_ACTIVATION)
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
                        if (parent.type == EnemyType.SHOOTER)
                            enemy.GetComponent<Enemy>().projectiles = 3;
                        else
                            enemy.GetComponent<Enemy>().projectiles = 0;
                        if (parent.type == EnemyType.SHIELD)
                        {
                            enemy.GetComponent<Enemy>().shieldMagic = 10;
                            enemy.GetComponent<Enemy>().shieldMagicLeft = 10;
                        } else
                        {
                            enemy.GetComponent<Enemy>().shieldMagic = 0;
                            enemy.GetComponent<Enemy>().shieldMagicLeft = 0;
                        }
                        parent.amount--;
                        if (parent.amount <= 0)
                        {
                            spawners.Remove(this);
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }
    }

}

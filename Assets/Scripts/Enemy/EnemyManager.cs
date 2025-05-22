using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyManager : MonoBehaviour
{

    private static EnemyManager _instance;



    public static EnemyManager Instance
    {
        get
        {
            if(_instance is null)
            {
                _instance = FindFirstObjectByType<EnemyManager>();
                if(_instance is null)
                {
                    GameObject obj = new GameObject("EnemyManager");
                    _instance = obj.AddComponent<EnemyManager>();
                }
            }
            return _instance;
        }
    }
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> _pools;
    public Dictionary<string, Queue<GameObject>> _poolDict;


    public List<Vector3> _spawnPoint;

    public MapGeneration _MapGeneration;
    private bool _mapIsReady = false;

    private float _spawnTimer = 0f;
    public float _spawnTime;
    private Vector3 _spawnPosition;
    private string _spawnTag;

    public void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        _poolDict = new Dictionary<string, Queue<GameObject>>();

        _MapGeneration.OnMapGenerated += MapisReady;

        foreach (Pool pool in _pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for(int i = 0; i <  pool.size; i ++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            _poolDict.Add(pool.tag, objectPool);
        }
    }
    private void Update()
    {
        if (_mapIsReady)
        {
            _spawnTimer += Time.deltaTime;
            if(_spawnTimer > _spawnTime)
            {
                _spawnTimer -= _spawnTime;
                _spawnPosition = GetRandomSpawnPoint();
                _spawnTag = GetRandomTag();
                SpawnFromPool(_spawnTag, _spawnPosition, Quaternion.identity);

            }
        }
    }

    public void MapisReady(bool Done)
    {
        if (Done)
        {
            _spawnPoint =  _MapGeneration.GetEnemySpawnPointList();
        }
        _mapIsReady = Done;
    }

    public GameObject SpawnFromPool(string tag,Vector3 position,Quaternion rotation)
    {
        if (!_poolDict.ContainsKey(tag)) return null;

        Queue<GameObject> pool = _poolDict[tag];
        if (pool.Count <= 0) return null;

        GameObject obj = pool.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        string tag = obj.tag;
        if (_poolDict.ContainsKey(tag))
        {
            _poolDict[tag].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
    
    public Vector3 GetRandomSpawnPoint()
    {
        return _spawnPoint[UnityEngine.Random.Range(0, _spawnPoint.Count)];
    }
    
    public string GetRandomTag()
    {
        return _poolDict.Keys.ToArray()[UnityEngine.Random.Range(0, _poolDict.Count)];
    }

    public void ClearPool()
    {

    }
}

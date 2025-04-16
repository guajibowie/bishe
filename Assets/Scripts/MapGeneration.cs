
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;

public class MapGeneration : MonoBehaviour
{
    public GameObject _wallPrefab;
    public GameObject[] _floorPrefabs;
    public Transform _environment;
    public GameObject _playerPrefab;
    public GameObject[] _decorations;

    public List<string> _floorCloneName;


    public int _maxDecoration;
    public float _floorRadius;
    public int _matrixRows;
    public int _matrixCols;
    public int _seedNumber;
    private (int,int)[] _moveDirections = {(1, 0), (-1, 0), (0, 1), (0, -1)};
    private (int, int)[] _directions = { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };
    private HashSet < (int, int) >  seeds = new HashSet<(int, int)>();
    public int _counts = 0;
    public float _walkerChance;
    public int _maxWalker;
    public int _elementNumber;
    public float _threshold;

    public List<MapWalker> _walkers_L;

    public Vector2 _spawnPoint;

    public NavMeshSurface _NavMeshSurface;
    public List<Vector3> _monsterSpawnPoint;

    // Start is called before the first frame update
    void Awake()
    {

        _NavMeshSurface = GetComponent<NavMeshSurface>();
        _elementNumber = _matrixCols * _matrixRows;
        _floorCloneName = new List<string>();

        SeedInit();
        WalkerMatrixInit();
        //MatrixInit();
        var player = Instantiate(_playerPrefab, new Vector3(_spawnPoint.x, 4, _spawnPoint.y), Quaternion.identity);
        PlayerManager.Instance.SetPlayerMovement(player.GetComponent<PlayerMovement>());

        
    }
    public void SeedInit()
    {
        if (seeds.Count > 0)
        {
            seeds.Clear();
        }
        seeds.Add((_matrixRows / 2, _matrixCols / 2));
        float x = _matrixRows * _floorRadius + _floorRadius;
        float y = _matrixCols * _floorRadius + _floorRadius;
        _spawnPoint = new Vector2(x, y);
    }

    public void WalkerMatrixInit()
    {
        int[,] _map = new int[_matrixRows, _matrixCols];
        _walkers_L = new List<MapWalker>();
        MapWalker curWalker = new MapWalker(new Vector2(_matrixRows / 2, _matrixCols / 2),GetDirection(),_walkerChance);

        Instantiate(_floorPrefabs[0], new Vector3(_spawnPoint.x,0,_spawnPoint.y), Quaternion.Euler(0, 0, 0), _environment);
        _map[_matrixRows / 2, _matrixCols / 2] = 1;
        _walkers_L.Add(curWalker);
        _counts++;
        StartCoroutine(CreateFloor(_map));

    }

    public void MatrixInit()
    {
        int[,] _map = new int[ _matrixRows, _matrixCols ];
        int[,] _breadcrumb = (int[,])_map.Clone();
        Queue < (int, int) > _queue = new Queue<(int, int)>(seeds);
        _counts = 0;
        while(_queue.Count > 0)
        {
            (int x, int y) = _queue.Dequeue();
            int _nb = CountNeighbors(_map, x, y);
            float _tmp = (1 - (float)_nb / 9);
            float _probability = Mathf.Pow(_tmp, 2);
            float _rand = UnityEngine.Random.value;
            _breadcrumb[x, y] = 1;
            if (_rand < _probability)
            {
                _map[x, y] = 1;
                Vector3 vector3 = new Vector3(x * _floorRadius * 2 + _floorRadius, 0, y * _floorRadius * 2 + _floorRadius);
                Instantiate(_floorPrefabs[0], vector3, Quaternion.Euler(0, 0, 0), _environment);
                _counts += 1;
                foreach (var dir in _moveDirections)
                {
                    int nx = x + dir.Item1;
                    int ny = y + dir.Item2;
                
                    if (nx >= 0 && nx < _matrixRows && ny >= 0 && ny < _matrixCols && _breadcrumb[nx,ny] != 1)
                    {
                        _queue.Enqueue((nx, ny));
                        _breadcrumb[nx, ny] = 1;
                    }
                }
            }
            
        }
        Debug.Log("count:" + _counts);
    }

    public int CountNeighbors(int[,] map,int x,int y)
    {
        int _count = 0;
        foreach(var dir in _directions)
        {
            int nx = x + dir.Item1;
            int ny = y + dir.Item2;

            if(nx >= 0 && nx < _matrixRows && ny >= 0 && ny < _matrixCols && map[nx,ny] == 1)
            {
                _count++;
            }
        }
        return _count;
    }

    public void setNeighborWalls(int[,] map, int x, int y)
    {
        if (map[x, y] != 1) return;
        foreach (var dir in _directions)
        {
            int nx = x + dir.Item1;
            int ny = y + dir.Item2;

            if (nx >= 0 && nx < _matrixRows && ny >= 0 && ny < _matrixCols)
            {
                if(map[nx, ny] == 0)
                {
                    Instantiate(_wallPrefab, new Vector3(nx * _floorRadius * 2 + _floorRadius, 0, ny * _floorRadius * 2 + _floorRadius), Quaternion.identity,_environment);
                }
            }
            else
            {
                Instantiate(_wallPrefab, new Vector3(nx * _floorRadius * 2 + _floorRadius, 0, ny * _floorRadius * 2 + _floorRadius), Quaternion.identity, _environment);
            }

        }
    }
    public void PrintMatrix(int[,] matrix)
    {
        int N = matrix.GetLength(0); // 获取矩阵的行数
        int M = matrix.GetLength(1); // 获取矩阵的列数

        for (int i = 0; i < N; i++)
        {
            string row = "";
            for (int j = 0; j < M; j++)
            {
                row += matrix[i, j] + " ";
            }
            Debug.Log(row.TrimEnd()); // 使用 Debug.Log 打印每一行
        }
    }

    public Vector2 GetDirection()
    {
        int choice = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

        return new Vector2(_moveDirections[choice].Item1, _moveDirections[choice].Item2);

    }

    public Vector2 GetSpawnPoint()
    {
        return _spawnPoint;
    }

    public GameObject SetFloor(Vector2 vector2,bool Random = false)
    {
        if (Random)
        {
            return Instantiate(_floorPrefabs[UnityEngine.Random.Range(0,_floorPrefabs.Length)], new Vector3(vector2.x * _floorRadius * 2 + _floorRadius, 0, vector2.y * _floorRadius * 2 + _floorRadius), Quaternion.identity, _environment);
        }
        return Instantiate(_floorPrefabs[0],new Vector3(vector2.x * _floorRadius * 2 + _floorRadius, 0,vector2.y * _floorRadius * 2 + _floorRadius), Quaternion.identity, _environment);
    }
    IEnumerator CreateFloor(int[,] map)
    {

        List<int> toBeDel = new List<int>();
        while(((float)_counts / _elementNumber) < _threshold)
        {
            int curWalkerNum = _walkers_L.Count;
            toBeDel.Clear();
            for (int i = 0; i < curWalkerNum; i++)
            {
                MapWalker curWalker = _walkers_L[i];
                if(map[(int)curWalker._position.x, (int)curWalker._position.y] != 1)
                {
                    GameObject floor = SetFloor(curWalker._position,true);
                    if (!_floorCloneName.Contains(floor.name))
                    {
                        _floorCloneName.Add(floor.name);
                    }
                    StartCoroutine(CreateDecorations(floor));
                    _counts++;
                    map[(int)curWalker._position.x, (int)curWalker._position.y] = 1;
                }
                   
                if (curWalker.Change() && curWalkerNum > 1)
                {
                    toBeDel.Add(i);
                    curWalkerNum--;
                    continue;
                }
                if (curWalker.Change())
                {
                    curWalker._direction = GetDirection();
                }
                if (curWalker.Change() && curWalkerNum < _maxWalker)
                {
                    _walkers_L.Add(new MapWalker(curWalker._position, GetDirection(), _walkerChance));
                    curWalkerNum++;
                }
                curWalker.UpdatePosition();
                curWalker._position = new Vector2(Mathf.Clamp(curWalker._position.x, 0, _matrixRows-1), Mathf.Clamp(curWalker._position.y, 0, _matrixCols-1));
            }
            if(toBeDel.Count > 0)
            {

                toBeDel.Sort((a, b) => b.CompareTo(a));
                foreach(var index in toBeDel)
                {
                    _walkers_L.RemoveAt(index);
                }
            }
            yield return null;
        }
        StartCoroutine(CreateWalls(map));
    }
    IEnumerator CreateWalls(int[,] map)
    {
        for(int i = 0; i < _matrixRows; i++)
        {
            for(int j = 0; j < _matrixCols; j++)
            {
                setNeighborWalls(map, i, j);
                yield return null;
            }
        }

        CreateNavMesh();
    }

    IEnumerator CreateDecorations(GameObject parent)
    {
        int decorationsNUM = UnityEngine.Random.Range(0, _maxDecoration);
        if (decorationsNUM == 0)
        {
            yield break;
        }
        for (int i = 0; i < decorationsNUM; i ++ )
        {
            Vector3 local = GetRandomVector3(-_floorRadius, _floorRadius, new Vector3(1, 0, 1)) + Vector3.up*10;
            GameObject decoration =  Instantiate(GetDecoration(),local,Quaternion.identity);
            decoration.transform.SetParent(parent.transform, false);

            RaycastHit hit;
            if(Physics.Raycast(decoration.transform.position,-decoration.transform.up, out hit,Mathf.Infinity))
            {
                if (!_floorCloneName.Contains(hit.transform.name))
                {
                    Destroy(decoration);
                    continue;
                }
                decoration.transform.position = hit.point;
                decoration.transform.rotation = Quaternion.Euler(new Vector3(decoration.transform.rotation.x, decoration.transform.rotation.y+ UnityEngine.Random.Range(0,90), decoration.transform.rotation.x));
                decoration.transform.localScale = new Vector3(
                    decoration.transform.localScale.x / decoration.transform.parent.localScale.x,
                    decoration.transform.localScale.y / decoration.transform.parent.localScale.y,
                    decoration.transform.localScale.z / decoration.transform.parent.localScale.z
                )* UnityEngine.Random.Range(1,2);
            }

            yield return null;
        }
    }

    public Vector3 GetRandomVector3(float min = 0,float max = 1,Vector3 axisMask = default)
    {
        return new Vector3(UnityEngine.Random.Range(min,max) * axisMask.x, UnityEngine.Random.Range(min, max) * axisMask.y, UnityEngine.Random.Range(min, max) * axisMask.z);
    }

    public GameObject GetDecoration()
    {
        int index = UnityEngine.Random.Range(0,_decorations.Length);
        return _decorations[index];

    }

    public void CreateNavMesh()
    {
        _NavMeshSurface.RemoveData();
        _NavMeshSurface.BuildNavMesh();
    }
}

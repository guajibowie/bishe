using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemManager : MonoBehaviour
{

    private static DropItemManager _instance;

    public static DropItemManager Instance
    {
        get
        {
            if(_instance is null)
            {
                _instance = FindFirstObjectByType<DropItemManager>();
                if(_instance is null)
                {
                    GameObject _obj = new GameObject("DropItemManager");
                    _instance = _obj.AddComponent<DropItemManager>();
                }
            }
            return _instance;
        }
    }

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public List<GameObject> _dropItem = new List<GameObject>();

    Array _dropType = Enum.GetValues(typeof(DropType));
    public void CreateDropItem(Vector3 position)
    {
        DropType type = (DropType)_dropType.GetValue(UnityEngine.Random.Range(0, _dropType.Length));
        InstantiateDropItem(type, position);
    }

    public void CreateDropItem(DropType type, Vector3 position)
    {
        InstantiateDropItem(type, position);
    }

    /// <summary>
    /// 所有创造掉落物实例化的实际方法
    /// </summary>
    /// <param name="type"></param>
    /// <param name="position"></param>
    private void InstantiateDropItem(DropType type, Vector3 position)
    {
        GameObject DropItem;
        switch (type)
        {
            case DropType.Health:
                {
                    DropItem = _dropItem[0];
                    break;
                }
            case DropType.HealthLimit:
                {
                    DropItem = _dropItem[1];
                    break;
                }
            case DropType.Damage:
                {
                    DropItem = _dropItem[2];
                    break;
                }
            case DropType.Speed:
                {
                    DropItem = _dropItem[3];
                    break;
                }
            case DropType.JumpFroce:
                {
                    DropItem = _dropItem[4];
                    break;
                }
            default:
                DropItem = _dropItem[0];
                break;
        }

        Instantiate<GameObject>(DropItem, position: position, rotation: Quaternion.identity);
    }
}

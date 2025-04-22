using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemManager : MonoBehaviour
{
    public List<GameObject> _dropItem = new List<GameObject>();

    Array _dropType = Enum.GetValues(typeof(DropType));
    public void CreateDropItem(Vector3 position)
    {
        DropType type = (DropType)_dropType.GetValue(UnityEngine.Random.Range(0, _dropType.Length));
        GameObject DropItem;
        switch (type)
        {
            case DropType.Health:
            {
                DropItem = _dropItem[0];
                break;
            }
            case DropType.Damage:
            {
                DropItem = _dropItem[1];
                break;
            }
            case DropType.Speed:
            {
                DropItem = _dropItem[2];
                break;
            }
            default:
                DropItem = _dropItem[0];
                break;
        }

        Instantiate<GameObject>(DropItem, position: position,rotation:Quaternion.identity);
    }

    public void CreateDropItem(DropType type, Vector3 position)
    {
        GameObject DropItem;
        switch (type)
        {
            case DropType.Health:
                {
                    DropItem = _dropItem[0];
                    break;
                }
            case DropType.Damage:
                {
                    DropItem = _dropItem[1];
                    break;
                }
            case DropType.Speed:
                {
                    DropItem = _dropItem[2];
                    break;
                }
            default:
                DropItem = _dropItem[0];
                break;
        }

        Instantiate<GameObject>(DropItem, position: position, rotation: Quaternion.identity);
    }
}

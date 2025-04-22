using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionControl : MonoBehaviour
{
    public WeaponDepot _WeaponDepot;
    public PlayerMovement _PlayerMovement;


    public void PickWeapon(string weaponName)
    {
        _WeaponDepot.AddWeapon(weaponName);
    }
    public void OnHurt(float damage)
    {
        _PlayerMovement.Hurt(damage);
    }
    public void PickDropItem(DropType type,float value)
    {
        switch (type)
        {
            case DropType.Health:
                _PlayerMovement.Restore(value);
                return;
            case DropType.HealthLimit:
                PlayerManager.Instance.AddHPlimit(value);
                return;
            case DropType.Damage:
                PlayerManager.Instance.AddDamage(value);
                return;
            case DropType.JumpFroce:
                PlayerManager.Instance.AddJumpFactor(value);
                return;
            case DropType.Speed:
                PlayerManager.Instance.AddSpeedFactor(value);
                return;
            default:
                return;
        }
    }
}

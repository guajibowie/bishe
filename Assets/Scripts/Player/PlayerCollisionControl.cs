using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionControl : MonoBehaviour
{
    public WeaponDepot _WeaponDepot;


    public void PickWeapon(string weaponName)
    {
        _WeaponDepot.AddWeapon(weaponName);
    }
}

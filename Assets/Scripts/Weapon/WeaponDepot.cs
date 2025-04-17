using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponDepot : MonoBehaviour
{
    public List<GameObject> _weaponDepot = new List<GameObject>();
    private int _currentWeaponIndex = 0;
    public GameInput _InputAction;

    public Camera _mainCamera;

    private void Awake()
    {
        
        _InputAction = new GameInput();
    }
    void Start()
    {
        _mainCamera = Camera.main;
        for(int i = 0; i < _weaponDepot.Count; i++)
        {
            _weaponDepot[i].SetActive(i == _currentWeaponIndex);
        }
    }
    private void OnEnable()
    {
        _InputAction.Enable();
        _InputAction.Player.SwitchWeapon.performed += OnMouseScroll;
    }
    private void OnDisable()
    {

        _InputAction.Player.SwitchWeapon.performed -= OnMouseScroll;
        _InputAction.Disable();
    }

    public void SwitchToNextWeapon()
    {
        InitCamera();
        _weaponDepot[_currentWeaponIndex].SetActive(false);
        _currentWeaponIndex = (_currentWeaponIndex + 1) % _weaponDepot.Count;
        _weaponDepot[_currentWeaponIndex].SetActive(true);
    }
    public void SwitchToPreviousWeapon()
    {
        InitCamera();
        _weaponDepot[_currentWeaponIndex].SetActive(false);
        _currentWeaponIndex = (_currentWeaponIndex - 1 + _weaponDepot.Count) % _weaponDepot.Count;
        _weaponDepot[_currentWeaponIndex].SetActive(true);
    }

    public void OnMouseScroll(InputAction.CallbackContext context)
    {
        Vector2 scroll = context.ReadValue<Vector2>();
        if(_weaponDepot.Count > 0)
        {
            if(scroll.y < 0)
            {
                SwitchToNextWeapon();
            }else if(scroll.y > 0)
            {
                SwitchToPreviousWeapon();
            }
        }
    }
    
    public void AddWeapon(string weaponName)
    {
       Transform weapon = transform.Find(weaponName);
        if(weapon != null)
        {
            if (_weaponDepot.Contains(weapon.gameObject))
            {
                Debug.Log("we got it");
            }
            else
            {
                _weaponDepot.Add(weapon.gameObject);
                SwitchWeapon(_weaponDepot.Count - 1);
            }
        }
    }

    public void SwitchWeapon(int Index = 0)
    {
        _weaponDepot[_currentWeaponIndex].SetActive(false);
        _currentWeaponIndex = Index;
        _weaponDepot[_currentWeaponIndex].SetActive(true);


    }
    public void InitCamera()
    {
        _mainCamera.fieldOfView = 60f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using UnityEngine.InputSystem;
using JetBrains.Annotations;

public class PerspectiveControl : MonoBehaviour
{
    public Transform PlayerCamera;

    private float _picthLimit = 75f;
    private float _mouseSensitivity = 20f;
    private Vector2 _mouseInput;
    private float _cameraPitch = 0f;
    private GameManager gameManager;
    [SerializeField] private float _rotX;
    [SerializeField] private float _rotY;


    private void Start()
    {
        gameManager = GameManager.Instance;

        _mouseSensitivity = gameManager.GetMouseSensitivity();

        gameManager._mouseSensitivityChange += UpdateMouseSensitivity;
    }
    private void Update()
    {
        Look();
    }

    public void Look()
    {
        _rotY = _mouseInput.x * _mouseSensitivity * Time.deltaTime;
        _rotX = _mouseInput.y * _mouseSensitivity * Time.deltaTime;

        _cameraPitch -= _rotX;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -_picthLimit, _picthLimit);
        PlayerCamera.localRotation = Quaternion.Euler(_cameraPitch, 0, 0);
        transform.Rotate(Vector3.up * _rotY);
    }

    public void UpdateMouseSensitivity(float newMouseSensitivity)
    {
        _mouseSensitivity = newMouseSensitivity;
    }
    public void MouseInput(InputAction.CallbackContext callbackContext)
    {
        _mouseInput = callbackContext.ReadValue<Vector2>();
    }
}

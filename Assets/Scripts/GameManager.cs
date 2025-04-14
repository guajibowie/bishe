using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
                if(_instance == null)
                {
                    GameObject _obj = new GameObject("GameManager");
                    _instance = _obj.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    public float _initalMouseSensitivity;
    [SerializeField] private float _inputThreshold;
    [SerializeField] private float _mouseSensitivity;

    public delegate void OnInputThresholdChange(float newInputThreshold);
    public event OnInputThresholdChange _inputThresholdChange;

    public delegate void OnMouseSensitivityChange(float newMouseSensitivity);
    public event OnMouseSensitivityChange _mouseSensitivityChange;


    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _inputThreshold = 0.1f;
        _mouseSensitivity = 80f;
        _initalMouseSensitivity = _mouseSensitivity;
    }


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    //private void Update()
    //{

    //}
    public void ChangeInputThreshold(float newInputThreshold)
    {
        _inputThreshold = newInputThreshold;
        _inputThresholdChange?.Invoke(_inputThreshold);
    }

    public void ChangeMouseSensitiviity(float newMouseSensitivity)
    {
        _mouseSensitivity = newMouseSensitivity;
        _mouseSensitivityChange?.Invoke(_mouseSensitivity);
    }

    public float GetInputThreshold()
    {
        return _inputThreshold;
    }

    public float GetMouseSensitivity()
    {
        return _mouseSensitivity;
    }
}

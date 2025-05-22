using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool _IsPause;
    private static GameManager _instance;
    public float _GameTimer;
    public float _GameEndTime;
    public bool _GameStart;

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

    public GameInput _GameInput;

    public GameObject _Menu;
    public GameObject _GameOver_panel;

    private bool _IsMenu;

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

        _IsMenu = false;
        _IsPause = false;
        _inputThreshold = 0.1f;
        _mouseSensitivity = 80f;
        _initalMouseSensitivity = _mouseSensitivity;
        _GameInput = new GameInput();
        _GameInput.Player.pause.performed += PauseGame;
    }
    private void OnEnable()
    {
        _GameInput.Enable();
    }
    private void PauseGame(InputAction.CallbackContext context)
    {
        if (_IsMenu && !_IsPause) return;
        Time.timeScale = Time.timeScale > 0 ? 0 : 1;
        _IsPause = (Time.timeScale == 0);
        if (_IsPause)
        {
            _IsMenu = true;
            _Menu.SetActive(true);
            CursorRelease();
        }
        else
        {
            _IsMenu = false;
            _Menu.SetActive(false);
            CursorLock();
        }
    }

    public void GameOver_Panel(bool success)
    {
        if (_IsMenu)
        {
            return;
        }
        _IsMenu = true;
        string _text = success ? "success" : "game over";
        if (success)
        {
            Time.timeScale = 0;
        }
        TextMeshProUGUI _textBuf = _GameOver_panel.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        _textBuf.text = _text;
        _GameOver_panel.SetActive(true);
        CursorRelease();

    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void FixedUpdate()
    {
        if (_GameStart)
        {
            _GameTimer += Time.fixedDeltaTime;
            if(_GameTimer > _GameEndTime)
            {
                GameOver_Panel(true);
                _GameStart = false;
            }
        }
    }
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
    public void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void CursorRelease()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void ResumeButton()
    {
        Time.timeScale = 1;
        _IsPause = false;
        _Menu.SetActive(false);
        CursorLock();
        _IsMenu = false;
    }

    public void MenuButton()
    {
        _Menu.SetActive(false);
        _GameOver_panel.SetActive(false);
        Time.timeScale = 1;
        _IsPause = false;
        _IsMenu = false;
        SceneManager.LoadScene(0);
        
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void MapDone(bool done) {
        _GameTimer = 0f;
        _GameStart = done;
    }
}

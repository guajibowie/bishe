using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _instance;
    public static PlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PlayerManager>();
                if (_instance == null)
                {
                    GameObject _obj = new GameObject("PlayerManager");
                    _instance = _obj.AddComponent<PlayerManager>();
                }
            }
            return _instance;
        }
    } // 单例

    /// <summary>
    /// 以下参数应当与PlayerMovement中的同名变量同步改动
    /// </summary>
    [SerializeField] private float _speedFactor;
    [SerializeField] private float _jumpFactor;
    [SerializeField] private float _speedScale;
    [SerializeField] private float _playerHPlimit;
    [SerializeField] private float _damage;

    public Vector3 _playerPosition => _PlayerMovement.transform.position;

    private bool _isAlive;
    //管理的脚本
    public PlayerMovement _PlayerMovement;

    public PlayerMovementState _playerMovementState;

    public delegate void OnSpeedFactorCahnge(float newSpeedFactor);
    public event OnSpeedFactorCahnge _speedFactorChange;

    public delegate void OnJumpFactorCahnge(float newJumpFactor);
    public event OnJumpFactorCahnge _jumpFactorChange;

    public delegate void OnSpeedScaleChange(float newSpeedScale);
    public event OnSpeedScaleChange _speedScaleChange;

    public delegate void OnPlayerHPlimitChange(float newLimit);
    public event OnPlayerHPlimitChange _HPlimitChange;

    public delegate void OnDamageChange(float newDamage);
    public event OnDamageChange _damageChange;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);


        _speedFactor = 1f;
        _speedScale = 1f;
        _jumpFactor = 1f;
        _playerHPlimit = 100f;
        _damage = 0f;
        _isAlive = true;

    }

    private void Start()
    {
        _playerMovementState = _PlayerMovement.GetPlayerMovementState();

        _PlayerMovement._playerMovementChange += SyncPlayerMovementState;
    }

    //
    private void SyncPlayerMovementState(PlayerMovementState newState)
    {
        _playerMovementState = newState;
    }


    /// <summary>
    /// 速度相关
    /// </summary>
    /// 
    public void AddHPlimit(float newDeltaLimit)
    {
        _playerHPlimit += newDeltaLimit;
        _HPlimitChange?.Invoke(_playerHPlimit);
    }
    public void ChangeSpeedFactor(float newSpeedFactor)
    {
        _speedFactor = newSpeedFactor;
        _speedFactorChange?.Invoke(_speedFactor);
    }

    public void AddSpeedFactor(float deltaSpeedFactor)
    {
        _speedFactor += deltaSpeedFactor;
        _speedFactorChange?.Invoke(_speedFactor);
    }

    public void ChangSpeedScale(float newSpeedScale)
    {
        _speedScale = newSpeedScale;
        _speedScaleChange?.Invoke(_speedScale);
    }

    public void AddSpeedScale(float deltaSpeedScale)
    {
        _speedScale += deltaSpeedScale;
        _speedScaleChange?.Invoke(_speedScale);

    }


    /// <summary>
    /// 跳跃相关
    /// </summary>
    public void ChangeJumpFactor(float newJumpFactor)
    {
        _jumpFactor = newJumpFactor;
        _jumpFactorChange?.Invoke(_jumpFactor);
    }
    public void AddJumpFactor(float deltaJumpFactor)
    {
        _jumpFactor += deltaJumpFactor;
        _jumpFactorChange?.Invoke(_jumpFactor);
    }
    /// <summary>
    /// 伤害相关
    /// </summary>
    /// <returns></returns>
    public void ChangeDamage(float newDamage)
    {
        _damage = newDamage;
        _damageChange?.Invoke(_damage);
    }
    public void AddDamage(float newDamage)
    {
        _damage += newDamage;
        _damageChange?.Invoke(_damage);
    }

    public float GetSpeedFactor()
    {
        return _speedFactor;
    }
    public float GetSpeedScale()
    {
        return _speedScale;
    }

    public float GetJumpFactor()
    {
        return _jumpFactor;
    }

    public bool GetMoving()
    {
        return _PlayerMovement._moving;
    }

    public bool GetIsRunning()
    {
        return _PlayerMovement._isRunning;
    }

    public bool GetIsGrounded()
    {
        return _PlayerMovement._isGrounded;
    }
    public bool GetIsFlying()
    {
        return _PlayerMovement._isGrounded;
    }
    public void testInstance()
    {
        Debug.Log("Hello,I am PlayerManager");
    }

    public void PlayerDeaded()
    {
        _isAlive = false;
    }

    public bool CheckAlive()
    {
        return _isAlive;
    }
    public void SetPlayerMovement(PlayerMovement playerMovement)
    {
        _PlayerMovement = playerMovement;
    }
}

using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;


public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// 角色控制脚本
    /// </summary>
    public GameObject _weapon;
    
    public float _VIF = 0.2f; //速度插值系数 ： velocity interpolation factor
    public float _deceleration = 0.9f; // 减速系数
    private float _cushion = -2f; //落地缓冲力
    private float _gravity = -9.8f;
    [Header("速度")]
    [SerializeField]    public float _runSpeed = 10f;  
    [SerializeField]    public float _walkSpeed = 6f;
    [SerializeField]    private float _jumpFroce = 6f;
    [SerializeField]    private float _targetSpeed = 6f;
    [SerializeField]    private float _currentSpeed = 0f;
    private Vector3 _airDirection;

    /// <summary>
    /// 与肉鸽玩法相关的参数，希望由PlayerManager管理
    /// </summary>
    private float _speedFactor = 1f;
    private float _jumpFactor = 1f;
    private float _HPlimit = 100f;

    private float _curHP;

    //从GameManager获得的参数
    private float _inputThreshold = 0.1f;

    private Vector2 _moveInput;
    private Vector2 _mouseInput;
    private Vector3 _horizontolDirection; //水平输入方向，用于充当中间值方便应用重力
    private Vector3 _moveDirection;

    public bool _isRunning;
    public bool _isGrounded;
    private float _airTimer;
    public bool _isJumping;
    public bool _moving;
    public bool _isAlive;

    public CharacterController CharacterController;
    public InputAction InputAction;
    public Camera MainCamera;
    public Camera SecondCamera; // 第二相机，用于显示手部
    [Header("音频")]
    private AudioSource AudioSource;
    public AudioUtility _Audio;
    public AudioClip _walkClip;
    public AudioClip _jumpClip;
    public AudioClip _runClip;

    private PlayerMovementState _playerMovementState;
    private PlayerMovementState _preMovementState;
    /// <summary>
    /// 单例
    /// </summary>
    private PlayerManager playerManager;
    private GameManager gameManager;

    public delegate void OnPlayerMovementStateChange(PlayerMovementState newState);
    public event OnPlayerMovementStateChange _playerMovementChange;


    private void Awake()
    {
        _isRunning = false;
        _isGrounded = false;
        _isJumping = false;
        _moving = false;
        _curHP = _HPlimit;
        _isAlive = true;

        _playerMovementState = PlayerMovementState.IDLE;
    }

    private void Start()
    {
        CharacterController = GetComponent<CharacterController>();
        AudioSource = GetComponent<AudioSource>();
        _Audio = GetComponent<AudioUtility>();

        playerManager = PlayerManager.Instance;
        gameManager = GameManager.Instance;


        _speedFactor = playerManager.GetSpeedFactor();
        _jumpFactor = playerManager.GetJumpFactor();
        _inputThreshold = gameManager.GetInputThreshold();

        
        //playerManager注册区
        playerManager._jumpFactorChange += UpdateJumpFactor;
        playerManager._speedFactorChange += UpdateSpeedFactor;
        playerManager._HPlimitChange += UpateHPlimit;
        //gameManager注册区
        gameManager._inputThresholdChange += UpdateInputThreshold;
    }

    private void OnDestroy()
    {
        //playerManager注销区
        playerManager._jumpFactorChange -= UpdateJumpFactor;
        playerManager._speedFactorChange -= UpdateSpeedFactor;
        playerManager._HPlimitChange -= UpateHPlimit;
        //gameManager注销区
        gameManager._inputThresholdChange -= UpdateInputThreshold;
    }

    private void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            //GameManager.Instance.GameOver_Panel(PlayerManager.Instance.CheckAlive());
            Hurt(34);
        }
        //移动
        if (!CharacterController.isGrounded)
        {
            _airTimer += Time.deltaTime;
            if(_airTimer > 0.2f)
            {
                _isGrounded = CharacterController.isGrounded;
                _airTimer = 0;
            }
        }
        else
        {
            _isGrounded = true;
        }
        CountMoveDirection();
        ApplyGravity();
        Move();
        UpdateState();
    }

    public void UpdateState()
    {

        //_preMovementState = _playerMovementState;//存放当前状态 以判断状态是否改变
        bool stateChange = false;

        if (!_isGrounded)
        {
            if(_playerMovementState != PlayerMovementState.FLYING)
            {
                _playerMovementState = PlayerMovementState.FLYING;
                stateChange = true;
            }
        } 
        else if(_currentSpeed > 0)
        {
            if(_isRunning)
            {
                if(_playerMovementState != PlayerMovementState.RUNNING)
                {
                    _playerMovementState = PlayerMovementState.RUNNING;
                    stateChange = true;
                }
            }
            else
            {
                if(_playerMovementState != PlayerMovementState.WALKING)
                {
                    _playerMovementState = PlayerMovementState.WALKING;
                    stateChange = true;
                }
            }
        }
        else if(_playerMovementState != PlayerMovementState.IDLE)
        {
            _playerMovementState = PlayerMovementState.IDLE;
            stateChange = true;
        }
        //事件回调
        if(stateChange)
        {
            _playerMovementChange?.Invoke(_playerMovementState);
        }
    }
    private void CountMoveDirection()
    {
        _targetSpeed = _isRunning ? _runSpeed : _walkSpeed;
        //判断是否在地上，我希望人物在天上不能加速，只能受到空气阻力的影响
        if (_isGrounded)
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, _VIF);
            _horizontolDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;
            _airDirection = _horizontolDirection;
        }
        else
        {

            _horizontolDirection = _airDirection;
        }
        //若不在地上则会受到_deceleration的影响，一般会减速
        _horizontolDirection *= _currentSpeed * _speedFactor * (_isGrounded ? 1f : _deceleration);
        
        _moving = _horizontolDirection.sqrMagnitude > 0;

        _moveDirection.x = _horizontolDirection.x;
        _moveDirection.z = _horizontolDirection.z;
    }
    private void Move()
    {
        if(_isGrounded && _moving)
        {
            _Audio.PlayLoopClip(_isRunning ? _runClip : _walkClip);
        }
        else
        {
            _Audio.StopPlay();
        }
        CharacterController.Move(_moveDirection * Time.deltaTime);
    }
    private void ApplyGravity()
    {
        if (_isGrounded && _moveDirection.y < 0)
        {
            _moveDirection.y = _cushion;
            _isJumping = false;
        }
        else
        {
            _moveDirection.y += _gravity * Time.deltaTime;
        }
    }

    public void PlayerMove(InputAction.CallbackContext callbackContext)
    {
        _moveInput = callbackContext.ReadValue<Vector2>();
    }

    public void PlayerJump(InputAction.CallbackContext callbackContext)
    {
        if (!_isJumping && _isGrounded)
        {
            _moveDirection.y = _jumpFroce * _jumpFactor;
            if(_jumpClip != null)
            {
                _Audio.PlayClipOneShot(_jumpClip);
            }
            _isJumping = true;
        }
    }
    public void PlayerRun(InputAction.CallbackContext callbackContext)
    {
        _isRunning = callbackContext.ReadValueAsButton();
    }


    public void Hurt(float damage)
    {
        if (!PlayerManager.Instance.CheckAlive()) return;
        _curHP -= damage;
        if(_curHP <= 0)
        {
            _isAlive = false;
            _weapon.SetActive(false);
            PlayerManager.Instance.PlayerDeaded();
            Invoke("SetEndPanel",4);
            Debug.Log("dead");
            this.enabled = false;
        }
    }

    private void SetEndPanel()
    {
            GameManager.Instance.GameOver_Panel(PlayerManager.Instance.CheckAlive());
    }

    public void Restore(float amount)
    {
        if (_curHP <= 0) return;
        float lost = _HPlimit - _curHP;
        float reHp = amount > lost ? lost : amount;
        _curHP += reHp;
    }



    private void UpdateSpeedFactor(float newSpeedFactor)
    {
        _speedFactor = newSpeedFactor;
    }

    private void UpateHPlimit(float newLimit)
    {
        _HPlimit = newLimit;
    }
    private void UpdateJumpFactor(float newJumpFactor)
    {
        _jumpFactor = newJumpFactor;
    }

    private void UpdateInputThreshold(float newInputThreshold)
    {
        _inputThreshold = newInputThreshold;
    }

  

    //供给PlayerManager
    public PlayerMovementState GetPlayerMovementState()
    {
        return _playerMovementState;
    }
}

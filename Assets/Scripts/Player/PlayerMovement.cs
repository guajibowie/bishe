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
    /// ��ɫ���ƽű�
    /// </summary>
    public GameObject _weapon;
    
    public float _VIF = 0.2f; //�ٶȲ�ֵϵ�� �� velocity interpolation factor
    public float _deceleration = 0.9f; // ����ϵ��
    private float _cushion = -2f; //��ػ�����
    private float _gravity = -9.8f;
    [Header("�ٶ�")]
    [SerializeField]    public float _runSpeed = 10f;  
    [SerializeField]    public float _walkSpeed = 6f;
    [SerializeField]    private float _jumpFroce = 6f;
    [SerializeField]    private float _targetSpeed = 6f;
    [SerializeField]    private float _currentSpeed = 0f;
    private Vector3 _airDirection;

    /// <summary>
    /// ������淨��صĲ�����ϣ����PlayerManager����
    /// </summary>
    private float _speedFactor = 1f;
    private float _jumpFactor = 1f;
    private float _HPlimit = 100f;

    private float _curHP;

    //��GameManager��õĲ���
    private float _inputThreshold = 0.1f;

    private Vector2 _moveInput;
    private Vector2 _mouseInput;
    private Vector3 _horizontolDirection; //ˮƽ���뷽�����ڳ䵱�м�ֵ����Ӧ������
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
    public Camera SecondCamera; // �ڶ������������ʾ�ֲ�
    [Header("��Ƶ")]
    private AudioSource AudioSource;
    public AudioUtility _Audio;
    public AudioClip _walkClip;
    public AudioClip _jumpClip;
    public AudioClip _runClip;

    private PlayerMovementState _playerMovementState;
    private PlayerMovementState _preMovementState;
    /// <summary>
    /// ����
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

        
        //playerManagerע����
        playerManager._jumpFactorChange += UpdateJumpFactor;
        playerManager._speedFactorChange += UpdateSpeedFactor;
        playerManager._HPlimitChange += UpateHPlimit;
        //gameManagerע����
        gameManager._inputThresholdChange += UpdateInputThreshold;
    }

    private void OnDestroy()
    {
        //playerManagerע����
        playerManager._jumpFactorChange -= UpdateJumpFactor;
        playerManager._speedFactorChange -= UpdateSpeedFactor;
        playerManager._HPlimitChange -= UpateHPlimit;
        //gameManagerע����
        gameManager._inputThresholdChange -= UpdateInputThreshold;
    }

    private void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            //GameManager.Instance.GameOver_Panel(PlayerManager.Instance.CheckAlive());
            Hurt(34);
        }
        //�ƶ�
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

        //_preMovementState = _playerMovementState;//��ŵ�ǰ״̬ ���ж�״̬�Ƿ�ı�
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
        //�¼��ص�
        if(stateChange)
        {
            _playerMovementChange?.Invoke(_playerMovementState);
        }
    }
    private void CountMoveDirection()
    {
        _targetSpeed = _isRunning ? _runSpeed : _walkSpeed;
        //�ж��Ƿ��ڵ��ϣ���ϣ�����������ϲ��ܼ��٣�ֻ���ܵ�����������Ӱ��
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
        //�����ڵ�������ܵ�_deceleration��Ӱ�죬һ������
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

  

    //����PlayerManager
    public PlayerMovementState GetPlayerMovementState()
    {
        return _playerMovementState;
    }
}

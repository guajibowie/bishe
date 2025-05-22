using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


public class Weapon_Shotgun : Weapon
{
    [Header("枪支属性")]
    public float _fireRate;
    public float _shootOffsetRange;
    public float _shootRange;
    public float _fireTimer;
    public ushort _magCapacity;
    public ushort _currentMagBullet;
    public ushort _ammunition;
    public ushort _initialAmmunition;
    public ushort _projectileNumber;
    public float _damage;

    private bool _isAiming;
    private bool _isFiring;
    private bool _isReloading;
    public FireMode _fireMode;

    public Transform _shootPoint;
    public Transform _casingSpawnPoint;

    [Header("VFX属性")]
    private float _muzzleFlashDration = 0.02f;
    private ushort _minMuzzleParticleEmissionRange = 1;
    private ushort _maxMuzzleParticleEmissionRange = 8;
    //private float _bulletForce = 200f;




    //VFX
    [Header("VFX")]
    public ParticleSystem _sparkParticle;
    public ParticleSystem _muzzleParticle;
    public Light _muzzleFlash;
    //子弹
    public GameObject _bulletPrefab;
    public GameObject _casingPrefab;

    [Header("输入")]
    public GameInput inputActions;
    public InputAction FireAction;
    public InputAction ReloadAction;
    public InputAction ChangeFireModeAction;
    public InputAction InspectAction;
    public InputAction AimAction;
    [Header("音频相关")]
    public AudioClips _AudioClips;
    public AudioSource _AudioSource;
    public AudioUtility _Audio;


    [Header("UI")]
    public Image[] _crosshairImages;
    private Vector3[] _expandDirections = new Vector3[] //准心移位方向数组
    {
        Vector3.right * -1,
        Vector3.right,
        Vector3.up,
        Vector3.up * -1
    };
    public TextMeshProUGUI _bulletText;
    public TextMeshProUGUI _fireModeText;
    //准心
    public GameObject _Crosshair;
    [SerializeField] private float _currentExpand = 0;
    [SerializeField] private float _targetExpand = 0;
    [SerializeField] private float _expandFactor = 300f;
    [SerializeField] private float _maxExpand = 50f;
    [SerializeField] private float _minExpand = 0;
    [SerializeField] private float _basicExpand = 0;
    [SerializeField] private float _walkExpand = 10f;
    [SerializeField] private float _runExpand = 20f;
    [SerializeField] private float _fireExpand = 10f;
    [SerializeField] private float _flyExpand = 30f;
    [SerializeField] private float _expandOffset = 1f;

    [Header("手柄设置")]
    private Gamepad _Gamepad;
    private Coroutine _stopMotorCoroutine = null;
    [SerializeField] private float _gamepadMotorDuration = 0.2f;
    [SerializeField] private float _gamepadMotorSpeed_fire = 0.5f;

    [Header("动画相关")]
    public Animator _Animator;

    [SerializeField] private int _fire_Anim = Animator.StringToHash("fire");
    [SerializeField] private int _fireAim_Anim = Animator.StringToHash("aim_fire");
    [SerializeField] private int _takeOutWeapon_Anim = Animator.StringToHash("Base Layer.take_out_weapon");
    [SerializeField] private int _reloadOpen_Anim = Animator.StringToHash("Base Layer.reload_open");
    [SerializeField] private int _reloadInsert_Anim = Animator.StringToHash("Base Layer.reload_insert");
    [SerializeField] private int _reloadClose_Anim = Animator.StringToHash("Base Layer.reload_close");
    [SerializeField] private int _isMoving_Anim = Animator.StringToHash("isMoving");
    [SerializeField] private int _isRunning_Anim = Animator.StringToHash("isRunning");
    [SerializeField] private int _inspect_Anim = Animator.StringToHash("inspect");
    [SerializeField] private int _isAiming_Anim = Animator.StringToHash("isAiming");
    [SerializeField] private int _reloadCompleted = Animator.StringToHash("reloadCompleted");
    [SerializeField] private int _isGrounded_Anim = Animator.StringToHash("isGrounded");
    [SerializeField] private float _fire_Anim_Dura = 0.1f;

    [Header("瞄准相关")]
    [Tooltip("腰射位置")]public Vector3 _initalArmTransform;
    [Tooltip("瞄准位置")]public Vector3 _aimArmTransform;
    [Tooltip("瞄准倍数")] public float _zoomLevel = 2f;
    [Tooltip("正常相机视野")] public float _normalCameraFOV = 60f;
    [Tooltip("瞄准时间")] public float _aimDuration = 0.5f;


    //脚本

    public PlayerMovement _PlayerMovement;

    public PlayerManager _PlayerManager;
    public Camera _GunCamera;
    public Camera _MainCamera;

    private void Awake()
    {

        //组件，类
        _MainCamera = Camera.main;
        _PlayerMovement = GetComponentInParent<PlayerMovement>();
        _PlayerManager = PlayerManager.Instance;

        _AudioSource = GetComponent<AudioSource>();
        _Audio = GetComponent<AudioUtility>();

        _Animator = GetComponent<Animator>();

        _Gamepad = Gamepad.current;


        //输入事件
        inputActions = new GameInput();
        FireAction = inputActions.Player.Fire;
        ReloadAction = inputActions.Player.Reload;
        ChangeFireModeAction = inputActions.Player.ChangeFireMode;
        InspectAction = inputActions.Player.Inspect;
        AimAction = inputActions.Player.Aim;

        FireAction.performed += FirePerformed;
        FireAction.canceled += FireCanceled;
        ReloadAction.performed += ReloadPerformed;
        ChangeFireModeAction.performed += ChangeFireMode;
        InspectAction.performed += Inspect;
        AimAction.performed += AimInPerformed;
        //

        _isReloading = false;
        _isFiring = false;
        _fireTimer = _fireRate; //希望第一枪没有延迟
        _ammunition = _initialAmmunition;
        _currentMagBullet = _magCapacity;
        _fireMode = FireMode.AUTO ;

        _initalArmTransform = transform.localPosition;

        
    }
    private void OnEnable()
    {
        InitCamera();
        CrosshairEnable();
        _isAiming = false;
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        UpdateUI();
    }
    void Update()
    {
        UpdateTargetExpand();
        ApplyCrosshairExpand();
        SyncShootOffsetRange();
        UpdateUI();
        SetPlayerStateToAnimator();
    }
    private void FixedUpdate()
    {
        if (_isFiring)
        {
            _fireTimer += Time.fixedDeltaTime;
            if (_fireTimer > _fireRate)
            {
                Fire();
                _fireTimer -= _fireRate;
            }
        }
    }
    private void UpdateTargetExpand()
    {
        _targetExpand = _basicExpand;
        switch (_PlayerManager._playerMovementState)
        {
            case PlayerMovementState.IDLE:
                break;
            case PlayerMovementState.WALKING:
                _targetExpand += _walkExpand;
                break;
            case PlayerMovementState.RUNNING:
                _targetExpand += _runExpand;
                break;
            case PlayerMovementState.FLYING:
                _targetExpand += _flyExpand;
                break;
        }
        if (_isFiring)
        {
            _targetExpand += _fireExpand;
        }

        _targetExpand = Mathf.Clamp(_targetExpand, _minExpand, _maxExpand);
    }

    private void SyncShootOffsetRange()
    {
        _shootOffsetRange = 0f;
        if (!_isAiming)
        {
            _shootOffsetRange += (_targetExpand - _fireExpand) / 100 + 0.05f;
        }
    }

    private void ApplyCrosshairExpand()
    {
        if (_currentExpand < _targetExpand - _expandOffset)
        {
            CrosshairExpand(_expandFactor);
        }
        else if(_currentExpand > _targetExpand)
        {
            CrosshairExpand( - _expandFactor);
        }
    }
    public void CrosshairExpand(float expandFactor)
    {
        if(expandFactor == 0)
        {
            return;
        }
        // 因为最后一个是中心点，所以需要 -1
       for( int i = 0; i < _crosshairImages.Length - 1; i ++)
        {
            _crosshairImages[i].transform.localPosition += expandFactor * _expandDirections[i] * Time.deltaTime;
        }
        _currentExpand += expandFactor * Time.deltaTime;
    }
    public override void AimIn()
    {
        _isAiming = true;
        transform.localPosition = _aimArmTransform;
    }
    public void AimInFOVchange()
    {
        StartCoroutine(AimFOVchange(_normalCameraFOV / _zoomLevel));
        GameManager.Instance.ChangeMouseSensitiviity(GameManager.Instance._initalMouseSensitivity / _zoomLevel);
    }
    public override void AimOut()
    {
        _isAiming = false;
        transform.localPosition = _initalArmTransform;
        StartCoroutine(AimFOVchange(_normalCameraFOV));
        GameManager.Instance.ChangeMouseSensitiviity(GameManager.Instance._initalMouseSensitivity);
    }

    public void CrosshairEnable()
    {
        _Crosshair.SetActive(true);
    }

    public void CrosshairDisable()
    {
        _Crosshair.SetActive(false);
    }

    public override void Fire()
    {
        if (_fireTimer < _fireRate || _currentMagBullet <= 0 || _Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _takeOutWeapon_Anim)
        {
            if(_currentMagBullet <= 0 && _isFiring)
            {
                _isFiring = false;
            }
            return;
        }
        shoot();

        if (_fireMode == FireMode.SEMI)
        {
            _isFiring = false;
        }
    }
    
    public void shoot()
    {
        for(int i = 0; i < _projectileNumber; i ++)
        {

            RaycastHit _hit;
            Vector3 _shootDirection = _shootPoint.forward;
            if (_shootOffsetRange != 0)
            {
                _shootDirection += _shootPoint.TransformDirection(new Vector3(UnityEngine.Random.Range(-_shootOffsetRange,_shootOffsetRange), UnityEngine.Random.Range(-_shootOffsetRange, _shootOffsetRange)));
            }
            if (Physics.Raycast(_shootPoint.position, _shootDirection,out _hit, _shootRange))
            {
                if (_hit.transform.CompareTag("Enemy01") || _hit.transform.CompareTag("Enemy02"))
                {
                    _hit.transform.GetComponent<Enemy01>().OnHurt(_damage);
                }
            }
            GameObject _bulletClone;
            _bulletClone = Instantiate(_bulletPrefab,_hit.point, _shootPoint.transform.rotation);
        }
        Instantiate(_casingPrefab, _casingSpawnPoint.transform.position, _casingSpawnPoint.transform.rotation);
        if (_isAiming)
        {
            _Animator.CrossFadeInFixedTime(_fireAim_Anim, _fire_Anim_Dura);
        }
        else
        {
            _Animator.CrossFadeInFixedTime(_fire_Anim,_fire_Anim_Dura);
        }

        //Rigidbody _bulletClone = Instantiate<Rigidbody>(_bulletPrefab.GetComponent<Rigidbody>(),_shootPoint.transform.position,_shootPoint.transform.rotation);
        //_bulletClone.AddForce((_bulletClone.transform.forward + _shootDirection) * _bulletForce, ForceMode.Impulse);
        StartCoroutine(FireVFX());
        _Audio.PlayClip(_AudioClips._shoot, _AudioClips._shootVolume);
        _currentMagBullet -= 1;
    }

    private void ChangeFireMode()
    {
        Array fireModes = Enum.GetValues(typeof(FireMode));
        int targetIndex = (Array.IndexOf(fireModes, _fireMode) + 1) % fireModes.Length;
        _fireMode = (FireMode)fireModes.GetValue(targetIndex);
    }


    //实际换弹实现,在动画中使用了事件

    public override void Reload()
    {
        Debug.Log("Reloading" + _magCapacity + _currentMagBullet);
        if (_ammunition <= 0) {
            if(_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _reloadInsert_Anim)
            {
                _Animator.SetBool(_reloadCompleted,true);
            }
            _isReloading = false;
            return; 
        }
        _currentMagBullet += 1;
        _ammunition -= 1;
        if (_currentMagBullet >= _magCapacity)
        {
            _Animator.SetBool(_reloadCompleted, true);
            _isReloading = false;
            return;
        }
        
    }

    public override void View()
    {
        if (_PlayerManager.GetIsRunning())
        {
            return;
        }
        _Animator.SetTrigger(_inspect_Anim);
    }

    private void StopGamepadMotor()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0, 0);
        }

    }

    private void SetPlayerStateToAnimator()
    {
        _Animator.SetBool(_isMoving_Anim,_PlayerManager.GetMoving());
        _Animator.SetBool(_isRunning_Anim, _PlayerManager.GetIsRunning());
        _Animator.SetBool(_isGrounded_Anim, _PlayerManager.GetIsGrounded());
    }
    //输入回调区
    public void AimInPerformed(InputAction.CallbackContext context)
    {
        if(_isReloading) return;
        if (_isAiming)
        {
            AimOut();
            
        }
        else
        {
            AimIn();
           
        }
        _Animator.SetBool(_isAiming_Anim, _isAiming);
    }
    public void ReloadPerformed(InputAction.CallbackContext context)
    {
        if (_ammunition > 0 && _currentMagBullet < _magCapacity)
        {
            if (_isAiming)
            {
                AimOut();
                CrosshairEnable();
                _Animator.SetBool(_isAiming_Anim, _isAiming);
            }
            _isReloading = true;
            _Animator.SetBool(_reloadCompleted, false);
            _Animator.Play(_reloadOpen_Anim, 0, 0);
            _Audio.PlayClip(_AudioClips._reloadOpen);
        }
    }
    private void ChangeFireMode(InputAction.CallbackContext context)
    {
        ChangeFireMode();
    }
    private void FirePerformed(InputAction.CallbackContext context)
    {
        if (_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _takeOutWeapon_Anim)
        {
            return;
        }
        if(_currentMagBullet > 0)
        {
            _isFiring = true;
        }
        if (Gamepad.current != null && _currentMagBullet >= 1)
        {
            Gamepad.current.SetMotorSpeeds(_gamepadMotorSpeed_fire, _gamepadMotorSpeed_fire);
            if(_fireMode != FireMode.AUTO)
            {
                if (_stopMotorCoroutine != null)
                {
                    StopCoroutine(_stopMotorCoroutine);
                }
                _stopMotorCoroutine = StartCoroutine(StopGmaepadMotorDelay(_gamepadMotorDuration));
            }
            
        }
    }
    private void FireCanceled(InputAction.CallbackContext context)
    {
        _isFiring = false;
        if(_fireMode == FireMode.AUTO)
        {
            StartCoroutine(StopGmaepadMotorDelay(_gamepadMotorDuration));
        }
        _fireTimer = _fireRate;
    }


    private void Inspect(InputAction.CallbackContext context)
    {
        View();
    }
    //协程
    private IEnumerator AimFOVchange(float targetFOV)
    {
        float elapsedTime = 0f;

        float startFOV = _GunCamera.fieldOfView;
        while (elapsedTime < _aimDuration)
        {
            elapsedTime += Time.deltaTime;
            elapsedTime += Time.deltaTime; // 累计时间
            float fractionComplete = elapsedTime / _aimDuration; // 当前进度
            _MainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, fractionComplete); // 平滑过渡
            _GunCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, fractionComplete); // 平滑过渡
            yield return null;
        }
        _MainCamera.fieldOfView = targetFOV;
        _GunCamera.fieldOfView = targetFOV;

    }
    private IEnumerator FireVFX()
    {
        _muzzleFlash.enabled = true;
        _muzzleParticle.Emit(1);
        _sparkParticle.Emit(UnityEngine.Random.Range(_minMuzzleParticleEmissionRange, _maxMuzzleParticleEmissionRange));
        yield return new WaitForSeconds(_muzzleFlashDration);
        _muzzleFlash.enabled = false;
    }

    private IEnumerator SetGamepadMotor(float leftMotor,float rightMotor, float duration)
    {
        if(Gamepad.current == null)
        {
            yield break;
        }
        Gamepad.current.SetMotorSpeeds(leftMotor, rightMotor);
        yield return new WaitForSeconds(duration);
        StopGamepadMotor();
    }

    private IEnumerator StopGmaepadMotorDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopGamepadMotor();
        _stopMotorCoroutine = null;
    }

    //动画函数
    public void ReloadInsertAudio()
    {
        _Audio.PlayClip(_AudioClips._reloadInsert);
    }

    public void ReloadCloseAudio()
    {
        _Audio.PlayClip(_AudioClips._reloadClose);
    }
    //

    public void UpdateUI()
    {
        if(_bulletText != null)
        {
            _bulletText.text = _currentMagBullet + "/" + _ammunition;

        }
        if(_fireModeText != null)
        {

            _fireModeText.text = _fireMode.ToString();
        }
    }

    public void InitCamera()
    {
        _MainCamera.fieldOfView = 60f;
        _GunCamera.fieldOfView = 60f;
    }
}

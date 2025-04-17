using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;



public class Weapon_Automatic : Weapon
{
    [Header("ǹ֧����")]
    public float _fireRate;
    public float _shootOffsetRange;
    public float _shootRange;
    public float _fireTimer;
    public ushort _magCapacity;
    public ushort _currentMagBullet;
    public ushort _ammunition;
    public ushort _initialAmmunition;

    private bool _isAiming;
    private bool _isFiring;
    public FireMode _fireMode;

    public Transform _shootPoint;
    public Transform _casingSpawnPoint;

    [Header("VFX����")]
    private float _muzzleFlashDration = 0.02f;
    private ushort _minMuzzleParticleEmissionRange = 1;
    private ushort _maxMuzzleParticleEmissionRange = 8;
    private float _bulletForce = 200f;




    //VFX
    [Header("VFX")]
    public ParticleSystem _sparkParticle;
    public ParticleSystem _muzzleParticle;
    public Light _muzzleFlash;
    //�ӵ�
    public GameObject _bulletPrefab;
    public GameObject _casingPrefab;

    [Header("����")]
    public GameInput inputActions;
    public InputAction FireAction;
    public InputAction ReloadAction;
    public InputAction ChangeFireModeAction;
    public InputAction InspectAction;
    public InputAction AimAction;
    [Header("��Ƶ���")]
    public AudioClips _AudioClips;
    public AudioSource _AudioSource;
    public AudioUtility _Audio;


    [Header("UI")]
    public Image[] _crosshairImages;
    private Vector3[] _expandDirections = new Vector3[] //׼����λ��������
    {
        Vector3.right * -1,
        Vector3.right,
        Vector3.up,
        Vector3.up * -1
    };
    public TextMeshProUGUI _bulletText;
    public TextMeshProUGUI _fireModeText;
    //׼��
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

    [Header("�ֱ�����")]
    private Gamepad _Gamepad;
    private Coroutine _stopMotorCoroutine = null;
    [SerializeField] private float _gamepadMotorDuration = 0.2f;
    [SerializeField] private float _gamepadMotorSpeed_fire = 0.5f;

    [Header("�������")]
    public Animator _Animator;

    [SerializeField] private int _fire_Anim = Animator.StringToHash("fire");
    [SerializeField] private int _fireAim_Anim = Animator.StringToHash("aim_fire");
    [SerializeField] private int _takeOutWeapon_Anim = Animator.StringToHash("Base Layer.take_out_weapon");
    [SerializeField] private int _reload_Anim = Animator.StringToHash("Base Layer.reload_ammo_left");
    [SerializeField] private int _reloadOutOfAmmu_Anim = Animator.StringToHash("Base Layer.reload_out_of_ammo");
    [SerializeField] private int _isMoving_Anim = Animator.StringToHash("isMoving");
    [SerializeField] private int _isRunning_Anim = Animator.StringToHash("isRunning");
    [SerializeField] private int _inspect_Anim = Animator.StringToHash("inspect");
    [SerializeField] private int _isAiming_Anim = Animator.StringToHash("isAiming");
    [SerializeField] private int _isGrounded_Anim = Animator.StringToHash("isGrounded");
    [SerializeField] private float _fire_Anim_Dura = 0.1f;

    [Header("��׼���")]
    [Tooltip("����λ��")]public Vector3 _initalArmTransform;
    [Tooltip("��׼λ��")]public Vector3 _aimArmTransform;
    [Tooltip("��׼����")] public float _zoomLevel = 2f;
    [Tooltip("���������Ұ")] public float _normalCameraFOV = 60f;
    [Tooltip("��׼ʱ��")] public float _aimDuration = 0.5f;


    //�ű�

    public PlayerMovement _PlayerMovement;

    public Camera _GunCamera;
    public Camera _MainCamera;

    private void Awake()
    {

        //�������
        _MainCamera = Camera.main;
        _PlayerMovement = GetComponentInParent<PlayerMovement>();

        _AudioSource = GetComponent<AudioSource>();
        _Audio = GetComponent<AudioUtility>();

        _Animator = GetComponent<Animator>();

        _Gamepad = Gamepad.current;


        //�����¼�
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


        _isFiring = false;
        _fireTimer = _fireRate; //ϣ����һǹû���ӳ�
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
        switch (PlayerManager.Instance._playerMovementState)
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
        // ��Ϊ���һ�������ĵ㣬������Ҫ -1
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

        if(_fireMode == FireMode.SEMI)
        {
            _isFiring = false;
        }
    }
    public void shoot()
    {
        RaycastHit _hit;
        Vector3 _shootDirection = _shootPoint.forward;
        if (_shootOffsetRange != 0)
        {
            _shootDirection += _shootPoint.TransformDirection(new Vector3(UnityEngine.Random.Range(-_shootOffsetRange,_shootOffsetRange), UnityEngine.Random.Range(-_shootOffsetRange, _shootOffsetRange)));
        }
        if (Physics.Raycast(_shootPoint.position, _shootDirection,out _hit, _shootRange))
        {
            Debug.Log("Fire");
            Debug.DrawLine(_shootPoint.position, _hit.point,Color.red,5f);
        }
        GameObject _bulletClone;
        _bulletClone = Instantiate(_bulletPrefab, _shootPoint.transform.position, _shootPoint.transform.rotation);
        _bulletClone.GetComponent<Rigidbody>().AddForce((_bulletClone.transform.forward + _shootDirection) * _bulletForce, ForceMode.Impulse);
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


    //ʵ�ʻ���ʵ��,�ڶ�����ʹ�����¼�

    public override void Reload()
    {
        if (_ammunition <= 0) {
            Debug.Log("Got no Ammunition");    
            return; 
        }
        ushort Add = (ushort)Mathf.Min(_magCapacity - _currentMagBullet, _ammunition);
        _currentMagBullet += Add;
        _ammunition -= Add;
    }

    public override void View()
    {
        if (PlayerManager.Instance.GetIsRunning())
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
        _Animator.SetBool(_isMoving_Anim,PlayerManager.Instance.GetMoving());
        _Animator.SetBool(_isRunning_Anim, PlayerManager.Instance.GetIsRunning());
        _Animator.SetBool(_isGrounded_Anim, PlayerManager.Instance.GetIsGrounded());
    }
    //����ص���
    public void AimInPerformed(InputAction.CallbackContext context)
    {
        if(_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _reload_Anim || _Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _reloadOutOfAmmu_Anim) return;
        if (_isAiming)
        {
            AimOut();
            
        }
        //else if(!_PlayerManager.GetIsRunning())
        else
        {
            AimIn();
           
        }
        _Animator.SetBool(_isAiming_Anim, _isAiming);
    }
    public void ReloadPerformed(InputAction.CallbackContext context)
    {
        if (_ammunition > 0)
        {
            if (_isAiming)
            {
                AimOut();
                CrosshairEnable();
                _Animator.SetBool(_isAiming_Anim, _isAiming);
            }
            if (_currentMagBullet > 0)
            {
                _Animator.Play(_reload_Anim, 0, 0);
                _Audio.PlayClip(_AudioClips._reload);
            }
            else
            {
                _Animator.Play(_reloadOutOfAmmu_Anim, 0, 0);
                _Audio.PlayClip(_AudioClips._reloadOutOfAmmu);
            }

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
    //Э��
    private IEnumerator AimFOVchange(float targetFOV)
    {
        float elapsedTime = 0f;

        float startFOV = _GunCamera.fieldOfView;
        while (elapsedTime < _aimDuration)
        {
            elapsedTime += Time.deltaTime;
            elapsedTime += Time.deltaTime; // �ۼ�ʱ��
            float fractionComplete = elapsedTime / _aimDuration; // ��ǰ����
            _MainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, fractionComplete); // ƽ������
            _GunCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, fractionComplete); // ƽ������
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

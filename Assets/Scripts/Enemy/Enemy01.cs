using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy01 : MonoBehaviour
{
    public FsmSystem<Enemy01> _FsmSystem;

    [Header("¹ÖÎïÊôÐÔ")]
    public float _HPlimit;
    public float _curHP;

    public float _chaseRange; //×·Öð·¶Î§
    public float _walkRange;
    public float _idleTime;
    public float _rotationSpeed;
    public float _attack;
    private float _lastAttackTime = 0f;
    public float _attackCooldown;

    public float _stuckThresholdTime;
    public float _stuckThresholdDistance;

    private float _stuckTimer;


    public bool _isCatching;

    private Vector3 _lastPosition;
    public NavMeshAgent _NavMeshAgent;
    public Animator _Animator;

    public float _attack_anim_dra;

    public int _attack_anim;
    public int _hitReaction_anim;
    public int _isMoving_anim;
    public int _isDeaded_anim;


    private void OnEnable()
    {
        _curHP = _HPlimit;
    }
    void Start()
    {

        _curHP = _HPlimit;
        _lastPosition = transform.position;
        _isCatching = false;

        _attack_anim = Animator.StringToHash("Armature|Attack");
        _hitReaction_anim = Animator.StringToHash("Armature|Hit_reaction");
        _isMoving_anim = Animator.StringToHash("IsMoving");
        _isDeaded_anim = Animator.StringToHash("IsDeaded");

        _FsmSystem = new FsmSystem<Enemy01>(this);
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _NavMeshAgent.updatePosition = false;
        if(NavMesh.SamplePosition(transform.position,out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            _NavMeshAgent.Warp(hit.position);
        }

        _FsmSystem.AddState("idle", new IdleState());
        _FsmSystem.AddState("walk", new WalkState());
        _FsmSystem.AddState("chase", new ChaseState());
    }

    // Update is called once per frame
    void Update()
    {
        if(_curHP <= 0f)
        {
            Destroy(gameObject);
        }
        if(_FsmSystem._currentStateID != "idle")
        {
            CheckStuck();
        }
        SyncAgentRotation();
        if (CheckRange())
        {
            if(_FsmSystem._currentStateID != "chase")
            {
                _FsmSystem.ChangeState("chase");
            }
        }
        else if(_FsmSystem._currentStateID ==  "chase")
        {
            _FsmSystem.ChangeState("idle");
        }
        _FsmSystem.Update();
    }

    private void OnAnimatorMove()
    {
        Vector3 _position = _Animator.rootPosition;
        _NavMeshAgent.nextPosition = new Vector3(_position.x, _NavMeshAgent.nextPosition.y, _position.z);
        _position.y = _NavMeshAgent.nextPosition.y;
        transform.position = _position;
    }
    /// <summary>
    /// ¼ì²â½ÇÉ«ÊÇ·ñ½øÈë×·»÷·¶Î§
    /// </summary>
    /// <returns></returns>
    public bool CheckRange()
    {
        float px = PlayerManager.Instance._playerPosition.x;
        float pz = PlayerManager.Instance._playerPosition.z;
        return Mathf.Pow((px - transform.position.x),2) + Mathf.Pow((pz - transform.position.z), 2) < Mathf.Pow(_chaseRange,2);
    }
    public void CheckStuck()
    {
        if (_isCatching || _FsmSystem._currentStateID == "idle") return;
        float moveDistance = Vector3.Distance(_lastPosition, transform.position);
        if(moveDistance < _stuckThresholdDistance)
        {
            _stuckTimer += Time.deltaTime;
        }
        else
        {
                _stuckTimer = 0;
        }

        if (_stuckTimer > _stuckThresholdTime)
        {
            _NavMeshAgent.SetDestination(GetNavMeshPoint());
            _Animator.SetBool(_isMoving_anim, true);
            _stuckTimer = 0;
        }

        _lastPosition = transform.position;
    }
    public Vector3 GetNavMeshPoint(Vector3 target = default)
    {

        NavMeshHit navMeshHit;
        if(target != default)
        {
            if (NavMesh.SamplePosition(target, out navMeshHit, _walkRange, NavMesh.AllAreas))
            {
                return navMeshHit.position;
            }

        }
        else
        {

            Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * _walkRange + transform.position;
            randomPosition.y = transform.position.y;
            if (NavMesh.SamplePosition(randomPosition, out navMeshHit, _walkRange, NavMesh.AllAreas))
            {
                return navMeshHit.position;
            }
        }
        return transform.position;
    }

    public void SyncAgentRotation()
    {
        if (_NavMeshAgent.velocity.magnitude <= 0.1f) return;
        Quaternion targetRotation = Quaternion.LookRotation(_NavMeshAgent.velocity);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _isCatching = true;
            if (_lastAttackTime + _attackCooldown <= Time.time)
            {
                _Animator.CrossFade(_attack_anim, 0.1f);
                StartCoroutine(Attack(other.transform, 1f));
                //collision.transform.GetComponent<PlayerCollisionControl>().OnHurt(_attack);
                _lastAttackTime = Time.time;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
            if (other.gameObject.CompareTag("Player"))
            {
                _isCatching = false;
            }
    }
    
    
    public void OnHurt(float Damage)
    {
        _curHP -= Damage;
        _Animator.Play(_hitReaction_anim);
        _curHP = Mathf.Clamp(_curHP, 0, _HPlimit);
        if(_curHP <= 0)
        {
            _Animator.SetBool(_isDeaded_anim, true);
            EnemyManager.Instance.ReturnToPool(gameObject);
            DropItemManager.Instance.CreateDropItem(transform.position);
        }
        Debug.Log("enemy hurt" + Damage + "\\" + _curHP);
    }

    //public void OnCollisionStay(Collision collision)
    //{
    //    if(collision.gameObject.CompareTag("Player"))
    //    {
    //        _isCatching = true;
    //        if (_lastAttackTime + _attackCooldown <= Time.time)
    //        {
    //            _Animator.CrossFade(_attack_anim,0.1f);
    //            StartCoroutine(Attack(collision.transform, 1f));
    //            //collision.transform.GetComponent<PlayerCollisionControl>().OnHurt(_attack);
    //            _lastAttackTime = Time.time;
    //        }
    //    }
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        _isCatching = false;
    //    }
    //}

    IEnumerator Attack(Transform target,float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_isCatching)
        {
            Debug.Log("atk" + transform.name);
            target.GetComponent<PlayerCollisionControl>().OnHurt(_attack);
        }
    }
}


class IdleState : State<Enemy01>
{
    public float _lastTime;
    public float _idleTime = 3f;
    
    public override void OnEnter()
    {
        _param._Animator.SetBool(_param._isMoving_anim, false);
        _lastTime = Time.time;

    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {
       if(_lastTime + _idleTime < Time.time)
        {
            fsm.ChangeState("walk");
        }
        
    }
}
class WalkState : State<Enemy01>
{

    public float _minDistance = 0.5f;
    public override void OnEnter()
    {
        _param._Animator.SetBool(_param._isMoving_anim, true);
        Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * _param._walkRange + _param.transform.position;
        randomPosition.y = _param.transform.position.y;
        NavMeshHit navMeshHit;
        if(NavMesh.SamplePosition(randomPosition,out navMeshHit, _param._walkRange, NavMesh.AllAreas))
        {
            _param._NavMeshAgent.SetDestination(navMeshHit.position);
        }

    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
        if (_param._NavMeshAgent.remainingDistance <= _param._NavMeshAgent.stoppingDistance)
        {
            fsm.ChangeState("idle");
        }
    }
}

class ChaseState : State<Enemy01>
{
    Vector3 _targetPosition;
    public override void OnEnter()
    {
        _targetPosition = _param.GetNavMeshPoint(PlayerManager.Instance._playerPosition);
        _param._NavMeshAgent.SetDestination(_targetPosition);
        _param._Animator.SetBool(_param._isMoving_anim, true);
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {
        if(!_param._isCatching)
        {
            if (_param._NavMeshAgent.isStopped)
            {
                _param._NavMeshAgent.isStopped = false;
            }
            _targetPosition = _param.GetNavMeshPoint(PlayerManager.Instance._playerPosition);
            _param._NavMeshAgent.SetDestination(_targetPosition);
            _param._Animator.SetBool(_param._isMoving_anim, true);
        }
        else
        {
            _param._Animator.SetBool(_param._isMoving_anim, false);
            _param._NavMeshAgent.isStopped = true;
        }

    }
}
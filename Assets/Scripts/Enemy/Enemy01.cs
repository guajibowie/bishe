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
    public float _chaseRange; //×·Öð·¶Î§
    public float _walkRange;
    public float _idleTime;
    public float _rotationSpeed;

    public float _stuckThresholdTime;
    public float _stuckThresholdDistance;

    private float _stuckTimer;
    private Vector3 _lastPosition;
    public NavMeshAgent _NavMeshAgent;
    void Start()
    {
        _lastPosition = transform.position;
        _FsmSystem = new FsmSystem<Enemy01>(this);
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _NavMeshAgent.updateRotation = false;
        if(NavMesh.SamplePosition(transform.position,out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            _NavMeshAgent.Warp(hit.position);
        }

        _FsmSystem.AddState("idle", new IdleState());
        _FsmSystem.AddState("walk", new WalkState());
    }

    // Update is called once per frame
    void Update()
    {
        if(_FsmSystem._currentStateID != "idle")
        {
            CheckStuck();
        }
        SyncAgentRotation();
        _FsmSystem.Update();    
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
            Debug.Log("stuck!");
            _NavMeshAgent.SetDestination(GetNavMeshPoint());
            _stuckTimer = 0;
        }

        _lastPosition = transform.position;
    }
    public Vector3 GetNavMeshPoint()
    {
        Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * _walkRange + transform.position;
        randomPosition.y = transform.position.y;
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(randomPosition, out navMeshHit, _walkRange, NavMesh.AllAreas))
        {
            return navMeshHit.position;
        }
        else
        {
            return transform.position;
        }
    }

    public void SyncAgentRotation()
    {
        if (_NavMeshAgent.velocity.magnitude <= 0.1f) return;
        Quaternion targetRotation = Quaternion.LookRotation(_NavMeshAgent.velocity);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }
}


class IdleState : State<Enemy01>
{
    public float _lastTime;
    public float _idleTime = 3f;
    
    public override void OnEnter()
    {
        _lastTime = Time.time;
        Debug.Log("Enter Idle");
    }

    public override void OnExit()
    {
        Debug.Log("Exit Idle");
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
        if (_param._NavMeshAgent.remainingDistance < _minDistance)
        {
            fsm.ChangeState("idle");
        }
    }
}
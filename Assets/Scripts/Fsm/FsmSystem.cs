using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FsmSystem<T>
{
    private Dictionary<string,State<T>> _states = new Dictionary<string,State<T>>();


    public FsmSystem(T param)
    {
        _param = param;
    }
    public string _currentStateID;
    public State<T> _currentState;
    public T _param;
    public void AddState(string stateID,State<T> state)
    {
        state.fsm = this;
        _states[stateID] = state;
        if(_currentStateID is null)
        {
            _currentStateID = stateID;
            _currentState = state;
            _currentState.OnEnter();
        }
    }


    public void RemoveState(string stateID)
    {
        if (_states.ContainsKey(stateID))
        {
            if (_states[stateID] == _currentState)
            {
                _currentState = null;
                _currentStateID = null;
            }
            _states[stateID].fsm = null;
            _states.Remove(stateID);
        }
    }

    public void ChangeState(string stateID)
    {
        if (_currentStateID == stateID) return;
        if (!_states.ContainsKey(stateID)) return;
        if(_currentState != null)
        {
            _currentState.OnExit();
        }
        _currentStateID = stateID;
        _currentState = _states[stateID];
        _currentState.OnEnter();
    }


    public void Update()
    {
        _currentState?.OnUpdate();
    }
}

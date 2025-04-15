using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FsmSystem : MonoBehaviour
{
    private Dictionary<string,State> _states = new Dictionary<string,State>();

    public string _currentStateID;
    public State _currentState;
    public void AddState(string stateID,State state)
    {
        if(_currentStateID is null)
        {
            _currentStateID = stateID;
            _currentState = state;
        }
        _states[stateID] = state;
    }


    public void RemoveState(string stateID)
    {
        if (_states.ContainsKey(stateID))
        {
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
}

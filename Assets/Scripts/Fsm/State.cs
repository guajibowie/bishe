using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State<T>
{
    public FsmSystem<T> fsm;
    public T _param => fsm._param;
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void OnUpdate();

}

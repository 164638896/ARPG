using UnityEngine;
using System.Collections;

public class RoleDeathState : IState
{
    public RoleDeathState(Role role, Animator animator, StateMachine stateMachine) 
        : base(role, animator, stateMachine, IState.StateType.Death)
    {
        AddStateHash(Animator.StringToHash("Death"));
    }

    public override void OnAniEnter(int stateHash)
    {
        //mChangeState = false;
    }

    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
    {
        base.ExecuteState(stateMachine, prevState, param1, param2);
        //mChangeState = false;
    }

    public override void OnLeave(int stateHash)
    {
        //mChangeState = true;
        base.OnLeave(stateHash);
    }
}
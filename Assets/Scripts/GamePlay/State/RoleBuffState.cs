using UnityEngine;
using System.Collections;

public class RoleBuffState : StateByTime
{
    public RoleBuffState(Role role, Animator animator, StateMachine stateMachine) 
        : base(role, animator, stateMachine, IState.StateType.Buff)
    {

    }

    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
    {
        base.ExecuteState(stateMachine, prevState, param1, param2);

        PlayAnimation((string)param1, 0);
        mDuringTime = (float)param2;
        mBeginTime = Time.fixedTime;
        if(mDuringTime > 0)
        {
            mChangeState = false;
        }
    }

    public override void OnLeave(int stateHash)
    {
        mChangeState = true;
        base.OnLeave(stateHash);
    }
}
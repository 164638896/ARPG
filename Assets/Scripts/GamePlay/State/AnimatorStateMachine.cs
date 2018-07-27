using System;
using UnityEngine;
using System.Collections;

public class AnimatorStateMachine : StateMachineBehaviour
{

    public Action<int> EnterStateCallBack;
    public Action<int> ExitStateCallBack;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (EnterStateCallBack != null)
        {
            EnterStateCallBack(stateInfo.shortNameHash);
        }
    }

    //public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    base.OnStateMachineEnter(animator, stateMachinePathHash);

    //    if (EnterStateCallBack != null)
    //    {
    //        EnterStateCallBack(stateMachinePathHash);
    //    }
    //}

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (ExitStateCallBack != null)
        {
            ExitStateCallBack(stateInfo.shortNameHash);
        }
    }

    //public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //    base.OnStateMachineExit(animator, stateMachinePathHash);

    //    if (ExitStateCallBack != null)
    //    {
    //        ExitStateCallBack(stateMachinePathHash);
    //    }
    //}
}
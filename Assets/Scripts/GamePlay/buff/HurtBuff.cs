using UnityEngine;
using UnityEngine.AI;

public class HurtBuffConfig : BuffConfig
{
    public string mStateName;
}

public class HurtBuff : IBuff
{
    public HurtBuffConfig mHurtBuffConfigInfo;

    public HurtBuff() : base()
    {
       
    }

    public override void OnEnter()
    {
        mHurtBuffConfigInfo = mBuffConfig as HurtBuffConfig;

        base.OnEnter();
        if(!mReceRole.mStateMachine.SwitchState(IState.StateType.Buff, mHurtBuffConfigInfo.mStateName, 0.0f))
        {
            //mReceRole.mStateMachine.SetNextState(IState.StateType.Buff, mHurtBuffConfigInfo.mStateName, 0.0f);
        }

        mReceRole.mRoleData.SetTargetRole(mSendRole.mRoleData.mInstId);

        // 死亡应该服务器通知.单机暂时写在这里
        int hurt = mSendRole.mRoleData.mAtk - mReceRole.mRoleData.mDef;
        if(hurt > 0)
        {
            mReceRole.mRoleData.mHp -= hurt;
            if (mReceRole.mRoleData.mHp <= 0)
            {
                mReceRole.mRoleData.mHp = 0;
                mReceRole.mAnimator.SetBool("Death", true);
            }
        }
    }

    public override void OnLeave()
    {
        base.OnLeave();

    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }
}
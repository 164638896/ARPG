using UnityEngine;
using UnityEngine.AI;

public class ControlBuffConfig : BuffConfig
{
    public bool mClearBuff;
    public bool mEnableBehavior;
    public bool mEnableMove;
    public bool mEnableHurt;
    public bool mEnableSelect;
}

public class ControlBuff : IBuff
{
    public ControlBuffConfig mControlBuffConfig;

    public override void OnEnter()
    {
        mControlBuffConfig = mBuffConfig as ControlBuffConfig;

        base.OnEnter();

        if(mControlBuffConfig.mClearBuff)
        {
            mReceRole.mBuffSystem.ClearAllBuff();
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
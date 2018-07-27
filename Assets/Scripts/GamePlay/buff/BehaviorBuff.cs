using UnityEngine;

public class BehaviorBuffConfig : BuffConfig
{
    public string mEffect;
    public bool mCanMove; // 能否移动
    public bool mCanAtk;  // 能否攻击
}

public class BehaviorBuff : IBuff
{
    public BehaviorBuffConfig mBehaviorBuffConfig;

    public override void OnEnter()
    {
        mBehaviorBuffConfig = mBuffConfig as BehaviorBuffConfig;
        base.OnEnter();
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
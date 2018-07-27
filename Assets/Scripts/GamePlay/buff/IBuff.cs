using UnityEngine;


public class BuffConfig
{
    public IBuff.BuffType mType;
    public int mTypeId;
    public string mName;
    public float mDuration;
}

public abstract class IBuff
{
    public enum BuffType
    {
        None,
        Behavior,
        Move,
        Hurt,
        Control,
    }

    public enum BuffAttach
    {
        Self = 1,
        Target = 2,
        Area = 3,
    }

    public enum BuffStage
    {
        Start = 1,
        Hit = 2,
    }

    protected Role mReceRole = null; // buff接收者
    protected Role mSendRole; // buff释放者

    public float mBeginTime = 0.0f;
    public int mInstId = 0;

    public BuffConfig mBuffConfig = null;

    public IBuff()
    {

    }

    public virtual void Init(Role sendRole, Role receRole, BuffConfig info, float beginTime)
    {
        mSendRole = sendRole;
        mReceRole = receRole;
        mBuffConfig = info;
        mBeginTime = beginTime;
    }

    public virtual void OnEnter()
    {

    }

    public virtual void OnLeave()
    {
        mReceRole.mBuffSystem.RemoveBuff(mBuffConfig.mType, mInstId);
    }

    public virtual void OnFixedUpdate()
    {
        if (mBeginTime + mBuffConfig.mDuration <= Time.fixedTime)
        {
            OnLeave();
        }
    }
}

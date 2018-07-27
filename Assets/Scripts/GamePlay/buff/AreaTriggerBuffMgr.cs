using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

//区域buff 是服务端实现的.对于客户端资源在地图上播放个特效
public class AreaTriggerBuffConfig
{
    public int mTypeId;

    public string mName;
    public float mDuration;

    public AreaTriggerBuff.Shape mShape;
    public Vector3 mRadius;

    public float mDelayTime; // 延迟时间
    public float mRepeatRate; // 触发间隔
    public AreaTriggerBuff.Destroy mDestroy;

    public GameObject mEffect;
    public GameObject mDestroyEffect;

    public List<BuffId> mBuffIdList = new List<BuffId>(); // buffId
}

public class AreaTriggerBuff
{
    public enum Shape
    {
        Sphere = 1,
        Box = 2,
    }

    public enum Destroy
    {
        Time = 1,
        TimeOrTrigger = 2,
    }

    Role mSendRole;
    public float mBeginTime = 0.0f;
    public int mInstId = 0;
    public AreaTriggerBuffConfig mConfigInfo = null;
    public Vector3 mBuffPos;

    public virtual void Init(Role sendRole, Vector3 buffPos, AreaTriggerBuffConfig info, int insId, float beginTime)
    {
        mSendRole = sendRole;
        mBuffPos = buffPos;

        mConfigInfo = info;
        mInstId = insId;
        mBeginTime = beginTime;
    }

    public virtual void OnEnter()
    {
        TimeTaskManager.AddTimer((uint)(mConfigInfo.mDelayTime*1000), (int)(mConfigInfo.mRepeatRate * 1000), TriggerBuff);
    }

    public virtual void OnLeave()
    {
        TimeTaskManager.RemoveTimer(TriggerBuff);

        if (mConfigInfo.mEffect != null)
        {
            mConfigInfo.mEffect.SetActive(false);
        }

        if (mConfigInfo.mDestroyEffect != null)
        {
            mConfigInfo.mDestroyEffect.SetActive(false);
            mConfigInfo.mDestroyEffect.transform.position = mBuffPos;
            mConfigInfo.mDestroyEffect.SetActive(true);
        }

        AreaBuffTriggerMgr.singleton.RemoveBuff(mInstId);
    }

    public virtual void OnUpdate()
    {
        if (mBeginTime + mConfigInfo.mDuration + mConfigInfo.mDelayTime <= Time.fixedTime)
        {
            OnLeave();
        }
    }

    protected void TriggerBuff()
    {
        Collider[] array;
        if (mConfigInfo.mShape == Shape.Sphere)
        {
            array = Physics.OverlapSphere(mBuffPos, mConfigInfo.mRadius.x);
        }
        else if (mConfigInfo.mShape == Shape.Box)
        {
            array = Physics.OverlapBox(mBuffPos, mConfigInfo.mRadius);
        }
        else
        {
            return;
        }

        if (mConfigInfo.mEffect != null)
        {
            mConfigInfo.mEffect.SetActive(true);
            mConfigInfo.mEffect.transform.position = mBuffPos;
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Role role = array[i].gameObject.GetComponent<Role>();
            if (role == null)
            {
                continue;
            }

            foreach (var item in mConfigInfo.mBuffIdList)
            {
                if (item.mBuffAttach == IBuff.BuffAttach.Self && mSendRole.mRoleData.mGroupType == role.mRoleData.mGroupType) // 己方
                {
                    if (role.mBuffSystem.EnableSelect())
                    {
                        role.mBuffSystem.AddBuff(mSendRole, item.mType, item.mTypeId);
                    }
                }
                else if (item.mBuffAttach == IBuff.BuffAttach.Target && mSendRole.mRoleData.mGroupType != role.mRoleData.mGroupType) // 敌方
                {
                    if (role.mBuffSystem.EnableSelect())
                    {
                        role.mBuffSystem.AddBuff(mSendRole, item.mType, item.mTypeId);
                        if (mConfigInfo.mDestroy == Destroy.TimeOrTrigger)
                        {
                            OnLeave();
                        }
                    }
                }
            }
        }
    }
}

public class AreaBuffTriggerMgr
{
    private Dictionary<int/*InstId*/, AreaTriggerBuff> mCurrBuffList = new Dictionary<int/*InsId*/, AreaTriggerBuff>();
    private List<int/*InstId*/> mDeathBuffList = new List<int>();
    static private Stack<AreaTriggerBuff> mRecoveryBuffList = new Stack<AreaTriggerBuff>();

    private static int mInstId = 0;

    static AreaBuffTriggerMgr _singleton = null;
    static public AreaBuffTriggerMgr singleton
    {
        get
        {
            if (_singleton == null) _singleton = new AreaBuffTriggerMgr();
            return _singleton;
        }
    }

    public AreaBuffTriggerMgr()
    {

    }

    public int AddBuff(Role sendRole, Vector3 buffPos, int typeId)
    {
        AreaTriggerBuff areaBuff = NewBuff();

        areaBuff.Init(sendRole, buffPos, BuffCofig.singleton.GetAreaTriggerBuffConfig(typeId), ++mInstId, Time.fixedTime);
        areaBuff.OnEnter();
        mCurrBuffList.Add(areaBuff.mInstId, areaBuff);

        return mInstId;
    }

    public AreaTriggerBuff NewBuff()
    {
        AreaTriggerBuff buff = null;
        if (mRecoveryBuffList.Count > 0)
        {
            buff = mRecoveryBuffList.Pop();
        }
        else
        {
            buff = new AreaTriggerBuff();
        }
        return buff;
    }

    public void OnUpdate()
    {
        foreach (var id in mDeathBuffList)
        {
            AreaTriggerBuff buff;
            mCurrBuffList.TryGetValue(id, out buff);
            if (buff != null) mRecoveryBuffList.Push(buff);
            mCurrBuffList.Remove(id);
        }
        mDeathBuffList.Clear();

        foreach (var item in mCurrBuffList)
        {
            item.Value.OnUpdate();
        }
    }

    public void RemoveBuff(int insId)
    {
        mDeathBuffList.Add(insId);
    }
}

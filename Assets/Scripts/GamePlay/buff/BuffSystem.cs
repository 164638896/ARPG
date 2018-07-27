using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuffSystem
{
    public Role mRole = null;
    private Dictionary<int/*IBuff.BuffType*/, Dictionary<int/*InstId*/, IBuff>> mCurrBuffList = new Dictionary<int/*IBuff.BuffType*/, Dictionary<int/*InsId*/, IBuff>>();
    private List<BuffId> mDeathBuffList = new List<BuffId>();

    static private Dictionary<int/*IBuff.BuffType*/, Stack<IBuff>> mRecoveryBuffList = new Dictionary<int, Stack<IBuff>>();

    private static int mInstId = 0;

    public BuffSystem(Role role)
    {
        mRole = role;

        mCurrBuffList[(int)IBuff.BuffType.Behavior] = new Dictionary<int/*InstId*/, IBuff>();
        mCurrBuffList[(int)IBuff.BuffType.Move] = new Dictionary<int/*InstId*/, IBuff>();
        mCurrBuffList[(int)IBuff.BuffType.Hurt] = new Dictionary<int/*InstId*/, IBuff>();
        mCurrBuffList[(int)IBuff.BuffType.Control] = new Dictionary<int/*InstId*/, IBuff>();

        mRecoveryBuffList[(int)IBuff.BuffType.Behavior] = new Stack<IBuff>();
        mRecoveryBuffList[(int)IBuff.BuffType.Move] = new Stack<IBuff>();
        mRecoveryBuffList[(int)IBuff.BuffType.Hurt] = new Stack<IBuff>();
        mRecoveryBuffList[(int)IBuff.BuffType.Control] = new Stack<IBuff>();
    }

    public int AddBuff(Role sendRole, IBuff.BuffType type, int typeId)
    {
        IBuff buff = null;

        if (type == IBuff.BuffType.Behavior)
        {
            if(EnableBehaviorBuff())
            {
                buff = GetSameBuff(type, typeId);
                if (buff == null)
                {
                    buff = NewBuff(type);
                }
                else
                {
                    Debug.Log("");
                }
                buff.Init(sendRole, mRole, BuffCofig.singleton.GetBehaviorBuffConfig(typeId), Time.fixedTime);
            }
        }
        else if (type == IBuff.BuffType.Move)
        {
            if (EnableMoveBuff())
            {
                buff = GetSameBuff(type, typeId);
                if(buff == null)
                {
                    buff = NewBuff(type);
                }
                else
                {
                    Debug.Log("");
                }
                
                buff.Init(sendRole, mRole, BuffCofig.singleton.GetMoveBuffConfig(typeId), Time.fixedTime);
            }
        }
        else if(type == IBuff.BuffType.Hurt)
        {
            if (EnableHurtBuff())
            {
                buff = GetSameBuff(type, typeId);
                if (buff == null)
                {
                    buff = NewBuff(type);
                }
                else
                {
                    Debug.Log("");
                }
                buff.Init(sendRole, mRole, BuffCofig.singleton.GetHurtBuffConfig(typeId), Time.fixedTime);
            }
        }
        else if(type == IBuff.BuffType.Control)
        {
            buff = GetSameBuff(type, typeId);
            if (buff == null)
            {
                buff = NewBuff(type);
            }
            else
            {
                Debug.Log("");
            }
            buff.Init(sendRole, mRole, BuffCofig.singleton.GetControlBuffConfig(typeId), Time.fixedTime);
        }

        if(buff != null)
        {
            buff.OnEnter();
            mCurrBuffList[(int)type][buff.mInstId] = buff;
            return mInstId;
        }
        else
        {
            return -1;
           // Debug.Log("AddBuff " + "type=" + type + " typeId=" + typeId);
        }
    }

    public IBuff NewBuff(IBuff.BuffType type)
    {
        IBuff buff = null;
        if (mRecoveryBuffList[(int)type].Count > 0)
        {
            buff = mRecoveryBuffList[(int)type].Pop();
        }
        else
        {
            if (type == IBuff.BuffType.Move)
            {
                buff = new MoveBuff();
            }
            else if(type == IBuff.BuffType.Behavior)
            {
                buff = new BehaviorBuff();
            }
            else if (type == IBuff.BuffType.Hurt)
            {
                buff = new HurtBuff();
            }
            else if (type == IBuff.BuffType.Control)
            {
                buff = new ControlBuff();
            }
        }

        buff.mInstId = ++mInstId;
        return buff;
    }

    public void OnFixedUpdate()
    {
        foreach (var id in mDeathBuffList)
        {
            IBuff buff;
            mCurrBuffList[(int)id.mType].TryGetValue(id.mTypeId, out buff);
            if(buff != null) mRecoveryBuffList[(int)id.mType].Push(buff);
            mCurrBuffList[(int)id.mType].Remove(id.mTypeId);
        }
        mDeathBuffList.Clear();

        foreach (var typeList in mCurrBuffList)
        {
            foreach (var item in typeList.Value)
            {
                item.Value.OnFixedUpdate();
            }
        }
    }

    public void ClearAllBuff() // 先全部删除buff.后面在支持只删除debuff
    {
        foreach (var typeList in mCurrBuffList)
        {
            foreach (var item in typeList.Value)
            {
                RemoveBuff(item.Value.mBuffConfig.mType, item.Value.mInstId);
            }
        }
    }

    public void RemoveBuff(IBuff.BuffType type, int insId)
    {
        BuffId id = new BuffId();
        id.mType = type;
        id.mTypeId = insId;
        mDeathBuffList.Add(id);
    }

    public bool CanMove()
    {
        bool move = true;
        foreach (var item in mCurrBuffList[(int)IBuff.BuffType.Behavior])
        {
            if (item.Value is BehaviorBuff)
            {
                BehaviorBuff bb = item.Value as BehaviorBuff;
                if (bb.mBehaviorBuffConfig.mCanMove == false)
                {
                    move = false;
                    break;
                }
            }
        }

        return move;
    }

    public bool CanAtk()
    {
        bool atk = true;
        foreach (var item in mCurrBuffList[(int)IBuff.BuffType.Behavior])
        {
            if (item.Value is BehaviorBuff)
            {
                BehaviorBuff bb = item.Value as BehaviorBuff;
                if (bb.mBehaviorBuffConfig.mCanAtk == false)
                {
                    atk = false;
                    break;
                }
            }
        }

        return atk;
    }

    public bool EnableBehaviorBuff()
    {
        bool enable = true;
        foreach (var item in mCurrBuffList[(int)IBuff.BuffType.Control])
        {
            if (item.Value is ControlBuff)
            {
                ControlBuff cb = item.Value as ControlBuff;
                if (cb.mControlBuffConfig.mEnableBehavior == false)
                {
                    enable = false;
                    break;
                }
            }
        }

        return enable;
    }

    public bool EnableMoveBuff()
    {
        bool enable = true;
        foreach (var item in mCurrBuffList[(int)IBuff.BuffType.Control])
        {
            if (item.Value is ControlBuff)
            {
                ControlBuff cb = item.Value as ControlBuff;
                if (cb.mControlBuffConfig.mEnableMove == false)
                {
                    enable = false;
                    break;
                }
            }
        }

        return enable;
    }

    public bool EnableHurtBuff()
    {
        bool enable = true;
        foreach (var item in mCurrBuffList[(int)IBuff.BuffType.Control])
        {
            if (item.Value is ControlBuff)
            {
                ControlBuff cb = item.Value as ControlBuff;
                if (cb.mControlBuffConfig.mEnableHurt == false)
                {
                    enable = false;
                    break;
                }
            }
        }

        return enable;
    }

    public bool EnableSelect()
    {
        bool enable = true;
        foreach (var item in mCurrBuffList[(int)IBuff.BuffType.Control])
        {
            if (item.Value is ControlBuff)
            {
                ControlBuff cb = item.Value as ControlBuff;
                if (cb.mControlBuffConfig.mEnableSelect == false)
                {
                    enable = false;
                    break;
                }
            }
        }

        return enable;
    }

    public IBuff GetSameBuff(IBuff.BuffType type, int typeId)
    {
        foreach (var item in mCurrBuffList[(int)type])
        {
            if (item.Value.mBuffConfig.mTypeId == typeId)
            {
                return item.Value;
            }
        }

        return null;
    }

    public bool HasSameBuff(IBuff.BuffType type, int typeId)
    {
        foreach (var item in mCurrBuffList[(int)type])
        {
            if (item.Value.mBuffConfig.mTypeId == typeId)
            {
                return true;
            }
        }

        return false;
    }
}

using System.Collections.Generic;
using UnityEngine;

public abstract class IState
{
    public enum StateType
    {
        None,
        MultlAtk,
        Atk_TL,
        Buff,
        Grounded,
        Airborne,
        MoveToPos,
        TrailingObj,
        Death,

        // 暂时不用的状态
        Atk,
        MultlAtk_TL,
        MultlAtkOut_TL,
    }

    protected Animator mAnimator = null;

    protected StateMachine mStateMachine = null;
    protected Role mRole;

    public bool mChangeState = true;
    public StateType mStateType = StateType.None;
    protected List<int> mStateHashList = new List<int>();

    public IState(Role r, Animator animator, StateMachine sMachine, StateType stateType)
    {
        mRole = r;
        mAnimator = animator;
        mStateMachine = sMachine;
        mStateType = stateType;
    }

    public StateType GetStateType() { return mStateType; }

    public void AddStateHash(params int[] stateHash)
    {
        for (int i = 0; i < stateHash.Length; ++i)
        {
            mStateHashList.Add(stateHash[i]);
        }
    }

    public bool HasStateHash(int stateHash)
    {
        return mStateHashList.Contains(stateHash);
    }

    public virtual void OnAniEnter(int stateHash)
    {
        mStateMachine.SetCurrState(mStateType);
    }

    public virtual void OnLeave(int stateHash)
    {

    }

    public virtual void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
    {

    }

    public virtual bool ExecuteStateAgain(StateMachine stateMachine, object param1, object param2)
    {
        return false;
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void OnFixedUpdate() { }

    public virtual void OnLateUpdate() { }

    public virtual void OnHitEnter(Collider other)
    {
        if (mRole.mRoleData.mHp <= 0) return;

        ColliderParam colliderParam = other.gameObject.GetComponent<ColliderParam>();
        if (colliderParam == null) return;

        SkillConfig.SkillInfo skillInfo = SkillConfig.singleton.GetSkillInfo(colliderParam.mSkillId);
        if (skillInfo == null) return;

        GameObject sendRoleGO;
        Role sendRole;
        if (colliderParam)
        {
            if (colliderParam.HasCollider(mRole))
                return;

            colliderParam.AddCollider(mRole);

            sendRole = colliderParam.GetUserRole();
            sendRoleGO = sendRole.gameObject;

            if (sendRole.mRoleData.mGroupType == mRole.mRoleData.mGroupType) return;
        }
        else
        {
            return;
        }

        foreach (var item in skillInfo.mBuffIdList)
        {
            if (item.mTrigger == IBuff.BuffStage.Hit)
            {
                if (item.mBuffAttach == IBuff.BuffAttach.Self)
                {
                    sendRole.mBuffSystem.AddBuff(sendRole, item.mType, item.mTypeId);
                }
                else if (item.mBuffAttach == IBuff.BuffAttach.Target)
                {
                    if (skillInfo.mTargetType == SkillConfig.TargetType.None && !mRole.mBuffSystem.EnableSelect())
                    {
                        continue;
                    }
                    mRole.mBuffSystem.AddBuff(sendRole, item.mType, item.mTypeId);
                }
                else if (item.mBuffAttach == IBuff.BuffAttach.Area)
                {
                    AreaBuffTriggerMgr.singleton.AddBuff(sendRole, sendRole.transform.position, item.mTypeId);
                }
            }
        }
    }

    public bool PlayAnimation(string aniName, float crossTime)
    {
        int hashName = Animator.StringToHash(aniName);
        if (mAnimator.HasState(0, hashName))
        {
            mStateHashList.Clear();
            mStateHashList.Add(hashName);

            if (crossTime > 0.0f)
            {
                mAnimator.CrossFade(aniName, crossTime);
            }
            else
            {
                mAnimator.Play(aniName, 0, 0);
            }

            return true;
        }

        return false;
    }

    public void StateComplete()
    {
        mStateMachine.nextState();
    }
}

public class StateByTime : IState
{
    protected float mBeginTime = 0.0f;
    protected float mDuringTime = 0.0f;

    public StateByTime(Role role, Animator animator, StateMachine stateMachine, IState.StateType type)
    : base(role, animator, stateMachine, type)
    {

    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (mBeginTime + mDuringTime < Time.fixedTime)
        {
            StateComplete();
        }
    }
}
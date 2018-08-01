using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public class RoleAttackState : IState
//{
//    protected Role mTargetRole = null;
//    protected int mCurrSkillID = 0;
//    protected SkillConfig.SkillInfo mSkillInfo;

//    public RoleAttackState(Role role, Animator animator, StateMachine stateMachine, StateType stateType = IState.StateType.Atk)
//        : base(role, animator, stateMachine, stateType)
//    {

//    }

//    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
//    {
//        base.ExecuteState(stateMachine, prevState, param1, param2);

//        mCurrSkillID = (int)param1;
//        mSkillInfo = SkillConfig.singleton.GetSkillInfo(mCurrSkillID);
//        if (mSkillInfo == null) return;
//        mTargetRole = mRole.mRoleData.GetTargetRole();

//        Skill.ExecuteSkill(mRole, mTargetRole, mSkillInfo);
//        PlayAnimation(mSkillInfo.mStateName, 0.0f);
//        mChangeState = false;
//    }

//    public override void OnLeave(int stateHash)
//    {
//        mChangeState = true;

//        if (mSkillInfo.mSelfEffect)
//        {
//            mSkillInfo.mSelfEffect.SetActive(false);
//        }
//        base.OnLeave(stateHash);
//    }
//}

public class RoleMultAtkState : IState
{
    protected Role mTargetRole = null;
    protected int mCurrSkillID = 0;
    protected SkillConfig.SkillInfo mSkillInfo;

    private static string mParameter = "MulAtk";

    private int mMulAtk = 0;

    public RoleMultAtkState(Role role, Animator animator, StateMachine stateMachine)
    : base(role, animator, stateMachine, IState.StateType.MultlAtk)
    {
        AddStateHash(Animator.StringToHash("MulAtk01"), Animator.StringToHash("MulAtk02"), Animator.StringToHash("MulAtk03"),
            Animator.StringToHash("MulAtk01_Out"), Animator.StringToHash("MulAtk02_Out"), Animator.StringToHash("MulAtk03_Out"));
    }

    public override void OnAniEnter(int stateHash)
    {
        mCurrSkillID = -1;
        for (int i = 0; i < mRole.mRoleData.mMulSkillList.Count; ++i)
        {
            if (stateHash == mStateHashList[i])
            {
                mCurrSkillID = mRole.mRoleData.mMulSkillList[i];
                break;
            }
        }

        if (mCurrSkillID < 0) return;

        base.OnAniEnter(stateHash);
        mSkillInfo = SkillConfig.singleton.GetSkillInfo(mCurrSkillID);
        if (mSkillInfo == null) return;

        if (mSkillInfo.mSelfEffect != null)
        {
            mSkillInfo.mSelfEffect.SetActive(false);
            mSkillInfo.mSelfEffect.SetActive(true);
        }

        if (mSkillInfo.mTargetEffect != null)
        {
            mSkillInfo.mTargetEffect.SetActive(false);
            mSkillInfo.mTargetEffect.SetActive(true);
        }

        Skill.ExecuteSkill(mRole, mTargetRole, mSkillInfo);
        mChangeState = false;
    }

    public override void OnLeave(int stateHash)
    {
        if (mMulAtk > 0 && mMulAtk <= mStateHashList.Count)
        {
            // 连击完成
            if (!mRole.mBuffSystem.CanAtk() || stateHash == mStateHashList[mMulAtk + 2])
            {
                mMulAtk = 0;
                mAnimator.SetInteger(mParameter, mMulAtk);
                mChangeState = true;

                for (int i = 0; i < mRole.mRoleData.mMulSkillList.Count; ++i)
                {
                    mSkillInfo = SkillConfig.singleton.GetSkillInfo(mRole.mRoleData.mMulSkillList[i]);
                    if (mSkillInfo != null)
                    {
                        mSkillInfo.mSelfEffect.SetActive(false);
                    }
                }
            }
        }
    }

    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
    {
        base.ExecuteState(stateMachine, prevState, param1, param2);

        mTargetRole = mRole.mRoleData.GetTargetRole();

        mMulAtk = 1;
        mAnimator.SetInteger(mParameter, mMulAtk);
    }

    public override bool ExecuteStateAgain(StateMachine stateMachine, object param1, object param2)
    {
        if (mMulAtk < mRole.mRoleData.mMulSkillList.Count)
        {
            mAnimator.SetInteger(mParameter, ++mMulAtk);
        }

        return true;
    }

    public override void OnUpdate()
    {

    }
}

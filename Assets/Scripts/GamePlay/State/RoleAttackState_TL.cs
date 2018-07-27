using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

class Skill
{
    static public bool ExecuteSkill(Role role, Role targetRole, SkillConfig.SkillInfo skillInfo)
    {
        if(targetRole && targetRole.mRoleData.mHp <= 0)
        {
            return false;
        }

        if (targetRole && skillInfo.mTargetType != SkillConfig.TargetType.None)
        {
            if (Vector3.Distance(role.transform.position, targetRole.transform.position) > skillInfo.mAtkDistance)
            {
                return false;
            }

            role.mRoleData.SetForward(targetRole.transform.position, role.transform.position);
            role.transform.rotation = Quaternion.LookRotation(role.mRoleData.GetForward());
        }
        else
        {
            role.transform.rotation = Quaternion.LookRotation(role.mRoleData.GetForward());
        }

        foreach (var item in skillInfo.mBuffIdList)
        {
            if (item.mTrigger == IBuff.BuffStage.Start)
            {
                if (item.mBuffAttach == IBuff.BuffAttach.Self)
                {
                    role.mBuffSystem.AddBuff(role, item.mType, item.mTypeId);
                }
                else if (item.mBuffAttach == IBuff.BuffAttach.Target)
                {
                    if(targetRole)
                    {
                        if (skillInfo.mTargetType == SkillConfig.TargetType.None && !targetRole.mBuffSystem.EnableSelect())
                        {
                            continue;
                        }
                        targetRole.mBuffSystem.AddBuff(role, item.mType, item.mTypeId);
                    }
                }
                else if (item.mBuffAttach == IBuff.BuffAttach.Area)
                {
                    AreaBuffTriggerMgr.singleton.AddBuff(role, role.transform.position, item.mTypeId);
                }
            }
        }

        if (skillInfo.mSelfEffect)
        {
            skillInfo.mSelfEffect.transform.position = role.transform.position;
            skillInfo.mSelfEffect.transform.rotation = Quaternion.LookRotation(role.transform.forward);
            EffectColliderList effect = skillInfo.mSelfEffect.GetComponent<EffectColliderList>();
            if (effect)
            {
                effect.SetColliders(role, skillInfo.mSkillTypeId);
            }
            skillInfo.mSelfEffect.SetActive(true);
        }

        if (skillInfo.mTargetEffect)
        {
            skillInfo.mTargetEffect.transform.position = targetRole.transform.position;
            skillInfo.mTargetEffect.transform.rotation = Quaternion.LookRotation(role.transform.forward);
            EffectColliderList effect = skillInfo.mTargetEffect.GetComponent<EffectColliderList>();
            if (effect)
            {
                effect.SetColliders(role, skillInfo.mSkillTypeId);
            }
        }

        return true;
    }
}

public class RoleAttackState_TL : StateByTime
{
    protected Role mTargetRole = null;
    protected PlayableDirector mDirector = null;
    protected int mCurrSkillID = 0;
    protected SkillConfig.SkillInfo mSkillInfo;

    public RoleAttackState_TL(Role role, Animator animator, StateMachine stateMachine, StateType stateType = IState.StateType.Atk_TL)
        : base(role, animator, stateMachine, stateType)
    {

    }

    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
    {
        base.ExecuteState(stateMachine, prevState, param1, param2);

        mCurrSkillID = (int)param1;
        mChangeState = false;

        mSkillInfo = SkillConfig.singleton.GetSkillInfo(mCurrSkillID);
        if (mSkillInfo == null || mSkillInfo.mTimeline == null) return;
        mTargetRole = mRole.mRoleData.GetTargetRole();

        if (Skill.ExecuteSkill(mRole, mTargetRole, mSkillInfo))
        {
            mChangeState = false;
        }
        else
        {
            StateComplete();
            return;
        }

        mDirector = mSkillInfo.mTimeline.GetComponent<PlayableDirector>();
        mDirector.Stop();

        foreach (PlayableBinding bind in mDirector.playableAsset.outputs)
        {
            if (bind.streamName == "Self")
            {
                AnimationTrack animationTrack = bind.sourceObject as AnimationTrack;
                mDuringTime = (float)animationTrack.duration;
                mBeginTime = Time.fixedTime;
                mDirector.SetGenericBinding(bind.sourceObject, mAnimator);
            }
            else if (bind.streamName == "Target")
            {
                mDirector.SetGenericBinding(bind.sourceObject, mTargetRole.mAnimator);

                mRole.transform.position = -mRole.transform.forward * mSkillInfo.mAtkDistance + mTargetRole.transform.position;

                mRole.mRoleData.SetForward(mTargetRole.transform.position, mRole.transform.position);
                mRole.transform.rotation = Quaternion.LookRotation(mRole.mRoleData.GetForward());

                mTargetRole.mRoleData.SetForward(mRole.transform.position, mTargetRole.transform.position);
                mTargetRole.transform.rotation = Quaternion.LookRotation(mTargetRole.mRoleData.GetForward());
            }
            else if (bind.streamName == "SelfEffect")
            {
                mDirector.SetGenericBinding(bind.sourceObject, mSkillInfo.mSelfEffect);
            }
            else if (bind.streamName == "TargetEffect")
            {
                mDirector.SetGenericBinding(bind.sourceObject, mSkillInfo.mTargetEffect);
            }
            else if (bind.streamName == "Control Track")
            {
                ControlTrack ct = bind.sourceObject as ControlTrack;
                foreach (var clip in ct.GetClips())
                {
                    ControlPlayableAsset playableAsset = clip.asset as ControlPlayableAsset;
                    mDirector.SetReferenceValue(playableAsset.sourceGameObject.exposedName, mRole.gameObject);
                }
            }
            else if (bind.streamName == "Playable Track")
            {
                PlayableTrack ct = bind.sourceObject as PlayableTrack;
                foreach (TimelineClip clip in ct.GetClips())
                {
                    PlayableAssetEx playableAsset = clip.asset as PlayableAssetEx;
                    if(playableAsset)
                    {
                        playableAsset.mTL = this;
                    }
                }
            }
        }

        mDirector.Play();
    }

    public override void OnLeave(int stateHash)
    {
        mChangeState = true;
        base.OnLeave(stateHash);
    }

    public void OnBehaviourPlay()
    {
        foreach (var item in mSkillInfo.mBuffIdList)
        {
            if (item.mTrigger == IBuff.BuffStage.Hit)
            {
                if (item.mBuffAttach == IBuff.BuffAttach.Self)
                {
                    mRole.mBuffSystem.AddBuff(mRole, item.mType, item.mTypeId);
                }
                else if (item.mBuffAttach == IBuff.BuffAttach.Target)
                {
                    if(mTargetRole)
                    {
                        if (mSkillInfo.mTargetType == SkillConfig.TargetType.None && !mTargetRole.mBuffSystem.EnableSelect())
                        {
                            continue;
                        }

                        mTargetRole.mBuffSystem.AddBuff(mRole, item.mType, item.mTypeId);
                    }
                }
                else if (item.mBuffAttach == IBuff.BuffAttach.Area)
                {
                    AreaBuffTriggerMgr.singleton.AddBuff(mRole, mRole.transform.position, item.mTypeId);
                }
            }
        }
    }
}


public class RoleMultAtkState_TL : RoleAttackState_TL
{
    private static string[] mOutAniName = { "MulAtk01_Out", "MulAtk02_Out", "MulAtk03_Out" };

    public RoleMultAtkState_TL(Role role, Animator animator, StateMachine stateMachine)
    : base(role, animator, stateMachine, IState.StateType.MultlAtk_TL)
    {

    }

    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
    {
        base.ExecuteState(stateMachine, prevState, param1, param2);

        int index = GetIndex(mCurrSkillID);
        mStateMachine.SetNextState(IState.StateType.MultlAtkOut_TL, mOutAniName[index], null, 1);
    }

    public override bool ExecuteStateAgain(StateMachine stateMachine, object param1, object param2)
    {
        int nextIndex = GetIndex(mCurrSkillID) + 1;
        if (nextIndex > 0 && nextIndex < mRole.mRoleData.mMulSkillList.Count)
        {
            mStateMachine.SetNextState(IState.StateType.MultlAtk_TL, mRole.mRoleData.mMulSkillList[nextIndex], null, 1);
        }

        return true;
    }

    private int GetIndex(int skillId)
    {
        int index = 0;
        for (; index < mRole.mRoleData.mMulSkillList.Count; ++index)
        {
            if (mRole.mRoleData.mMulSkillList[index] == skillId)
            {
                break;
            }
        }
        return index;
    }
}


public class RoleMultAtkOutState_TL : IState
{
    public RoleMultAtkOutState_TL(Role role, Animator animator, StateMachine stateMachine)
        : base(role, animator, stateMachine, IState.StateType.MultlAtkOut_TL)
    {

    }

    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
    {
        base.ExecuteState(stateMachine, prevState, param1, param2);

        PlayAnimation((string)param1, 0);
        mChangeState = false;
    }

    public override void OnLeave(int stateHash)
    {
        mChangeState = true;
        base.OnLeave(stateHash);
    }
}
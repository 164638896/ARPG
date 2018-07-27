using UnityEngine;
using System.Collections;
using BT;

public class Monster : Role
{
    public SkillConfig.SkillInfo mSkillInfo = null;
    MonsterAI mAI;
    public override void aWake()
    {
        base.aWake();
    }

    public override void start()
    {
        base.start();

        //mCurrSkillId 应该服务器传过来
        int index = Random.Range(0, mRoleData.mSkillList.Count);
        mRoleData.mCurrSkillId = mRoleData.mSkillList[index];
        mSkillInfo = SkillConfig.singleton.GetSkillInfo(mRoleData.mCurrSkillId);
        if (mSkillInfo == null)
        {
            Debug.Log("skill is null skillId = " + mRoleData.mCurrSkillId);
        }

        mAI = new MonsterAI(this);
        InvokeRepeating("AI", 1.0f, 1.0f);
    }

    public override void update()
    {
        base.update();
    }

    public void AI()
    {
        mAI.Update();
    }

    //public void AI()
    //{
    //    if (mRoleData.mHp <= 0)
    //    {
    //        CancelInvoke("AI");
    //        return;
    //    }

    //    if (mStateMachine.GetCurStateType() == IState.StateType.Grounded)
    //    {
    //        if (mRoleData.GetTargetRole() != null)
    //        {
    //            if (!mRoleData.GetTargetRole().mBuffSystem.EnableSelect())
    //            {
    //                return;
    //            }

    //            float dis = Vector3.Distance(mRoleData.GetTargetRole().transform.position, transform.position);

    //            if (dis > mSkillInfo.mAtkDistance && mBuffSystem.CanMove())
    //            {
    //                Vector3 rolePos = new Vector3(transform.position.x, 0, transform.position.z);
    //                Vector3 targetPos = new Vector3(mRoleData.GetTargetRole().transform.position.x, 0, mRoleData.GetTargetRole().transform.position.z);
    //                Vector3 targetDir = (targetPos - rolePos).normalized;
    //                Vector3 dir = Quaternion.AngleAxis(Random.Range(-40, 40), Vector3.up) * targetDir;

    //                float offset = 1.0f;
    //                if (offset > mSkillInfo.mAtkDistance) offset = mSkillInfo.mAtkDistance;
    //                Vector3 movePos = mRoleData.GetTargetRole().transform.position - dir * offset;

    //                mStateMachine.SwitchState(IState.StateType.MoveToPos, 1.0f, movePos);
    //            }

    //            if (dis <= mSkillInfo.mAtkDistance && mBuffSystem.CanAtk())
    //            {
    //                mRoleData.SetForward(mRoleData.GetTargetRole().transform.position, transform.position);
    //                mStateMachine.SwitchState(IState.StateType.Atk_TL, mRoleData.mCurrSkillId, null);

    //                int index = Random.Range(0, mRoleData.mSkillList.Count);
    //                mRoleData.mCurrSkillId = mRoleData.mSkillList[index];
    //                mSkillInfo = SkillConfig.singleton.GetSkillInfo(mRoleData.mCurrSkillId);
    //                if (mSkillInfo == null)
    //                {
    //                    Debug.Log("skill is null skillId = " + mRoleData.mCurrSkillId);
    //                }
    //            }
    //        }
    //    }
    //}
}

public class ReviveAction : BTAction
{
    Monster mMonster;

    public ReviveAction(Monster monster)
    {
        mMonster = monster;
    }

    protected override void Enter()
    {
        base.Enter();
        mMonster.mRoleData.mHp = 100;
        mMonster.mBuffSystem.ClearAllBuff();
        mMonster.mAnimator.SetBool("Death", false);
        mMonster.mStateMachine.SwitchState(IState.StateType.Grounded, null, null);
    }

    protected override BTResult Execute()
    {
        return BTResult.Success;
    }
}

public class IdleAction : BTAction
{
    Monster mMonster;

    public IdleAction(Monster monster)
    {
        mMonster = monster;
    }

    protected override void Enter()
    {
        base.Enter();
        mMonster.mStateMachine.SwitchState(IState.StateType.Grounded, null, null);
    }

    protected override BTResult Execute()
    {
        return BTResult.Success;
    }
}

public class PatrolAction : BTAction
{
    Monster mMonster;

    public PatrolAction(Monster monster)
    {
        mMonster = monster;
    }

    protected override void Enter()
    {
        base.Enter();
        Vector3 newPos = mMonster.transform.position + new Vector3(Random.Range(-2.0f, 2.0f), 0, Random.Range(-2.0f, 2.0f));
        mMonster.mStateMachine.SwitchState(IState.StateType.MoveToPos, 1.0f, newPos);
    }

    protected override BTResult Execute()
    {
        return BTResult.Success;
    }
}

public class FollowAction : BTAction
{
    Monster mMonster;

    public FollowAction(Monster monster)
    {
        mMonster = monster;
    }

    protected override void Enter()
    {
        base.Enter();

        Role mTargetRole = mMonster.mRoleData.GetTargetRole();
        if (mTargetRole)
        {
            Vector3 rolePos = new Vector3(mMonster.transform.position.x, 0, mMonster.transform.position.z);
            Vector3 targetPos = new Vector3(mTargetRole.transform.position.x, 0, mTargetRole.transform.position.z);
            Vector3 targetDir = (targetPos - rolePos).normalized;
            Vector3 dir = Quaternion.AngleAxis(Random.Range(-40, 40), Vector3.up) * targetDir;

            float offset = 1.0f;
            if (offset > mMonster.mSkillInfo.mAtkDistance) offset = mMonster.mSkillInfo.mAtkDistance;
            Vector3 movePos = mTargetRole.transform.position - dir * offset;

            mMonster.mStateMachine.SwitchState(IState.StateType.TrailingObj, 1.0f, mMonster.mSkillInfo.mAtkDistance);
        }
    }

    protected override BTResult Execute()
    {
        return BTResult.Success;
    }
}

public class AttackAction : BTAction
{
    Monster mMonster;

    public AttackAction(Monster monster)
    {
        mMonster = monster;
    }
    protected override void Enter()
    {
        base.Enter();

        Role mTargetRole = mMonster.mRoleData.GetTargetRole();
        if (mTargetRole)
        {
            mMonster.mRoleData.SetForward(mTargetRole.transform.position, mMonster.transform.position);
            mMonster.mStateMachine.SwitchState(IState.StateType.Atk_TL, mMonster.mRoleData.mCurrSkillId, null);

            // 更换技能
            int index = Random.Range(0, mMonster.mRoleData.mSkillList.Count);
            mMonster.mRoleData.mCurrSkillId = mMonster.mRoleData.mSkillList[index];
            mMonster.mSkillInfo = SkillConfig.singleton.GetSkillInfo(mMonster.mRoleData.mCurrSkillId);
            if (mMonster.mSkillInfo == null)
            {
                Debug.Log("skill is null skillId = " + mMonster.mRoleData.mCurrSkillId);
            }
        }
    }

    protected override BTResult Execute()
    {
        return BTResult.Success;
    }
}

public class RunAwayAction : BTAction
{
    Monster mMonster;

    public RunAwayAction(Monster monster)
    {
        mMonster = monster;
    }

    protected override void Enter()
    {
        base.Enter();

        Role mTargetRole = mMonster.mRoleData.GetTargetRole();
        if (mTargetRole)
        {
            Vector3 rolePos = new Vector3(mMonster.transform.position.x, 0, mMonster.transform.position.z);
            Vector3 targetPos = new Vector3(mTargetRole.transform.position.x, 0, mTargetRole.transform.position.z);
            Vector3 targetDir = (rolePos - targetPos).normalized;

            Vector3 movePos = mMonster.transform.position + targetDir * 1.0f;

            mMonster.mStateMachine.SwitchState(IState.StateType.TrailingObj, 1.0f, mMonster.mSkillInfo.mAtkDistance);
        }
    }

    protected override BTResult Execute()
    {
        return BTResult.Success;
    }
}

public class MonsterAI
{
    private BTSelector mRoot;
    Monster mMonster;

    public MonsterAI(Monster monster)
    {
        mMonster = monster;
        Init();
    }

    public void Init()
    {
        // 条件
        BaseCondiction.ExternalFunc IsDeadFun = () => { if (mMonster.mRoleData.mHp <= 0) return true; else return false; };
        var Alive = new PreconditionNOT(IsDeadFun, "活");
        var Dead = new Precondition(IsDeadFun, "死");

        BaseCondiction.ExternalFunc targetFun = () =>
        {
            Role targetRole = mMonster.mRoleData.GetTargetRole();
            if (targetRole)
            {
                return targetRole.mBuffSystem.EnableSelect();
            }
            else
                return false;
        };
        var hasTarget = new Precondition(targetFun, "发现目标");
        var hasNoTarget = new PreconditionNOT(targetFun, "无目标");

        BaseCondiction.ExternalFunc AtkRangeFun = () =>
        {
            Role targetRole = mMonster.mRoleData.GetTargetRole();
            if (targetRole)
            {
                float dis = Vector3.Distance(targetRole.transform.position, mMonster.transform.position);
                if (dis <= mMonster.mSkillInfo.mAtkDistance) return true;
                else
                {
                    if (dis > 5)//脱离目标
                    {
                        mMonster.mRoleData.SetTargetRole(-1);
                    }
                }
            }

            return false;
        };
        var AtkRange = new Precondition(AtkRangeFun, "在攻击范围内");
        var NoAtkRange = new PreconditionNOT(AtkRangeFun, "超出攻击范围");

        //BaseCondiction.ExternalFunc HpFun = () => { if (mMonster.mRoleData.mHp <= 50) return true; else return false; };
        //var HPLess = new Precondition(HpFun, "快死");
        //var HPMore = new PreconditionNOT(HpFun, "健康");

        BaseCondiction.ExternalFunc CanMoveFun = () => { return mMonster.mBuffSystem.CanMove(); };
        var canMove = new Precondition(CanMoveFun, "能移动");
        var cantMove = new PreconditionNOT(CanMoveFun, "不能移动");

        BaseCondiction.ExternalFunc CanAtkFun = () => { return mMonster.mBuffSystem.CanAtk(); };
        var canAtk = new Precondition(CanAtkFun, "能攻击");
        var cantAtk = new PreconditionNOT(CanAtkFun, "不能攻击");

        //BaseCondiction.ExternalFunc EnableSelectFun = () =>
        //{
        //    Role targetRole = mMonster.mRoleData.GetTargetRole();
        //    if (targetRole)
        //    {
        //        return targetRole.mBuffSystem.EnableSelect();
        //    }

        //    return false;
        //};
        //var enableSelect = new Precondition(EnableSelectFun, "能选");
        ////var disableSelect = new PreconditionNOT(EnableSelectFun, "不能选");

        //BT Tree
        mRoot = new BTSelector();
        mRoot.name = "Root";
        mRoot.Activate();

        BTSequence DeadSeq = new BTSequence();
        {
            DeadSeq.AddChild(Dead);
            // 死亡Action
            DeadSeq.AddChild(new BTActionWait(5));
            DeadSeq.AddChild(new ReviveAction(mMonster));
            mRoot.AddChild(DeadSeq);
        }

        BTSelector AliveSel = new BTSelector();
        BTSequence AliveSeq = new BTSequence();
        {
            AliveSeq.AddChild(Alive);
            AliveSeq.AddChild(AliveSel);
            mRoot.AddChild(AliveSeq);
        }

        BTSequence followSubtree = new BTSequence();
        {
            followSubtree.AddChild(canMove);
            followSubtree.AddChild(hasTarget);
            followSubtree.AddChild(NoAtkRange);
            //followSubtree.AddChild(HPMore);

            // 追击Action
            followSubtree.AddChild(new FollowAction(mMonster));

            AliveSel.AddChild(followSubtree);
        }

        BTSequence atkSeq = new BTSequence();
        {
            atkSeq.AddChild(canAtk);
            atkSeq.AddChild(hasTarget);
            atkSeq.AddChild(AtkRange);
            //atkSeq.AddChild(HPMore);

            // 攻击Action
            atkSeq.AddChild(new AttackAction(mMonster));
            atkSeq.AddChild(new BTActionWaitRandom(2.0f, 3.0f));

            AliveSel.AddChild(atkSeq);
        }

        BTSequence patrolSeq = new BTSequence();
        {
            patrolSeq.AddChild(canMove);
            patrolSeq.AddChild(hasNoTarget);
            patrolSeq.AddChild(new BTActionWaitRandom(1.0f, 5.0f));

            // 巡逻Action
            patrolSeq.AddChild(new PatrolAction(mMonster));

            AliveSel.AddChild(patrolSeq);
        }

        BTSequence IdleSeq = new BTSequence();
        {
            IdleSeq.AddChild(hasNoTarget);

            // 休息Action
            IdleSeq.AddChild(new IdleAction(mMonster));

            AliveSel.AddChild(IdleSeq);
        }

        //BTSequence runAwaySeq = new BTSequence();
        //{
        //    runAwaySeq.AddChild(canMove);
        //    runAwaySeq.AddChild(hasTarget);
        //    runAwaySeq.AddChild(HPLess);
        //    // 逃跑Action
        //    runAwaySeq.AddChild(new RunAwayAction(mMonster));

        //    AliveSel.AddChild(runAwaySeq);
        //}
    }

    public void Update()
    {
        mRoot.Tick();
    }
}
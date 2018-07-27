using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class RoleGroundedState : IState
{
    public RoleGroundedState(Role role, Animator animator, StateMachine stateMachine)
    : base(role, animator, stateMachine, IState.StateType.Grounded)
    {
        //AddStateHash(Animator.StringToHash("Grounded"));
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        float speed = mAnimator.GetFloat("Speed");
        if (speed > 0.0f && mRole.mBuffSystem.CanMove())
        {
            Vector3 dir = mRole.mRoleData.GetForward();
            mRole.transform.Translate(dir * Time.deltaTime * mRole.mRoleData.mMoveSpeed, Space.World);
            mRole.transform.LookAt(mRole.transform.position + dir);
        }
    }
}

public class RoleMoveToPosState : IState
{
    private Vector3 TargetPos;

    public RoleMoveToPosState(Role role, Animator animator, StateMachine stateMachine)
    : base(role, animator, stateMachine, IState.StateType.MoveToPos)
    {

    }

    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
    {
        base.ExecuteState(stateMachine, prevState, param1, param2);

        if (param1 is float)
        {
            mAnimator.SetFloat("Speed", (float)param1);
        }
        if (param2 is Vector3)
        {
            TargetPos = (Vector3)param2;
        }
    }

    public override bool ExecuteStateAgain(StateMachine stateMachine, object param1, object param2)
    {
        if (param1 is float)
        {
            mAnimator.SetFloat("Speed", (float)param1);
        }
        if (param2 is Vector3)
        {
            TargetPos = (Vector3)param2;
        }

        return true;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        float speed = mAnimator.GetFloat("Speed");
        if (speed <= 0.0f)
        {
            StateComplete();
            return;
        }

        if (!mRole.mBuffSystem.CanMove())
        {
            StateComplete();
            return;
        }

        mRole.mRoleData.SetForward(TargetPos, mRole.transform.position);

        Vector3 dir = mRole.mRoleData.GetForward();
        mRole.transform.LookAt(mRole.transform.position + dir);
        mRole.transform.Translate(dir * Time.deltaTime * mRole.mRoleData.mMoveSpeed, Space.World);
        if (Vector3.Distance(mRole.transform.position, TargetPos) < 0.5f)
        {
            mRole.transform.position = TargetPos;
            StateComplete();
        }
    }

    public override void OnLeave(int stateHash)
    {
        mAnimator.SetFloat("Speed", 0.0f);
        base.OnLeave(stateHash);
    }
}


public class RoleTrailingObjState : IState
{
    private float mAtkDistance = 0;
    private Transform TargetTranform;

    public RoleTrailingObjState(Role role, Animator animator, StateMachine stateMachine)
    : base(role, animator, stateMachine, IState.StateType.TrailingObj)
    {

    }

    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
    {
        if (param1 is float)
        {
            mAnimator.SetFloat("Speed", (float)param1);
        }
        if (param2 is float)
        {
            mAtkDistance = (float)param2;
        }

        if(mRole.mRoleData.GetTargetRole())
        {
            TargetTranform = mRole.mRoleData.GetTargetRole().transform;
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        float speed = mAnimator.GetFloat("Speed");
        if (speed <= 0.0f || TargetTranform == null)
        {
            StateComplete();
            return;
        }

        if (!mRole.mBuffSystem.CanMove())
        {
            StateComplete();
            return;
        }

        mRole.mRoleData.SetForward(TargetTranform.position, mRole.transform.position);

        Vector3 dir = mRole.mRoleData.GetForward();
        mRole.transform.LookAt(mRole.transform.position + dir);
        mRole.transform.Translate(dir * Time.deltaTime * mRole.mRoleData.mMoveSpeed, Space.World);

        if (Vector3.Distance(mRole.transform.position, TargetTranform.position) < mAtkDistance)
        {
            StateComplete();
        }
    }

    public override void OnLeave(int stateHash)
    {
        mAnimator.SetFloat("Speed", 0.0f);
        TargetTranform = null;
        base.OnLeave(stateHash);
    }
}


//public class RoleMoveToPosState : IState
//{
//    private Vector3 mTargetPos;
//    private int mIndex;
//    private NavMeshPath mPath = new NavMeshPath();

//    public RoleMoveToPosState(Role role, Animator animator, StateMachine stateMachine)
//    : base(role, animator, stateMachine, IState.StateType.MoveToPos)
//    {

//    }

//    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
//    {
//        base.ExecuteState(stateMachine, prevState, param1, param2);
//        if (param1 is float)
//        {
//            mAnimator.SetFloat("Speed", (float)param1);
//        }
//        if (param2 is Vector3)
//        {
//            mTargetPos = (Vector3)param2;
//        }

//        if (mRole.mAgent)
//        {
//            mRole.mAgent.enabled = true;
//            mRole.mAgent.speed = mRole.mRoleData.mMoveSpeed;
//            mRole.mAgent.SetDestination(mTargetPos);
//            mRole.mAgent.CalculatePath(mTargetPos, mPath);
//            mIndex = 0;
//            if (mPath.corners.Length > 0)
//            {
//                mTargetPos = mPath.corners[mIndex++];
//            }
//        }

//        mRole.mRoleData.SetForward(mTargetPos, mRole.transform.position);
//        mRole.transform.LookAt(mRole.transform.position + mRole.mRoleData.GetForward());
//    }

//    public override void OnFixedUpdate()
//    {
//        base.OnFixedUpdate();

//        float speed = mAnimator.GetFloat("Speed");
//        if (speed <= 0.0f)
//        {
//            StateComplete();
//            return;
//        }

//        if (!mRole.mBuffSystem.CanMove())
//        {
//            StateComplete();
//            return;
//        }

//        mRole.mRoleData.SetForward(mTargetPos, mRole.transform.position);

//        Vector3 dir = mRole.mRoleData.GetForward();
//        mRole.transform.LookAt(mRole.transform.position + dir);
//        mRole.transform.Translate(dir * Time.deltaTime * mRole.mRoleData.mMoveSpeed, Space.World);
//        if (Vector3.Distance(mRole.transform.position, mTargetPos) < 0.5f)
//        {
//            mRole.transform.position = mTargetPos;

//            if(mIndex < mPath.corners.Length)
//            {
//                mTargetPos = mPath.corners[mIndex++];
//            }
//            else
//            {
//                StateComplete();
//            }
//        }
//    }

//    public override void OnLeave(int stateHash)
//    {
//        mAnimator.SetFloat("Speed", 0.0f);

//        base.OnLeave(stateHash);
//    }
//}


//public class RoleTrailingObjState : IState
//{
//    private Vector3 TargetPos;
//    private float mAtkDistance = 0;

//    public RoleTrailingObjState(Role role, Animator animator, StateMachine stateMachine)
//    : base(role, animator, stateMachine, IState.StateType.TrailingObj)
//    {

//    }

//    public override void ExecuteState(StateMachine stateMachine, IState prevState, object param1, object param2)
//    {
//        mRole.mAgent.enabled = true;

//        if (param1 is float)
//        {
//            mAnimator.SetFloat("Speed", (float)param1);
//        }
//        if (param2 is float)
//        {
//            mAtkDistance = (float)param2;
//        }

//        TargetPos = mRole.mRoleData.GetTargetRole().transform.position;

//        if (mRole.mAgent)
//        {
//            mRole.mAgent.speed = mRole.mRoleData.mMoveSpeed;
//            //mRole.mAgent.stoppingDistance = mAtkDistance;
//            mRole.mAgent.SetDestination(TargetPos);
//        }

//        mRole.mRoleData.SetForward(TargetPos, mRole.transform.position);
//        mRole.transform.LookAt(mRole.transform.position + mRole.mRoleData.GetForward());
//    }

//    public override void OnFixedUpdate()
//    {
//        base.OnFixedUpdate();

//        float speed = mAnimator.GetFloat("Speed");
//        if (speed <= 0.0f)
//        {
//            StateComplete();
//            return;
//        }

//        if (!mRole.mBuffSystem.CanMove())
//        {
//            StateComplete();
//            return;
//        }

//        if (mRole.mAgent.hasPath && mRole.mAgent.remainingDistance <= mAtkDistance)
//        {
//            StateComplete();
//        }
//        else
//        {
//            TargetPos = mRole.mRoleData.GetTargetRole().transform.position;

//            if (mRole.mAgent)
//            {
//                mRole.mAgent.speed = mRole.mRoleData.mMoveSpeed;
//                mRole.mAgent.stoppingDistance = mAtkDistance;
//                mRole.mAgent.SetDestination(TargetPos);
//            }

//            mRole.mRoleData.SetForward(TargetPos, mRole.transform.position);
//            mRole.transform.LookAt(mRole.transform.position + mRole.mRoleData.GetForward());
//        }
//    }

//    public override void OnLeave(int stateHash)
//    {
//        mAnimator.SetFloat("Speed", 0.0f);
//        mRole.mAgent.ResetPath();
//        base.OnLeave(stateHash);
//    }
//}
using UnityEngine;
using UnityEngine.AI;

public class MoveBuffConfig : BuffConfig
{
    public MoveBuff.MoveDir mDir;
    public float mDistance;
    public bool mCollision;
}

public class MoveBuff : IBuff
{
    public enum MoveDir
    {
        TargetForward = 1,  // 施放者朝向目标(怪物)的方向
        TargetBackward = 2, // 施放者朝向目标(怪物)的反方向
        SelfForward = 3,    // 施放者正方向
        SelfBackward = 4,   // 施放者反方向
        SelfForwardOffset = 5, // 施放者正方向偏移点的方向
    }

    public MoveBuffConfig mMoveBuffConfig;

    // s = xt - 0.5at^2
    float a = 10;
    float x;
    Vector3 mStartPos = Vector3.zero;
    Vector3 mDir = Vector3.zero;
    float mDistance = 0;

    public override void OnEnter()
    {
        mMoveBuffConfig = mBuffConfig as MoveBuffConfig;

        base.OnEnter();

        x = (mMoveBuffConfig.mDistance + 0.5f * a * mMoveBuffConfig.mDuration * mMoveBuffConfig.mDuration) / mMoveBuffConfig.mDuration;

        if (mMoveBuffConfig.mDir == MoveDir.TargetForward)
        {
            Vector3 receRolePos = new Vector3(mReceRole.transform.position.x, 0, mReceRole.transform.position.z);
            Vector3 sendRolePos = new Vector3(mSendRole.transform.position.x, 0, mSendRole.transform.position.z);
            mDir = (receRolePos - sendRolePos).normalized;
            mDistance = mMoveBuffConfig.mDistance;
        }
        else if (mMoveBuffConfig.mDir == MoveDir.TargetBackward)
        {
            Vector3 receRolePos = new Vector3(mReceRole.transform.position.x, 0, mReceRole.transform.position.z);
            Vector3 sendRolePos = new Vector3(mSendRole.transform.position.x, 0, mSendRole.transform.position.z);
            mDir = (sendRolePos - receRolePos).normalized;
            mDistance = mMoveBuffConfig.mDistance;
        }
        else if (mMoveBuffConfig.mDir == MoveDir.SelfForward)
        {
            mDir = mSendRole.transform.forward;
            mDistance = mMoveBuffConfig.mDistance;
        }
        else if (mMoveBuffConfig.mDir == MoveDir.SelfBackward)
        {
            mDir = -mSendRole.transform.forward;
            mDistance = mMoveBuffConfig.mDistance;
        }
        else if (mMoveBuffConfig.mDir == MoveDir.SelfForwardOffset)
        {
            Vector3 targetPos = mSendRole.transform.position + mSendRole.transform.forward * mMoveBuffConfig.mDistance;

            mDir = targetPos - mReceRole.transform.position;
            mDir.y = 0;
            mDir.Normalize();

            mDistance = Vector3.Distance(targetPos, mReceRole.transform.position);
        }

        mStartPos = mReceRole.transform.position;
    }

    public override void OnLeave()
    {
        base.OnLeave();
        mStartPos = Vector3.zero;
    }

    public override void OnFixedUpdate()
    {
        float t = Time.fixedTime - mBeginTime;
        if (t <= 0) return;

        float dis = x * t - 0.5f * a * t * t;

        if (t < mMoveBuffConfig.mDuration && dis < mDistance)
        {
            Vector3 offset = mDir * dis;
            //mReceRole.mAgent.Move(offset - mLastPos);
            mReceRole.transform.position = mStartPos + offset;
        }
        else
        {
            Vector3 offset = mDir * mDistance;
            //mReceRole.mAgent.Move(offset - mLastPos);
            mReceRole.transform.position = mStartPos + offset;
            OnLeave();
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour
{
    public ETCJoystick controlETCJoystick = null;

    Player mMyPlayer = null;
    PlayerData mMyPlayerData = null;
    bool m_IsGrounded;

    static float MAX_DISTANCE = 0.5f;
    static float MIN_DISTANCE = 0.01f;

    float m_GroundCheckDistance = MAX_DISTANCE;

    void Start()
    {
        Invoke("InitMyPlayer", 1);
    }

    private void Update()
    {
        if(mMyPlayer)
        {
            CheckGroundStatus(mMyPlayer.transform.position);
            mMyPlayer.mAnimator.SetBool("OnGround", m_IsGrounded);
            if (mMyPlayer.mCapsuleCollider) mMyPlayer.mCapsuleCollider.material.frictionCombine = m_IsGrounded ? PhysicMaterialCombine.Maximum : PhysicMaterialCombine.Multiply;

            if(!m_IsGrounded)
            {
                m_GroundCheckDistance = mMyPlayer.mRigidbody.velocity.y < 0 ? MAX_DISTANCE : MIN_DISTANCE;
            }
        }
    }

    public void InitMyPlayer()
    {
        GameObject myPlayerGO = RoleMgr.singleton.GetPlayer(0);
        if (myPlayerGO)
        {
            mMyPlayer = myPlayerGO.GetComponent<Player>();
            mMyPlayerData = mMyPlayer.mRoleData as PlayerData;
        }

        if (controlETCJoystick == null)
        {
            controlETCJoystick = ETCInput.GetControlJoystick("Joystick");
            if (controlETCJoystick && controlETCJoystick.cameraLookAt == null)
            {
                controlETCJoystick.cameraLookAt = mMyPlayer.transform;
            }
        }

    }

    public void JoyStickStart()
    {
        if (mMyPlayerData != null)
        {
            if (m_IsGrounded)
            {
                mMyPlayer.mStateMachine.SwitchState(IState.StateType.Grounded, 1.0f, null);
            }
        }
    }

    public void JoyStickMove()
    {
        if (mMyPlayerData != null)
        {
            if (mMyPlayer.mBuffSystem.CanMove())
            {
                if(m_IsGrounded)
                {
                    mMyPlayer.mAnimator.SetFloat("Speed", 1.0f);
                    mMyPlayer.mStateMachine.SwitchState(IState.StateType.Grounded, null, null);
                }
                else
                {
                    mMyPlayer.mStateMachine.SwitchState(IState.StateType.Airborne, null, null);
                }

                Quaternion quat = Quaternion.LookRotation(controlETCJoystick.cameraTransform.forward);
                Vector3 dir = new Vector3(controlETCJoystick.axisX.axisValue, 0, controlETCJoystick.axisY.axisValue);
                //dir = quat * dir;
                mMyPlayerData.SetForward(dir);
            }
            else
            {
                mMyPlayer.mAnimator.SetFloat("Speed", 0.0f);
                if (m_IsGrounded)
                {
                    mMyPlayer.mStateMachine.SwitchState(IState.StateType.Grounded, null, null);
                }
            }
        }
    }

    public void JoyStickEnd()
    {
        if (mMyPlayerData != null)
        {
            mMyPlayer.mAnimator.SetFloat("Speed", 0.0f);
            if (m_IsGrounded)
            {
                mMyPlayer.mStateMachine.SwitchState(IState.StateType.Grounded, null, null);
            }
        }
    }

    public void OnBtnSkill(int i)
    {
        if (mMyPlayer == null) return;

        if (!mMyPlayer.mBuffSystem.CanAtk())
        {
            return;
        }

        IState.StateType state = IState.StateType.None;
        if (i == 0) // 连击
        {
            mMyPlayer.mRoleData.mCurrSkillId = mMyPlayer.mRoleData.mMulSkillList[0];
            state = IState.StateType.MultlAtk;
        }
        else if (i == 1)
        {
            mMyPlayer.mRoleData.mCurrSkillId = mMyPlayer.mRoleData.mSkillList[0];
            state = IState.StateType.Atk_TL;
        }
        else if (i == 2)
        {
            mMyPlayer.mRoleData.mCurrSkillId = mMyPlayer.mRoleData.mSkillList[1];
            state = IState.StateType.Atk_TL;
        }
        else if (i == 3)
        {
            mMyPlayer.mRoleData.mCurrSkillId = mMyPlayer.mRoleData.mSkillList[2];
            state = IState.StateType.Atk_TL;
        }

        SkillConfig.SkillInfo skillInfo = SkillConfig.singleton.GetSkillInfo(mMyPlayer.mRoleData.mCurrSkillId);
        if (skillInfo != null)
        {
            if(skillInfo.mTargetType == SkillConfig.TargetType.None)
            {
                mMyPlayer.mStateMachine.SwitchState(state, mMyPlayer.mRoleData.mCurrSkillId, null);
            }
            else
            {
                if(skillInfo.mTargetType == SkillConfig.TargetType.Enemy)
                {
                    mMyPlayer.mRoleData.SetTargetRole(RoleMgr.singleton.FindNearMonster(mMyPlayer.transform.position));
                }
                else if(skillInfo.mTargetType == SkillConfig.TargetType.Friends)
                {

                }
                else if (skillInfo.mTargetType == SkillConfig.TargetType.Self)
                {

                }

                if (mMyPlayer.mRoleData.GetTargetRole())
                {
                    float dis = Vector3.Distance(mMyPlayer.mRoleData.GetTargetRole().transform.position, mMyPlayer.transform.position);
                    if (skillInfo.mAtkDistance < dis)
                    {
                        mMyPlayer.mStateMachine.SetNextState(state, mMyPlayer.mRoleData.mCurrSkillId, null, 1);
                        mMyPlayer.mStateMachine.SwitchState(IState.StateType.TrailingObj, 1.0f, skillInfo.mAtkDistance);
                    }
                    else
                    {
                        mMyPlayer.mStateMachine.SwitchState(state, mMyPlayer.mRoleData.mCurrSkillId, null);
                    }
                }
            }
        }
    }

    public void OnBtnJump()
    {
        if (mMyPlayer.mBuffSystem.CanMove())
        {
            if (m_IsGrounded)
            {
                m_GroundCheckDistance = MIN_DISTANCE;
                mMyPlayer.mAnimator.applyRootMotion = false;
                RoleData roleData = mMyPlayer.mRoleData;

                float speed = mMyPlayer.mAnimator.GetFloat("Speed");
                if (speed > 0.0f)
                {
                    mMyPlayer.mRigidbody.velocity = new Vector3(mMyPlayer.transform.forward.x * roleData.mMoveSpeed, roleData.mMoveSpeed, mMyPlayer.transform.forward.z * roleData.mMoveSpeed);
                }
                else
                {
                    mMyPlayer.mRigidbody.velocity = new Vector3(0, roleData.mMoveSpeed, 0);
                }

                mMyPlayer.mStateMachine.SwitchState(IState.StateType.Airborne, null, null);
            }
        }
    }

    public void CheckGroundStatus(Vector3 pos)
    {
        //int LayerGround = 1 << LayerMask.NameToLayer("Ground");
        RaycastHit hitInfo;
        if (Physics.Raycast(pos + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            mMyPlayer.mAnimator.applyRootMotion = true;
            m_IsGrounded = true;
        }
        else
        {
            mMyPlayer.mAnimator.applyRootMotion = false;
            m_IsGrounded = false;
        }
    }
}

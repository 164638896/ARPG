using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GroupType
{
    None,
    FriendPlayer,
    EnemyPlayer,
    Monster,
}

public class RoleData
{
    // 需要同步的数据
    public int mInstId;
    public int mMoveSpeed;
    public int mHp;
    public int mAtk; // 攻击
    public int mDef; // 防御
    public int mCurrSkillId = 0; // 当前使用技能
    private int mTargetInstId = -1;
    private Vector3 mForward = new Vector3(0, 0, 0); //移动方向

    // 可以读表
    public GroupType mGroupType;
    public Object mObjPrefab;
    private Role mTargetRole = null;

    public void SetTargetRole(int targetInstId)  // 通过服务器发过来的目标id找到mTargetRole
    {
        mTargetInstId = targetInstId;
        mTargetRole = RoleMgr.singleton.GetRole(mTargetInstId);
    }

    public Role GetTargetRole()
    {
        return mTargetRole;
    }

    public List<int> mMulSkillList = new List<int>(); // 连击(普通攻击)
    public List<int> mSkillList = new List<int>();   // 技能ID列表

    public void SetForward(Vector3 targetPos, Vector3 currPos)
    {
        mForward = (targetPos - currPos);
        mForward.y = 0;
        mForward.Normalize();
    }

    public void SetForward(Vector3 forward)
    {
        mForward = forward;
        mForward.y = 0;
        mForward.Normalize();
    }

    public Vector3 GetForward() { return mForward; }
}

public class MonsterData : RoleData
{
    public Vector3 mBornPoint;
    public GameObject mGameObject;
}

public class PlayerData : RoleData
{

}
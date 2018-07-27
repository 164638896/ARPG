using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderParam : MonoBehaviour
{
    public int mSkillId = 0;
    private Role mUser;
    private HashSet<int> mColliderList = new HashSet<int>();

    public Role GetUserRole()
    {
        return mUser;
    }

    public void InitCollider(Role role)
    {
        mColliderList.Clear();
        mUser = role;
    }

    public void AddCollider(Role role)
    {
        mColliderList.Add(role.GetInstanceID());
    }

    public bool HasCollider(Role role)
    {
        return mColliderList.Contains(role.GetInstanceID());
    }

    public void ClearCollider()
    {
        mColliderList.Clear();
        mUser = null;
    }
}

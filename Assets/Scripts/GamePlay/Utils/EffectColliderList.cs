using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectColliderList : MonoBehaviour
{
    public List<Collider> mColliderList = new List<Collider>();

    public void SetColliders(Role user, int skillId)
    {
        for (int i = 0; i < mColliderList.Count; ++i)
        {
            ColliderParam eCollider = mColliderList[i].GetComponent<ColliderParam>();
            if(eCollider)
            {
                eCollider.InitCollider(user);
                eCollider.mSkillId = skillId;
            }
        }
    }
}

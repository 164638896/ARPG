using System;
using UnityEngine;
using UnityEngine.Playables;

public class PlayableAssetEx : PlayableAsset
{
   public RoleAttackState_TL mParamAttackTL;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // 2.创建
        var scriptPlayable = ScriptPlayable<PlayableBehaviourEx>.Create(graph);

        scriptPlayable.GetBehaviour().mRoleAttackTL = mParamAttackTL;
        return scriptPlayable;
    }
}
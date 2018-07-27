using System;
using UnityEngine;
using UnityEngine.Playables;

public class PlayableAssetEx : PlayableAsset
{
   public RoleAttackState_TL mTL;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var scriptPlayable = ScriptPlayable<PlayableBehaviourEx>.Create(graph);

        scriptPlayable.GetBehaviour().mTL = mTL;
        return scriptPlayable;
    }
}
using System;
using UnityEngine;
using UnityEngine.Playables;

public class PlayableBehaviourEx : PlayableBehaviour
{
    public RoleAttackState_TL mTL;

    //public override void OnGraphStart(Playable playable)
    //{
    //    base.OnGraphStart(playable);
    //}

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        mTL.OnBehaviourPlay();
    }

    //public override void PrepareFrame(Playable playable, FrameData info)
    //{
    //    base.PrepareFrame(playable, info);
    //    Debug.Log("PrepareFrame Called");
    //}

    //public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    //{
    //    base.ProcessFrame(playable, info, playerData);
    //    Debug.Log("ProcessFrame Called");
    //}

    //public override void OnBehaviourDelay(Playable playable, FrameData info)
    //{
    //    base.OnBehaviourDelay(playable, info);
    //    Debug.Log("OnBehaviourDelay Called");
    //}

    //public override void OnBehaviourPause(Playable playable, FrameData info)
    //{
    //    base.OnBehaviourPause(playable, info);
    //    Debug.Log("OnBehaviourPause Called");
    //}

    //public override void OnGraphStop(Playable playable)
    //{
    //    base.OnGraphStop(playable);
    //    Debug.Log("OnGraphStop Called");
    //}

    //public override void OnPlayableCreate(Playable playable)
    //{
    //    base.OnPlayableCreate(playable);
    //    Debug.Log("OnPlayableCreate Called");
    //}

    //public override void OnPlayableDestroy(Playable playable)
    //{
    //    base.OnPlayableDestroy(playable);
    //    Debug.Log("OnPlayableDestroy Called");
    //}

    //public override void PrepareData(Playable playable, FrameData info)
    //{
    //    base.PrepareData(playable, info);
    //    Debug.Log("PrepareData Called");
    //}
}
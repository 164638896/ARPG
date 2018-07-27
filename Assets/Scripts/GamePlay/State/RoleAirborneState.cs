using UnityEngine;

public class RoleAirborne : IState
{
    public RoleAirborne(Role role, Animator animator, StateMachine stateMachine)
        : base(role, animator, stateMachine, IState.StateType.Airborne)
    {
        AddStateHash(Animator.StringToHash("Airborne"));
    }

    public override void OnAniEnter(int stateHash)
    {
        base.OnAniEnter(stateHash);
    }

    public override void OnLeave(int stateHash)
    {
        base.OnLeave(stateHash);
      
    }

    public override void OnFixedUpdate()
    {
        mRole.mRigidbody.AddForce(Physics.gravity);
        mRole.mAnimator.SetFloat("Jump", mRole.mRigidbody.velocity.y);
    }
}

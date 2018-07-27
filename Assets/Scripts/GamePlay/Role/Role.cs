using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class Role : MonoBehaviour
{
    public StateMachine mStateMachine = null;
    public BuffSystem mBuffSystem = null;
    public RoleData mRoleData = null;

    public Rigidbody mRigidbody = null;
    public Animator mAnimator = null;
    public NavMeshAgent mAgent = null;
    public CapsuleCollider mCapsuleCollider = null;

    void Awake()
    {
        mStateMachine = new StateMachine();
        mBuffSystem = new BuffSystem(this);
        mRigidbody = GetComponent<Rigidbody>();
        mAnimator = GetComponent<Animator>();
        mAgent = GetComponent<NavMeshAgent>();
        mCapsuleCollider = GetComponent<CapsuleCollider>();

        aWake();
    }

    private void Start()
    {
        start();
    }

    void Update()
    {
        update();
    }

    void FixedUpdate()
    {
        fixedUpdate();
    }

    void LateUpdate()
    {
        lateUpdate();
    }

    void OnTriggerEnter(Collider other)
    {
        onHitEnter(other);
    }

    public virtual void aWake()
    {

    }

    public virtual void start()
    {
        Animator animator = this.gameObject.GetComponent<Animator>();

        AnimatorStateMachine[] animStateMachine = animator.GetBehaviours<AnimatorStateMachine>();
        for (int i = 0; i < animStateMachine.Length; ++i)
        {
            animStateMachine[i].EnterStateCallBack = EnterAniState;
            animStateMachine[i].ExitStateCallBack = ExitAniState;
        }

        mStateMachine.RegistState(new RoleBuffState(this, animator, mStateMachine));
        mStateMachine.RegistState(new RoleGroundedState(this, animator, mStateMachine));
        mStateMachine.RegistState(new RoleAirborne(this, animator, mStateMachine));
        mStateMachine.RegistState(new RoleMoveToPosState(this, animator, mStateMachine));
        mStateMachine.RegistState(new RoleTrailingObjState(this, animator, mStateMachine));
        mStateMachine.RegistState(new RoleAttackState_TL(this, animator, mStateMachine));
        mStateMachine.RegistState(new RoleMultAtkState(this, animator, mStateMachine));
        mStateMachine.RegistState(new RoleDeathState(this, animator, mStateMachine));
        
        //mStateMachine.RegistState(new RoleAttackState(this, animator, mStateMachine));
        //mStateMachine.RegistState(new RoleMultAtkState_TL(this, animator, mStateMachine));
        //mStateMachine.RegistState(new RoleMultAtkOutState_TL(this, animator, mStateMachine));

        mStateMachine.SwitchState(IState.StateType.Grounded, null, null);
    }

    public virtual void update()
    {
        mStateMachine.OnUpdate();
    }

    public virtual void fixedUpdate()
    {
        mStateMachine.OnFixedUpdate();
        mBuffSystem.OnFixedUpdate();
    }

    public virtual void lateUpdate()
    {
        mStateMachine.OnLateUpdate();
    }

    public virtual void onHitEnter(Collider other)
    {
        mStateMachine.OnHitEnter(other);
    }

    public IState GetCurState()
    {
        return mStateMachine.GetCurState();
    }

    //public static float GetGroundHeight(Vector3 pos)
    //{
    //    int LayerGround = 1 << LayerMask.NameToLayer("Ground");
    //    RaycastHit hitInfo;
    //    if (Physics.Raycast(pos + (Vector3.up)*0.2f, Vector3.down, out hitInfo, 20.0f, LayerGround))
    //    {
    //        return hitInfo.point.y;
    //    }
    //    else
    //    {
    //        return pos.y;
    //    }
    //}

    void EnterAniState(int hashName)
    {
        mStateMachine.OnEnterAniState(hashName);
    }

    void ExitAniState(int hashName)
    {
        mStateMachine.OnExitAniState(hashName);
    }
}

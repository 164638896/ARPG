using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateMachine
{
    private Dictionary<int, IState> m_dictState;

    private IState m_curState;

    public class NextStateInfo
    {
        public IState.StateType stateType = IState.StateType.None;
        public object mParam1;
        public object mParam2;

        public NextStateInfo(IState.StateType sType, object param1, object param2)
        {
            stateType = sType;
            mParam1 = param1;
            mParam2 = param2;
        }
        public bool isUse = false;
    }

    private List<NextStateInfo> m_stateInfoList;

    public StateMachine()
    {
        m_curState = null;
        m_dictState = new Dictionary<int, IState>();
        m_stateInfoList = new List<NextStateInfo>();
        m_stateInfoList.Add(new NextStateInfo(IState.StateType.Grounded, null, null));
        m_stateInfoList.Add(new NextStateInfo(IState.StateType.None, null, null));
    }

    public bool RegistState(IState state)
    {
        if (null == state)
        {
            Debug.LogError("StateMachine::RegistState->state is null");
            return false;
        }

        if (m_dictState.ContainsKey((int)state.GetStateType()))
        {
            Debug.LogError("StateMachine::RegistState->state had exist! state id=" + state.GetStateType());
            return false;
        }

        m_dictState[(int)state.GetStateType()] = state;

        return true;
    }

    public void OnEnterAniState(int hashName)
    {
        foreach (var item in m_dictState)
        {
            if (item.Value.HasStateHash(hashName))
            {
                item.Value.OnAniEnter(hashName);
            }
        }
    }

    public void OnExitAniState(int hashName)
    {
        foreach (var item in m_dictState)
        {
            if (item.Value.HasStateHash(hashName))
            {
                item.Value.OnLeave(hashName);
            }
        }
    }

    public IState GetState(int iStateId)
    {
        IState ret = null;
        m_dictState.TryGetValue(iStateId, out ret);
        return ret;
    }

    public bool SwitchState(IState.StateType stateType, object param1, object param2)
    {
        if (null != m_curState && m_curState.GetStateType() == stateType)
        {
            return m_curState.ExecuteStateAgain(this, param1, param2);
        }

        if (null != m_curState && !m_curState.mChangeState)
        {
            // 不可切换，等待此状态完成。
            return false;
        }

        IState newState = null;
        m_dictState.TryGetValue((int)stateType, out newState);
        if (null == newState)
        {
            return false;
        }

        IState oldState = m_curState;

        if (null != oldState)
        {
            oldState.OnLeave(0);
        }

        m_curState = newState;

        if (null != newState)
        {
            newState.ExecuteState(this, oldState, param1, param2);
        }

        return true;
    }


    public bool nextState()
    {
        NextStateInfo nextStateInfo = null;
        for (int i = m_stateInfoList.Count - 1; i >= 0; --i)
        {
            if (m_stateInfoList[i].stateType != IState.StateType.None)
            {
                nextStateInfo = m_stateInfoList[i];
                if (nextStateInfo.isUse == false)
                {
                    nextStateInfo.isUse = true;
                    break;
                }
            }
        }

        return SetCurrState(nextStateInfo.stateType, nextStateInfo);
    }

    public IState GetCurState()
    {
        return m_curState;
    }

    public bool SetCurrState(IState.StateType stateType, NextStateInfo nextStateInfo = null)
    {
        if (null != m_curState && m_curState.GetStateType() == stateType) return false;

        IState newState = null;
        m_dictState.TryGetValue((int)stateType, out newState);
        if (null == newState)
        {
            return false;
        }

        IState oldState = m_curState;

        if (null != m_curState) m_curState.OnLeave(0);

        m_curState = newState;

        if (null != m_curState)
        {
            if (nextStateInfo != null)
            {
                m_curState.ExecuteState(this, oldState, nextStateInfo.mParam1, nextStateInfo.mParam2);
            }
            //else
            //{
            //    m_curState.ExecuteState(this, oldState, null, null);
            //}
        }

        return true;
    }

    public void SetNextState(IState.StateType nextStateType, object param1, object param2, int index = 0)
    {
        if (index >= 0 && index < m_stateInfoList.Count)
        {
            m_stateInfoList[index].stateType = nextStateType;
            m_stateInfoList[index].mParam1 = param1;
            m_stateInfoList[index].mParam2 = param2;
            m_stateInfoList[index].isUse = false;
        }
    }
    public void CancelNextState(int index = 0)
    {
        if (index > 0 && index < m_stateInfoList.Count)
        {
            m_stateInfoList[index].isUse = true;
        }
    }

    public IState.StateType GetCurStateType()
    {
        IState state = GetCurState();
        return (null == state) ? IState.StateType.None : state.GetStateType();
    }

    public bool IsInState(IState.StateType StateType)
    {
        if (null == m_curState)
        {
            return false;
        }

        return m_curState.GetStateType() == StateType;
    }

    public void OnUpdate()
    {
        if (null != m_curState)
        {
            m_curState.OnUpdate();
        }
    }

    public void OnFixedUpdate()
    {
        if (null != m_curState)
        {
            m_curState.OnFixedUpdate();
        }
    }

    public void OnLateUpdate()
    {
        if (null != m_curState)
        {
            m_curState.OnLateUpdate();
        }
    }

    public void OnHitEnter(Collider other)
    {
        if (null != m_curState)
        {
            m_curState.OnHitEnter(other);
        }
    }
}

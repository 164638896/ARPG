using System;
using System.Collections.Generic;
using System.Diagnostics;

public abstract class AbstractTimeTask
{
    private uint m_uiTimeId;//任务id
    private int m_iInterval;//任务重复时间间隔,为0不重复
    private ulong m_ulNextTick;//下一次触发的时间点

    public uint TimeId
    {
        get
        {
            return m_uiTimeId;
        }
        set
        {
            m_uiTimeId = value;
        }
    }
    public int Interval
    {
        get { return m_iInterval; }
        set { m_iInterval = value; }
    }
    public ulong NextTick
    {
        get
        {
            return m_ulNextTick;
        }
        set
        {
            this.m_ulNextTick = value;
        }
    }
 
    public abstract Action Action
    {
        get;
        set;
    }
 
    public abstract void DoAction();

}

public class TimeTask : AbstractTimeTask
{
    private Action m_action;//定义自己的委托
 
    public override Action Action
    {
        get
        {
            return m_action;
        }
        set
        {
            m_action = value;
        }
    }
 
    public override void DoAction()
    {
        m_action();
    }

}

public class TimeTaskManager
{
    private static uint m_uiNextTimeId;//总的id，需要分配给task，也就是每加如一个task，就自增
    private static uint m_uiTick;//总的时间，用来和task里面的nexttick变量来进行比较，看是否要触发任务
    private static Queue<AbstractTimeTask> m_queue;
    private static Stopwatch m_stopWatch;
    private static readonly object m_queueLock = new object();//队列锁
    private static List<Action> mRemoveList;
    private TimeTaskManager()
    {

    }

    static TimeTaskManager()
    {
        m_queue = new Queue<AbstractTimeTask>();
        m_stopWatch = new Stopwatch();
        mRemoveList = new List<Action>();
    }

    public static uint AddTimer(uint start, int interval, Action action)
    {
        AbstractTimeTask task = GetTimeTask(new TimeTask(), start, interval, action);
        lock (m_queueLock)
        {
            m_queue.Enqueue(task);
        }
        return task.TimeId;
    }

    public static bool RemoveTimer(Action action)
    {
        mRemoveList.Add(action);

        return true;
    }

    public static void Tick()
    {
        TimeTaskManager.m_uiTick += (uint)(m_stopWatch.ElapsedMilliseconds);
        //nityEngine.Debug.Log(TimeTaskManager.m_uiTick);
        m_stopWatch.Reset();
        m_stopWatch.Start();

        while (m_queue.Count != 0)
        {
            AbstractTimeTask task;
            lock (m_queueLock)
            {
                task = m_queue.Peek();
                if(mRemoveList.Contains(task.Action))
                {
                    m_queue.Dequeue();
                    mRemoveList.Remove(task.Action);
                    break;
                }
            }

            if (TimeTaskManager.m_uiTick < task.NextTick)//如果程序的总时间小于task要执行的时间点，就break点，继续等待
            {
                break;
            }

            lock (m_queueLock)
            {
                m_queue.Dequeue();
            }

            if (task.Interval > 0)//如果需要重复的话
            {
                task.NextTick += (ulong)task.Interval;
                lock (m_queueLock)
                {
                    m_queue.Enqueue(task);//再次加入队列里面，注意哦，id不变的
                }
                task.DoAction();
            }
            else
            {
                task.DoAction();//执行委托
            }
        }
    }

    private static AbstractTimeTask GetTimeTask(AbstractTimeTask task, uint start, int interval, Action action)
    {
        task.Interval = interval;
        task.TimeId = ++TimeTaskManager.m_uiNextTimeId;
        task.NextTick = TimeTaskManager.m_uiTick + start;
        task.Action = action;
        return task;
    }
}


//=====================================================
/*
    Actor将各Client分离，消息传递分离
*/
//=====================================================



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor {
    #region 字段
    //锁，用于Queue<ActorMessage>队列操作控制
    protected object m_lockobj = new object ();
    //Actor的唯一标识，递增
    protected int m_id;
    //停止 MonoBehaviour的协程行为
    protected bool m_isStop = false;
    protected MonoBehaviour m_monobehaviour = null;
    protected Queue<ActorMessage> m_msgQueue;
    #endregion

    #region 属性
    public int Id {
        get { return m_id; }
        set { m_id = value; }
    }
    public MonoBehaviour Momo
    {
        get { return m_monobehaviour; }
    }
    #endregion

    #region 构造函数
    public Actor(MonoBehaviour mono)
    {
        this.m_monobehaviour = mono;
        m_msgQueue = new Queue<ActorMessage>();
    }
    #endregion

    #region 方法
    public virtual void Init()
    {
        //开启携程
        if(m_monobehaviour != null)
            m_monobehaviour.StartCoroutine(Dispatch());
    }
    private IEnumerator Dispatch()
    {
        while (!m_isStop)
        {
            if (m_msgQueue.Count > 0)
            {
                ActorMessage msg = null;
                lock (m_lockobj)
                {
                    msg = m_msgQueue.Dequeue();
                }
                ReceiveMsg(msg);
            }
            else
            {
                yield return null;
            }
        }
    }
    protected virtual void ReceiveMsg(ActorMessage msg)
    {

    }
    public void Stop()
    {
        m_isStop = true;
    }
    #endregion

    #region Actor之间通信
    //Actor给Actor发送string类型的信息
    public void SendMsg(string msg)
    {
        if(string.IsNullOrEmpty(msg)) return;
        var m = new ActorMessage();
        m.msg = msg;
        lock (m_lockobj)
        {
            m_msgQueue.Enqueue(m);
        }
    }
    //Actor给Actor发送Packet类型的信息
    public void SendMsg(Packet packet)
    {
        if(packet == null) return;
        var m = new ActorMessage() { packet = packet };
        lock (m_lockobj)
        {
            m_msgQueue.Enqueue(m);
        }
    }
    //Actor给Actor发送ActorMessage类型的信心
    public void SendMsg(ActorMessage msg)
    {
        if(msg == null) return;
        lock (m_lockobj)
        {
            m_msgQueue.Enqueue(msg);
        }
    }
    #endregion
}
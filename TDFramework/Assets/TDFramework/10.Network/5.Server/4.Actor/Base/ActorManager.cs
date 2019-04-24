

//=====================================================
/*
    管理Actor
*/
//=====================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ActorManager
{
    #region 字段
    public static ActorManager Instance;
    //根据Actor的Id来缓存Actor
    Dictionary<int, Actor> m_actorDict;
    //根据Actor的Type来缓存Actor
    Dictionary<Type, Actor> m_actorType;
    //进行原子操作递增， 用于Actor的唯一标识
    private int m_actorId = 0;
    //用于停止ActorManager管理的所有Actor
    private bool m_isStop = false;
    #endregion

    #region 构造函数
    public ActorManager()
    {
        Instance = this;
        m_actorDict = new Dictionary<int, Actor>();
        m_actorType = new Dictionary<Type, Actor>();
    }
    #endregion

    #region 方法
    public int AddActor(Actor actor, bool addType = false)
    {
        if (m_isStop)
        {
            return -1;
        }
        //原子操作
        int actorId = Interlocked.Increment(ref m_actorId); 
        lock (m_actorDict)
        {
            m_actorDict.Add(actorId, actor);
            if (addType)
            {
                m_actorType.Add(actor.GetType(), actor);
            }
        }
        actor.Id = actorId;
        //向ActorManager添加Actor的时候，就初始化Actor，并启动Actor的分发消息任务
        actor.Init();       
        return actorId;
    }
    public void RemoveActor(int actorId)
    {
        lock (m_actorDict)
        {
            if (m_actorDict.ContainsKey(actorId))
            {
                Actor actor = m_actorDict[actorId];
                if (m_actorType.ContainsKey(actor.GetType()))
                {
                    Actor actor2 = m_actorType[actor.GetType()];
                    if (actor2 == actor)
                    {
                        actor.Stop();
                        m_actorType.Remove(actor.GetType());
                    }
                }
                m_actorDict.Remove(actorId);
            }
        }
    }
    public Actor GetActor(int actorId)
    {
        Actor actor = null;
        lock (m_actorDict)
        {
            m_actorDict.TryGetValue(actorId, out actor);
        }
        return actor;
    }
    public T GetActor<T>() where T : Actor
    {
        T actor = null;
        lock (m_actorDict)
        {
            Actor temp = null;
            m_actorType.TryGetValue(typeof(T), out temp);
            actor = (T)temp;
        }
        return actor;
    }
    //停止所有的Actor
    public void Stop()
    {
        m_isStop = true;
        lock (m_actorDict)
        {
            foreach (var actor in m_actorDict)
            {
                actor.Value.Stop();
            }
        }
    }
    #endregion
}

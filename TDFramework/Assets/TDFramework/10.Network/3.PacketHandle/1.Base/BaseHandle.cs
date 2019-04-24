
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHandle
{
    #region 字段
    public string Name;
    protected Agent m_agent;
    protected WorldActor m_worldActor;
    protected PlayerActor m_playerActor;
    #endregion

    #region 构造函数
    public BaseHandle(Agent agent, WorldActor worldActor, PlayerActor playerActor)
    {
        m_agent = agent;
        m_worldActor = worldActor;
        m_playerActor = playerActor;
    }
    #endregion

    #region 虚方法
    public virtual void ReceivePacket(Packet packet)
    {
        
    }
    #endregion
}

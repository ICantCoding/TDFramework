
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActor : Actor
{
    
    #region 字段
    //U3D客户端ID唯一标识
    private UInt16 m_u3dId;
    //Agent的Id
    private uint m_agentId;
    //Agent
    private Agent m_agent;
    //世界Actor
    private WorldActor m_worldActor;
    #endregion

    #region 属性
    public uint AgentId
    {
        get { return m_agentId; }
        set { m_agentId = value; }
    }
    public Agent Agent
    {
        get { return m_agent; }
    }
    #endregion

    #region 构造函数
    public PlayerActor(uint agentId, MonoBehaviour mono) : base(mono)
    {
        AgentId = agentId;
        ServerActor server = ActorManager.Instance.GetActor<ServerActor>();
        m_agent = server.GetAgent(m_agentId);
        m_agent.Actor = this;
        m_worldActor = ActorManager.Instance.GetActor<WorldActor>();
    }
    #endregion

    #region 重载方法
    protected override void ReceiveMsg(ActorMessage actorMsg)
    {
        if (actorMsg == null) return;
        if (!string.IsNullOrEmpty(actorMsg.msg))
        {
            HandleMsgStr(actorMsg.msg);
        }
        if (actorMsg.packet != null)
        {
            //处理ActorMessage中携带Packet内容的情况
            Packet packet = actorMsg.packet;
            HandlePacket(packet);
        }
    }
    #endregion

    #region 接收到客户端消息处理
    private void HandleMsgStr(string msg)
    {

    }
    private void HandlePacket(Packet packet)
    {
        UInt16 firstId = packet.m_firstId;
        UInt16 secondId = packet.m_secondId;
        BaseHandle handle = PacketHandleMap.Instance.GetHandleInstantiateObj(firstId, secondId, m_agent, m_worldActor, this);
        if (handle != null)
        {
            handle.ReceivePacket(packet);
        }
    }
    #endregion
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchDogActor : Actor
{
    #region 常量
    private const string CreatePlayerActorStr = "CreatePlayerActor";
    private const string DestroyPlayerActorStr = "DestroyPlayerActor";
    private const char SplitChar = '|';
    #endregion

    #region 字段
    private WorldActor m_worldActor = null;
    private ServerActor m_serverActor = null;
    #endregion

    #region 构造函数
    public WatchDogActor(MonoBehaviour mono) : base(mono)
    {

    }
    #endregion

    #region 属性
    public WorldActor WorldActor
    {
        get { return m_worldActor; }
        set { m_worldActor = value; }
    }
    public ServerActor ServerActor
    {
        get { return m_serverActor; }
        set { m_serverActor = value; }
    }
    #endregion

    #region 重写基类方法
    protected override void ReceiveMsg(ActorMessage actorMsg)
    {
        if (!string.IsNullOrEmpty(actorMsg.msg))
        {
            var cmds = actorMsg.msg.Split(SplitChar);
            if (cmds[0] == CreatePlayerActorStr)
            {
                var agentId = System.Convert.ToUInt32(cmds[1]);
                CreatePlayerActorCallback(agentId);
            }
            else if (cmds[0] == DestroyPlayerActorStr)
            {
                var agentId = System.Convert.ToUInt32(cmds[1]);
                DestroyPlayerActorCallback(agentId); //销毁PlayerActor
            }
        }
    }
    #endregion

    #region 看门狗发送特定的消息
    public void SendActorMessageToCreatePlayerActor(uint agentId)
    {
        SendMsg(string.Format("{0}|{1}", CreatePlayerActorStr, agentId));
    }
    public void SendActorMessageToDestroyPlayerActor(uint agentId)
    {
        SendMsg(string.Format("{0}|{1}", DestroyPlayerActorStr, agentId));
    }
    #endregion

    #region 方法
    private void CreatePlayerActorCallback(uint agentId)
    {
        //创建PlayerActor
        PlayerActor playerActor = new PlayerActor(agentId, m_monobehaviour); 
        ActorManager.Instance.AddActor(playerActor);
    }
    private void DestroyPlayerActorCallback(uint agentId)
    {
        Agent agent = ServerActor.GetAgent(agentId);
        if (agent == null) return;
        PlayerActor playerActor = (PlayerActor)agent.Actor;
        if (playerActor != null)
        {
            playerActor.Stop();
            ActorManager.Instance.RemoveActor(playerActor.Id);
            agent.Actor = null;
        }
        ServerActor.RemoveAgent(agent);
    }
    #endregion

}

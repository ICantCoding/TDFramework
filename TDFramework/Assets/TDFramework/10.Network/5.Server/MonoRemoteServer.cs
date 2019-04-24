using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoRemoteServer : MonoBehaviour {

    #region 字段
    private ActorManager m_actorManager = null;
    private WatchDogActor m_watchDogActor = null;
    private WorldActor m_worldActor = null;
    private ServerActor m_serverActor = null;
    #endregion

    #region Unity生命周期
    void Awake()
    {

    }
    void Start()
    {

    }
    #endregion

    #region 开启服务器引擎
    public void RunService (int serverPort) {
        if (m_actorManager != null) return;

        m_actorManager = new ActorManager ();

        m_watchDogActor = new WatchDogActor (this);
        //Id为1的Actor, 不与Agent绑定，也没有U3DId标识
        m_actorManager.AddActor (m_watchDogActor, true);

        m_worldActor = new WorldActor (this);
        m_watchDogActor.WorldActor = m_worldActor;
        //Id为2的Actor, 不与Agent绑定，也没有U3DId标识
        m_actorManager.AddActor (m_worldActor, true);

        m_serverActor = new ServerActor (this);
        m_watchDogActor.ServerActor = m_serverActor;
        m_serverActor.WatchDogActor = m_watchDogActor;
        //Id为3的Actor, 不与Agent绑定，也没有U3DId标识
        m_actorManager.AddActor (m_serverActor, true);

        //启动服务器
        m_serverActor.Run(serverPort);
    }
    #endregion

    #region 关闭服务器
    //目前暂无关闭服务器功能
    public void StopServive()
    {

    }
    #endregion
}
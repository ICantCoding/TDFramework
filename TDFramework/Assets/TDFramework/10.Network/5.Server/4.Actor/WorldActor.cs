using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldActor : Actor {
    #region 字段
    //使用字典管理U3DPlayerActor, Key为PlayerActor的U3DId客户端唯一编号
    private Dictionary<ushort, PlayerActor> m_playerActorDict;
    #endregion

    #region 构造函数
    public WorldActor (MonoBehaviour mono) : base (mono) {
        m_playerActorDict = new Dictionary<UInt16, PlayerActor> ();

    }
    #endregion

    #region 重载方法
    protected override void ReceiveMsg (ActorMessage actorMsg) {
        if (!string.IsNullOrEmpty (actorMsg.msg)) {

        }
    }
    public override void Init() {
        base.Init ();
        m_monobehaviour.StartCoroutine (UpdateWorld()); //用于定时更新需要的信息
    }
    private IEnumerator UpdateWorld() {
        while (!m_isStop) {
            yield return new WaitForSeconds (1); //每秒定时更新一次信息
        }
    }
    #endregion
}
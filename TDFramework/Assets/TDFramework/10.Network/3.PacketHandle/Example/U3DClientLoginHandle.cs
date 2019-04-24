

using System;
using System.Collections;
using System.Collections.Generic;
using TDFramework;

public class U3DClientLoginHandle : BaseHandle
{
    #region 构造函数
    public U3DClientLoginHandle(Agent agent, WorldActor worldActor, PlayerActor playerActor) :
        base(agent, worldActor, playerActor)
    {
        Name = "U3DClientLoginHandle";
    }
    #endregion

    #region 重写方法
    public override void ReceivePacket(Packet packet)
    {
        if(packet == null) return;
        U3DClientLogin u3dClientLogin = new U3DClientLogin(packet.m_data);
        System.Net.IPEndPoint ipEndPoint = (System.Net.IPEndPoint)m_agent.EndPoint;
        object[] objs = new object[2];
        objs[0] = u3dClientLogin;
        objs[1] = ipEndPoint;
    }
    #endregion
}

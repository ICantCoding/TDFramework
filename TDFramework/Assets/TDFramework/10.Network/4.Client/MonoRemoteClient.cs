using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoRemoteClient : MonoBehaviour, IMonoRemoteClient {

    #region 字段
    //待处理的数据包Packet队列.
    protected Queue<Packet> m_pendingPacketQueue = new Queue<Packet> ();
    protected RemoteClient m_remoteClient = null;
    #endregion

    #region Unity生命周期
    protected virtual void Awake () {

    }
    protected virtual void OnDestroy () {
        Stop ();
    }
    protected virtual void OnApplicationQuit () {
        Stop ();
    }
    #endregion

    #region 方法
    public virtual void Run (string serverIp, int serverPort, RemoteClientConnectServerSuccessCallback successCallback,
        RemoteClientConnectServerFailCallback failCallback) {
        m_remoteClient = new RemoteClient (this, serverIp, serverPort);
        if (m_remoteClient != null) {
            m_remoteClient.ConnectServerSuccessCallback = successCallback;
            m_remoteClient.ConnectServerFailCallback = failCallback;
            m_remoteClient.Connect ();
            StartCoroutine (UpdateInMainThread4PacketQueue ());
        }
    }
    protected IEnumerator UpdateInMainThread4PacketQueue () {
        while (true) {
            while (m_pendingPacketQueue.Count > 0) {
                lock (this) {
                    Packet packet = m_pendingPacketQueue.Dequeue ();
                    if (packet != null) {
                        ushort firstId = packet.m_firstId;
                        ushort secondId = packet.m_secondId;
                        BaseHandle handle = PacketHandleMap.Instance.GetHandleInstantiateObj (firstId, secondId, this);
                        if (handle != null) {
                            handle.ReceivePacket (packet);
                        }
                    }
                }
            }
            yield return null;
        }
    }
    public virtual void Stop () {
        if (m_remoteClient != null) {
            m_remoteClient.Close ();
        }
        StopAllCoroutines ();
    }
    //重连服务器
    public virtual void ReLink (string serverIp, int serverPort, RemoteClientConnectServerSuccessCallback successCallback,
        RemoteClientConnectServerFailCallback failCallback) {
        Stop ();
        Run (serverIp, serverPort, successCallback, failCallback);
    }
    #endregion

    #region IMonoRemoteClient接口实现
    public void Packet2EnqueueQueue (Packet packet) {
        lock (this) {
            m_pendingPacketQueue.Enqueue (packet);
        }
    }
    #endregion

    #region 举例发送消息到服务器
    public void SendU3DClientLoginInfoRequest () {
        U3DClientLogin u3dClientLogin = new U3DClientLogin () {
            m_clientId = 0,
            m_clientName = "TianShanShan",
        };
        Packet packet = new Packet (u3dClientLogin.m_clientId, 0,
            0, 0, u3dClientLogin.Size, u3dClientLogin.Packet2Bytes ());
        if (m_remoteClient != null) {
            m_remoteClient.Send (packet.Packet2Bytes ());
        }
    }
    #endregion
}
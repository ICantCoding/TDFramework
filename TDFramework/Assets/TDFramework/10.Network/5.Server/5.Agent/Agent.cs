using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Agent {
    #region 常量静态字段
    private static readonly object lockObj = new object ();
    private static uint maxId = 0;
    private const int bufferSize = 2048;
    #endregion

    #region 字段
    private uint m_id; //客户端代理Id号
    private Socket m_socket;
    private bool m_isClose = false; //客户端代理是否关闭连接
    private EndPoint m_endPoint;
    private byte[] m_buffer = new byte[bufferSize]; //接收数据流缓存，Tcp数据流无界限
    private PacketReader m_packetReader = null; //接收数据流转完整包数据
    //Agent发送数据缓存列表，列表让发送的数据包有序
    private List<SendBuffer> m_sendBufferList = new List<SendBuffer> ();
    private Actor m_actor = null; //Actor模式，一切皆Actor, Agent与Actor绑定
    private WatchDogActor m_dogActor = null; //看门狗Actor
    private ServerActor m_server = null; //服务器Server
    #endregion

    #region 属性
    public uint Id {
        get { return m_id; }
    }
    public Actor Actor {
        get { return m_actor; }
        set { m_actor = value; }
    }
    public WatchDogActor DogActor {
        get { return m_dogActor; }
        set { m_dogActor = value; }
    }
    public ServerActor ServerActor {
        get { return m_server; }
        set { m_server = value; }
    }
    public EndPoint EndPoint {
        get { return m_endPoint; }
    }
    #endregion

    #region 构造函数
    public Agent (Socket socket) {
        lock (lockObj) {
            m_id = ++maxId;
        }
        m_socket = socket;
        m_endPoint = m_socket.RemoteEndPoint;
        m_packetReader = new PacketReader (HandleMessage);
    }
    #endregion

    #region 方法
    public void StartReceive () {
        if (Valid ()) {
            try {
                if (m_dogActor != null) {
                    //看门狗发送一个消息, 用于创建PlayerActor
                    m_dogActor.SendActorMessageToCreatePlayerActor (m_id);
                }
                //会开辟子线程, 该子线程阻塞, 等待数据流的到来
                m_socket.BeginReceive (m_buffer, 0, bufferSize, SocketFlags.None, OnReceiveCallback, m_socket);
            } catch (Exception exception) {
                Debug.LogError ("服务器BeginReceive时失败, Reason: " + exception.Message);
                Close ();
            }
        }
    }
    private void OnReceiveCallback (IAsyncResult ar) {
        int recvLen = 0;
        try {
            recvLen = m_socket.EndReceive (ar);
        } catch (Exception exception) {
            Debug.LogError ("服务器接收客户端发送的消息时, EndReceive失败. Reason: " + exception.Message);
            Close ();
            return;
        }
        if (recvLen <= 0) {
            Debug.LogError ("服务器接收客户端发送的消息失败, Reason: 消息长度<=0.");
            Close ();
        } else {
            uint num = (uint) recvLen;
            m_packetReader.Process (m_buffer, (uint) recvLen);
            try {
                //继续等待下个数据
                m_socket.BeginReceive (m_buffer, 0, bufferSize, SocketFlags.None, OnReceiveCallback, m_socket);
            } catch (Exception exception) {
                Debug.LogError ("服务器BeginReceive时失败, Reason: " + exception.Message);
                Close ();
            }
        }
    }
    //MessageReader对象获取到完整的一个数据包后，需执行这个回调
    void HandleMessage (Packet packet) {
        if (m_actor != null) {
            //Agent接收到客户端发送过来的完整数据包Packet后，将这个数据包通过该Agent的Actor进行顺序排序
            m_actor.SendMsg (packet); //Agent对应的PlayerActor会接受这个Packet并做处理
        }
    }
    //判断该客户端的Socket连接是否有效
    public bool Valid () {
        if (m_socket != null && m_socket.Connected) {
            return true;
        }
        return false;
    }
    //关闭该客户端的连接
    public void Close () {
        if (m_isClose) {
            return;
        }
        if (Valid ()) {
            try {
                m_socket.Shutdown (SocketShutdown.Both); //优雅关闭Socket连接
                m_socket.Close ();
            } catch (Exception exception) {
                Debug.LogError ("服务器关闭某个客户端连接时失败, Reason: " + exception.Message);
            }
        }
        m_socket = null;
        m_isClose = true;
        if (m_dogActor != null && m_actor != null) {
            //关闭Agent的时候，肯定要使用看门狗通知销毁PlayerActor
            m_dogActor.SendActorMessageToDestroyPlayerActor (m_id);
        }
    }
    //Agent发送数据包
    public void SendPacket (byte[] datas) {
        if (Valid () == false) {
            return;
        }
        lock (m_sendBufferList) {
            SendBuffer sb = new SendBuffer () {
                position = 0,
                buffer = datas,
            };
            m_sendBufferList.Add (sb); //需要发送的数据包先放入到发送缓存列表中
            if (m_sendBufferList.Count == 1) {
                try {
                    m_socket.BeginSend (sb.buffer, sb.position, sb.Size, SocketFlags.None, OnSendCallback, m_socket);
                } catch (Exception exception) {
                    Debug.LogError ("服务器BeginSend时失败, Reason: " + exception.Message);
                    Close ();
                }
            }
        }
    }
    private void OnSendCallback (IAsyncResult ar) {
        int num = 0;
        try {
            num = m_socket.EndSend (ar);
        } catch (Exception exception) {
            Close ();
            Debug.LogError ("服务器EndSend是失败, Reason: " + exception.Message);
            return;
        }
        lock (m_sendBufferList) {
            if (Valid ()) {
                var sb = m_sendBufferList[0];
                SendBuffer nextSb = null;
                if (sb.Size == num) {
                    //表明SendBuffer中的数据全部发送完成
                    m_sendBufferList.RemoveAt (0);
                    if (m_sendBufferList.Count > 0) {
                        nextSb = m_sendBufferList[0];
                    }
                } else if (sb.Size > num) {
                    //表明SendBuffer中的数据没有全部发送出去
                    sb.position += num;
                    nextSb = m_sendBufferList[0];
                } else {
                    //表明上一个SendBuffer数据包发送错误，我们就不管上一个数据包了
                    m_sendBufferList.RemoveAt (0);
                    if (m_sendBufferList.Count > 0) {
                        nextSb = m_sendBufferList[0];
                    }
                }
                if (nextSb != null) {
                    try {
                        // m_socket.BeginSendTo(nextSb.buffer, nextSb.position, nextSb.Size, SocketFlags.None, m_endPoint, OnSendCallback, null);
                        m_socket.BeginSend (nextSb.buffer, nextSb.position, nextSb.Size, SocketFlags.None, OnSendCallback, null);
                    } catch (Exception exception) {
                        //异步发送数据失败，就关闭Agent的Socket连接
                        Debug.LogError ("服务器BeginSend时失败, Reason: " + exception.Message);
                        Close ();
                    }
                }
            }
        }
    }
    #endregion
}
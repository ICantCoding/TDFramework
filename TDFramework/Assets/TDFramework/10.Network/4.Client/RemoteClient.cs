using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public delegate void RemoteClientConnectServerSuccessCallback ();
public delegate void RemoteClientConnectServerFailCallback ();

public class RemoteClient : MonoBehaviour {
    #region 常量
    private const int BufferSize = 8192;
    #endregion

    #region 回调代理
    //RemoteClient连接服务器成功的回调代理
    public RemoteClientConnectServerSuccessCallback ConnectServerSuccessCallback = null;
    //RemoteClient连接服务器失败的回调代理
    public RemoteClientConnectServerFailCallback ConnectServerFailCallback = null;
    #endregion

    #region 字段
    private string m_serverIp;
    private int m_serverPort;
    private Socket m_clientSocket;
    private IPEndPoint m_endPoint;
    private byte[] m_buffer = new byte[BufferSize]; //接收数据流缓存，Tcp数据流无界限
    private List<SendBuffer> m_sendBufferList = new List<SendBuffer> ();
    private PacketReader m_packetReader = null;
    private IMonoRemoteClient m_monoRemoteClient = null;
    #endregion

    #region 构造方法
    public RemoteClient (IMonoRemoteClient monoRemoteClient, string serverIp, int serverPort) {
        m_serverIp = serverIp;
        m_serverPort = serverPort;
        m_packetReader = new PacketReader (HandleMessage);
        m_monoRemoteClient = monoRemoteClient;
    }
    //PacketReader对象获取到完整的一个数据包后，需执行这个回调
    void HandleMessage (Packet packet) {
        if (m_monoRemoteClient != null) {
            m_monoRemoteClient.Packet2EnqueueQueue (packet);
        }
    }
    #endregion

    #region 开始与服务器连接方法
    //连接服务器
    public void Connect () {
        m_endPoint = new IPEndPoint (IPAddress.Parse(m_serverIp), m_serverPort);
        try {
            m_clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result = m_clientSocket.BeginConnect (m_endPoint, new AsyncCallback (OnConnectCallback), null);
            //开启线程，检查客户端连接服务器是否成功.
            Thread thread = new Thread (CheckConnected);
            thread.IsBackground = true;
            thread.Start (result);
        } catch (Exception exception) {
            Debug.LogError ("连接服务器失败, Reason: " + exception.Message);
            Close ();
        }
    }
    private void OnConnectCallback (IAsyncResult ar) {
        if (m_clientSocket == null) {
            return;
        }
        bool success = false;
        string reasonStr = "";
        try {
            m_clientSocket.EndConnect (ar);
            success = true;
        } catch (Exception exception) {
            Close();
            reasonStr = exception.Message;
            success = false;
        } finally {
            if (success) {
                //连接服务器成功， 可在此添加连接服务器成功的操作, 目前还不知道需要执行什么操作，暂时保留
                //开始接受数据
                StartReceive ();
                //连接服务器成功的回调
                if (ConnectServerSuccessCallback != null) {
                    ConnectServerSuccessCallback ();
                }
            } else {
                if (ConnectServerFailCallback != null) {
                    ConnectServerFailCallback ();
                }
                Debug.LogError ("客户端连接服务器失败, Reason: " + reasonStr);
            }
        }
    }
    private void CheckConnected (object obj) {
        IAsyncResult result = (IAsyncResult) obj;
        //阻止当前线程，直到当前 WaitHandle 收到信号，同时使用 32 位带符号整数指定时间间隔（以毫秒为单位）
        //到信号，则为 true
        if (result != null && !result.AsyncWaitHandle.WaitOne (3000)) //3秒无响应则连接服务器失败
        {
            Debug.LogError ("客户端连接服务器超时.");
            Close ();
        }
    }
    private void StartReceive () {
        try {
            if (Valid ()) {
                m_clientSocket.BeginReceive (m_buffer, 0, m_buffer.Length, SocketFlags.None,
                    new AsyncCallback (OnReceiveCallback), null);
            }
        } catch (Exception exception) {
            Debug.LogError ("客户端BeginReceive失败, Reason: " + exception.Message);
            Close ();
        }
    }
    private void OnReceiveCallback (IAsyncResult ar) {
        int bytes = 0;
        try {
            if (Valid ()) {
                bytes = m_clientSocket.EndReceive (ar);
            }
        } catch (Exception exception) {
            Debug.LogError ("客户端EndReceive失败, Reason: " + exception.Message);
            Close ();
            return;
        }
        if (bytes <= 0) {
            Close ();
        } else {
            uint num = (uint) bytes;
            if (m_packetReader != null) {
                m_packetReader.Process (m_buffer, num);
            }
            try {
                if (Valid ()) {
                    m_clientSocket.BeginReceive (m_buffer, 0, m_buffer.Length, SocketFlags.None,
                        new AsyncCallback (OnReceiveCallback), null);
                }
            } catch (Exception exception) {
                Debug.LogError ("客户端BeginReceive失败, Reason: " + exception.Message);
                Close ();
            }
        }
    }
    #endregion

    #region 断开与服务器的连接方法
    public void Close () {
        try {
            if (m_clientSocket != null && m_clientSocket.Connected) {
                //优雅关闭与服务器的连接
                m_clientSocket.Shutdown (SocketShutdown.Both);
            }
            m_clientSocket.Close ();
        } catch (Exception exception) {
            Debug.LogError ("客户端下线操作失败, Reason: " + exception.Message);
        }
        m_clientSocket = null;
    }
    #endregion

    #region 判断客户端是否连接有效
    public bool Valid () {
        if (m_clientSocket != null && m_clientSocket.Connected == true) {
            return true;
        }
        return false;
    }
    #endregion

    #region 向服务器发送消息
    public void Send (byte[] datas) {
        lock (m_sendBufferList) {
            SendBuffer sb = new SendBuffer () {
                position = 0,
                buffer = datas
            };
            m_sendBufferList.Add (sb);
            if (m_sendBufferList.Count == 1) {
                try {
                    IAsyncResult ar = m_clientSocket.BeginSend (sb.buffer, sb.position, sb.Size,
                        SocketFlags.None, new AsyncCallback (OnSendCallback), null);
                    //开启线程检查发送数据是否正确, 是否超时, 瞬间发送10000条数据，那这里岂不是瞬间要开启10000个线程，这里有问题哈！
                    //其实一般不会瞬间发送那么多消息，使用线程池也算是合理的
                    ThreadPool.QueueUserWorkItem (CheckSendTimeout, ar);
                } catch (Exception exception) {
                    Debug.LogError ("客户端发送Packet失败, Reason: " + exception.Message);
                    Close ();
                }
            }
        }
    }
    private void OnSendCallback(IAsyncResult ar)
    {
        int num = 0;
        try
        {
            num = m_clientSocket.EndSend(ar);
        }
        catch (Exception exception)
        {
            Close();
            Debug.LogError("客户端发送Packet失败，Reason: " + exception.Message);
            return;
        }
        lock (m_sendBufferList)
        {
            if (Valid())
            {
                SendBuffer sb = m_sendBufferList[0];
                SendBuffer nextSb = null;
                if (sb.Size == num)
                {
                    //表明SendBuffer中的数据全部发送完成
                    m_sendBufferList.RemoveAt(0);
                    if (m_sendBufferList.Count > 0)
                    {
                        nextSb = m_sendBufferList[0];
                    }
                }
                else if (sb.Size > num)
                {
                    //表明SendBuffer中的数据没有全部发送出去
                    sb.position += num;
                    nextSb = m_sendBufferList[0];
                }
                else
                {
                    //表明上一个SendBuffer数据包发送错误，我们就不管上一个数据包了
                    m_sendBufferList.RemoveAt(0);
                    if (m_sendBufferList.Count > 0)
                    {
                        nextSb = m_sendBufferList[0];
                    }
                }
                if (nextSb != null)
                {
                    try
                    {
                        // m_clientSocket.BeginSendTo(nextSb.buffer, nextSb.position, nextSb.Size,
                        //     SocketFlags.None, m_endPoint, OnSendCallback, null);
                        m_clientSocket.BeginSend(sb.buffer, sb.position, sb.Size,
                            SocketFlags.None, new AsyncCallback(OnSendCallback), null);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError("客户端发送Packet失败, Reason: " + exception.Message);
                        Close();
                    }
                }
            }
        }
    }
    private void CheckSendTimeout(object obj)
    {
        try
        {
            IAsyncResult ar = (IAsyncResult)obj;
            if (!ar.AsyncWaitHandle.WaitOne(2000))
            {
                // Debug.LogError("Send Timeout.");
                Debug.LogError("客户端发送Packet失败, Reason: 发送超时.");
                Close();
            }
            else
            {
                // UnityEngine.Debug.Log("消息发送成功.");
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("客户端发送Packet失败, Reason: 发送超时." + exception.Message);
        }
    }
    #endregion

}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerActor : Actor
{
    #region 字段
    private TcpListener m_tcpListener = null;
    //tcp服务器端口号
    private int m_tcpPort = 0;
    //监听客户端连接的子线程
    private Thread m_thread = null;
    //对Agent使用字典管理, Key为Agent的Id, Value为Agent对象
    private Dictionary<uint, Agent> m_agentDict = new Dictionary<uint, Agent>();
    private WatchDogActor m_dogActor; //看门狗Actor
    #endregion

    #region 属性
    public WatchDogActor WatchDogActor
    {
        get { return m_dogActor; }
        set { m_dogActor = value; }
    }
    #endregion

    #region 构造函数
    public ServerActor(MonoBehaviour mono) : base(mono)
    {

    }
    #endregion

    #region 启动和关闭服务器
    public bool Run(int tcpPort)
    {
        try
        {
            m_tcpPort = tcpPort;
            m_tcpListener = new TcpListener(IPAddress.Any, m_tcpPort);
            //设置最大挂载连接数为1000
            m_tcpListener.Start(1000); 
        }
        catch (System.Exception exception)
        {
            Debug.LogError("服务器TcpListener Start失败, Reason: " + exception.Message);
            return false;
        }
        m_thread = new Thread(new ThreadStart(ListenClientConnectedThreadFunction));
        m_thread.IsBackground = true;
        //开启一个子线程专门用于监听客户端的连接
        m_thread.Start(); 
        return true;
    }
    public new void Stop()
    {
        //先关闭所有的Agent客户端连接   
        CloseAllAgents();
        try
        {
            if (m_tcpListener != null)
            {
                m_tcpListener.Stop();
                m_tcpListener = null;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("服务器关闭失败. Reason: " + exception.Message);
            m_tcpListener = null;
        }

        try
        {
            if (m_thread != null)
            {
                m_thread.Abort();
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("服务器关闭，监听客户端连接线程终止失败. Reason: " + exception.Message);
        }

        Stop(); //ServerActor本身的m_isStop设置为true
    }
    #endregion

    #region 子线程方法
    private void ListenClientConnectedThreadFunction()
    {
        while (true)
        {
            //确定是否有挂起的连接请求, Pending()返回true, 表示有从客户端来的连接请求
            try
            {
                if (m_tcpListener != null && m_tcpListener.Pending())
                {
                    Socket socket = m_tcpListener.AcceptSocket();
                    AddAgent(socket);
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogError("TcpListener监听客户端连接失败, Reason: " + exception.Message);
            }
            Thread.Sleep(1);
        }
    }
    #endregion

    #region 数据管理方法
    public void AddAgent(Socket socket)
    {
        Agent agent = new Agent(socket);
        agent.DogActor = m_dogActor; //设置Agent的看门狗Actor
        agent.ServerActor = this;
        lock (m_agentDict)
        {
            m_agentDict.Add(agent.Id, agent);
        }
        agent.StartReceive();
    }
    public void RemoveAgent(Agent agent)
    {
        if (m_agentDict.ContainsKey(agent.Id))
        {
            lock (m_agentDict)
            {
                m_agentDict.Remove(agent.Id);
            }
        }
    }
    public Agent GetAgent(uint agentId)
    {
        Agent agent = null;
        lock (m_agentDict)
        {
            m_agentDict.TryGetValue(agentId, out agent);
        }
        return agent;
    }
    private void CloseAllAgents()
    {
        int count = m_agentDict.Count;
        List<Agent> agentList = new List<Agent>();
        foreach (var item in m_agentDict)
        {
            Agent agent = item.Value;
            agentList.Add(agent);
        }

        foreach (var agent in agentList)
        {
            agent.Close();
        }
    }
    #endregion
}

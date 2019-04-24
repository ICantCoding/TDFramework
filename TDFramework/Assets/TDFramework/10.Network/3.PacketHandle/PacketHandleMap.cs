using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TDFramework;


public class PacketHandleMap : Singleton<PacketHandleMap> 
{

    #region 字段
    private Dictionary<UInt16, Dictionary<UInt16, string>> m_packetHandleMap = 
        new Dictionary<UInt16, Dictionary<UInt16, string>> ();
    #endregion

    #region 构造函数
    public PacketHandleMap()
    {
        UInt16 firstId, secondId = 0;
        string handleClassName = "";
        //U3D登录信令
        firstId = 0;
        secondId = 0;
        handleClassName = "U3DClientLoginHandle";
        AddHandleClassName(firstId, secondId, handleClassName);
    }
    #endregion

    #region 方法
    private void AddHandleClassName(UInt16 firstId, UInt16 secondId, string handleClassName)
    {
        if (string.IsNullOrEmpty(handleClassName)) return;
        Dictionary<UInt16, string> dic = null;
        m_packetHandleMap.TryGetValue(firstId, out dic);
        if (dic == null)
        {
            dic = new Dictionary<UInt16, string>();
            m_packetHandleMap.Add(firstId, dic);
        }
        if (dic.ContainsKey(secondId) == false)
        {
            dic.Add(secondId, handleClassName);
        }
    }
    public BaseHandle GetHandleInstantiateObj(UInt16 firstId, UInt16 secondId,
        Agent agent, WorldActor worldActor, PlayerActor playerActor)
    {
        string handleClassName = "";
        if (m_packetHandleMap.ContainsKey(firstId))
        {
            if (m_packetHandleMap[firstId].ContainsKey(secondId))
            {
                handleClassName = m_packetHandleMap[firstId][secondId];
            }
        }
        if (string.IsNullOrEmpty(handleClassName)) return null;
        //带参数的反射类实例
        Assembly assembly = Type.GetType(handleClassName).Assembly;
        Object[] parameters = new Object[3];
        parameters[0] = agent;
        parameters[1] = worldActor;
        parameters[2] = playerActor;
        BaseHandle handle = (BaseHandle)Assembly.Load(assembly.FullName).CreateInstance(handleClassName, false, BindingFlags.Default, 
            null, parameters, null, null);
        return handle;
    }
    public BaseHandle GetHandleInstantiateObj(PacketID packetId, MonoRemoteClient monoRemoteClient)
    {
        return GetHandleInstantiateObj(packetId.FirstID, packetId.SecondID, monoRemoteClient);
    }
    public BaseHandle GetHandleInstantiateObj(UInt16 firstId, UInt16 secondId, MonoRemoteClient networkEngine)
    {
        string handleClassName = "";
        if (m_packetHandleMap.ContainsKey(firstId))
        {
            if (m_packetHandleMap[firstId].ContainsKey(secondId))
            {
                handleClassName = m_packetHandleMap[firstId][secondId];
            }
        }
        if (string.IsNullOrEmpty(handleClassName)) return null;
        //带参数的反射类实例
        Assembly assembly = Type.GetType(handleClassName).Assembly;
        Object[] parameters = new Object[1];
        parameters[0] = networkEngine;
        BaseHandle handle = (BaseHandle)Assembly.Load(assembly.FullName).CreateInstance(handleClassName, false, BindingFlags.Default, 
            null, parameters, null, null);
        return handle;
    }
    public void GetFirstIdAndSecondId(string handleClassName, out UInt16 firstId, out UInt16 secondId)
    {
        firstId = 0;
        secondId = 0;
        if (string.IsNullOrEmpty(handleClassName)) return;
        var enumerator = m_packetHandleMap.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var dic = enumerator.Current.Value;
            var enumerator2 = dic.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                string str = enumerator2.Current.Value;
                if (str == handleClassName)
                {
                    firstId = enumerator.Current.Key;
                    secondId = enumerator2.Current.Key;
                    enumerator2.Dispose();
                    enumerator.Dispose();
                    return;
                }
            }
            enumerator2.Dispose();
        }
        enumerator.Dispose();
    }
    #endregion
}

using System;

public interface IMonoRemoteClient
{
    //RemoteClient接收到的数据包压入队列中
    void Packet2EnqueueQueue(Packet packet); 
}

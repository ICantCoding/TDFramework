
//============================================================
/*
    Actor之间传递消息的数据格式
*/
//============================================================

using System;
using System.Collections;
using System.Collections.Generic;

public class ActorMessage {
    public string msg; //ActorMessage可携带string消息
    public Packet packet; //ActorMessage可携带Packet
    public object obj; //ActorMessage可携带一个object参数
    public object obj1; //ActorMessage可携带第二个object参数
}
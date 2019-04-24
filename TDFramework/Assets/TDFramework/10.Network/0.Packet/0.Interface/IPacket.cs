

//===========================================
/*
    Packet接口,继承该接口的类,具有将Packet转换成byte[]字节数组的功能
*/
//===========================================

using System;

public interface IPacket
{
    byte[] Packet2Bytes();
}

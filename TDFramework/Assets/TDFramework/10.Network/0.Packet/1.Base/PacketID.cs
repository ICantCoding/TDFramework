
using System;
using System.Collections;
using System.Collections.Generic;

public class PacketID
{
    #region 字段
    public ushort FirstID;
    public ushort SecondID;
    #endregion

    #region 构造函数
    public PacketID(ushort firstId, ushort secondId)
    {
        this.FirstID = firstId;
        this.SecondID = secondId;
    }
    #endregion
}

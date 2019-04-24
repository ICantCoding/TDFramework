
using System;

public class SendBuffer
{
    public int position = 0;
    public byte[] buffer;
    public int Size
    {
        get { return buffer.Length - position; }
    }
}

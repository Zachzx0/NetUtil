using System;
using System.Net;

namespace NetUtil
{

    public class NetData
    {
        public const int NetDataLen = 4;
        DataSocket mSocket = null;
        byte[] data;
        int length;

        public EndPoint RemoteEndPoint
        {
            get
            {
                if (mSocket != null)
                {
                    return mSocket.RemoteEndPoint;
                }
                return null;
            }
        }


        public NetData(DataSocket socket)
        {
            mSocket = socket;
        }

        public NetData(int len, byte[] recvData, DataSocket socket) : this(socket)
        {
            length = len;
            data = new byte[length];
            Array.Copy(recvData, data, recvData.Length);
        }

        public NetData(byte[] recvData,int offset,int curLen,int totalLen, DataSocket socket) : this(socket)
        {
            length = totalLen;
            data = new byte[totalLen];
            Array.Copy(recvData,offset, data,0, curLen);
        }

        public bool DataComplete()
        {
            return length == data.Length;
        }

        public int GetUnRecvLen()
        {
            return length - data.Length;
        }

        public bool MergeData(byte[] recvData,int offset,int length)
        {
            if (!DataComplete())
            {
                Array.Copy(recvData, offset, data, data.Length, length);
            }
            return DataComplete();
        }

        public byte[] GetData()
        {
            if (DataComplete())
            {
                return data;
            }
            return null;
        }


        #region 静态方法
        public static int GetDataLen(byte[] buffer, ref int offset)
        {
            return buffer[offset++] << 24 |
              buffer[offset++] << 16 |
              buffer[offset++] << 8 |
              buffer[offset++];
        }
        #endregion
    }

    public class GameNetDataBuilder
    {
        public const int NetDataMsgId = 4;
        public const int NetErrorId = 4;
        public const int NetDataValid = 4;
        public const int NetDataLen = 4;


        public static uint GetDataValidValue(byte[] data,int offset =0)
        {
            uint value = 0;
            for(int i = offset; i < data.Length; i++)
            {
                value &= data[i];
            }
            return value;
        }

        public static bool ValidData(byte[] data ,int offset,uint dataValidValue)
        {
            uint value = GetDataValidValue(data, offset);
            return dataValidValue == value;
        }


        public static byte[] BuildGameNetData(uint msgId,uint ErroId,byte[] data)
        {
            uint dataValid = GetDataValidValue(data);
            uint realDataLen = (uint)(NetDataMsgId + NetErrorId + NetDataValid + data.Length);
            byte[] newData = new byte[NetDataLen + realDataLen];
            int offset = 0;

            SetUIntBytes(realDataLen, newData, offset, NetDataLen);
            offset += NetDataLen;
            SetUIntBytes(msgId, newData, offset, NetDataMsgId);
            offset += NetDataMsgId;
            SetUIntBytes(ErroId, newData, offset, NetErrorId);
            offset += NetErrorId;
            SetUIntBytes(dataValid, newData, offset, NetDataValid);
            offset += NetDataValid;
            Array.Copy(data, 0, newData, offset, data.Length);
            return newData;
        }

        public static bool ParseGameNetData(byte[] data ,out byte[] netData,out uint msgId,out uint ErrorId)
        {
            int offset = 0;
            msgId = GetUIntValue(data, offset, NetDataMsgId);
            offset += NetDataMsgId;
            ErrorId = GetUIntValue(data, offset, NetErrorId);
            offset += NetErrorId;
            uint DataValid = GetUIntValue(data, offset, NetDataValid);
            offset += NetDataValid;
            netData = new byte[data.Length - NetDataMsgId - NetErrorId - NetDataValid];
            Array.Copy(data, offset, netData, 0, data.Length - offset);
            if(ValidData(netData,0, DataValid))
            {
                return true;
            }
            return false;
        }

        #region 静态方法
        public static uint GetUIntValue(byte[] buffer, int offset,int len =4)
        {
            uint intValue = 0;
            for (int i = 0; i < len; i++)
            {
                intValue = (intValue | (uint)(buffer[offset + i] << (len - i - 1) * 8));
            }
            return intValue;
        }

        public static byte[] GetIntBytes(int value)
        {
            byte[] lenByte = new byte[4];
            for(int i = 0; i < 4; i++)
            {
                lenByte[i] = (byte)((value >> (4 - 1 - i) * 8) & 0xff);
            }
            return lenByte;
        }

        public static bool SetUIntBytes(uint value, byte[] buffer, int offset, int valueByteLen)
        {
            if (buffer.Length - offset < valueByteLen)
            {
                return false;
            }
            for (int i = 0; i < valueByteLen; i++)
            {
                buffer[offset + i] = (byte)((value >> (valueByteLen - 1 - i) * 8) & 0xff);
            }
            return true;
        }
        #endregion
    }
}

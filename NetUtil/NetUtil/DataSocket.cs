using System;
using System.Net;
using System.Net.Sockets;
using NetUtil.Log;
namespace NetUtil
{
    public class TestData
    {
        public string dataStr { get; set; }
    }

    public class DataSocket
    {
        public const int MaxRecvLen = 1024;

        Socket mSocket;
        NetDataCache mDataCache;


        byte[] recvBuffer = new byte[MaxRecvLen];
        NetData unCompleteData = null;
        int offset = 0;
        public event Action onSocketCloseHandler;

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

        public DataSocket(Socket socket,NetDataCache dataCache)
        {
            mSocket = socket;
            mDataCache = dataCache;
        }

        public bool StartRecv()
        {
            if (!SocketValid())
            {
                return false;
            }
            mSocket.BeginReceive(recvBuffer, offset,recvBuffer.Length,
                SocketFlags.None, OnRecv, mSocket);
            return true;
        }

        void OnRecv(IAsyncResult ar)
        {
            if (mSocket == null)
            {
                return;
            }

            try
            {
                Socket curSvr = mSocket;
                int curLen = curSvr.EndReceive(ar);
                LogTools.Log(string.Format("收到数据，长度:[{0}]", curLen));

                if (curLen < NetData.NetDataLen)
                {
                    LogTools.LogError("解析失败，数据长度不足解析报文长度");
                    return;
                }
                int dataOffset = 0;
                int lastLen = curLen - dataOffset;//剩余长度
                while (lastLen > 0) //如果还有数据
                {
                    if (unCompleteData == null)
                    {
                        if (lastLen < NetData.NetDataLen)//如果剩余长度不足以解析数据长度则跳出循环继续接收
                        {
                            break;
                        }
                        int dataLen = NetData.GetDataLen(recvBuffer, ref dataOffset);
                        lastLen = curLen - dataOffset;
                        bool curLenEnough = dataLen <= lastLen;
                        int realLen = curLenEnough ? dataLen : lastLen;
                        NetData data = new NetData(recvBuffer, dataOffset, realLen, dataLen, this);
                        dataOffset += realLen;
                        if (data.DataComplete())
                        {
                            mDataCache.EnqueueRecvData(data);
                        }
                        else
                        {
                            //如果数据不完整说明当前buffer的数据已经使用完，需要重新recv
                            unCompleteData = data;
                            break;
                        }
                    }
                    else
                    {
                        int unRecvLen = unCompleteData.GetUnRecvLen();
                        lastLen = curLen - dataOffset;
                        bool curLenEnough = unRecvLen <= lastLen;
                        int realLen = curLenEnough ? unRecvLen : lastLen;
                        if (unCompleteData.MergeData(recvBuffer, dataOffset, realLen))
                        {
                            mDataCache.EnqueueRecvData(unCompleteData);
                        }
                        dataOffset += realLen;
                    }
                    lastLen = curLen - dataOffset;
                }
                offset = curLen - dataOffset;
                if (offset > 0)
                {
                    Array.Copy(recvBuffer, dataOffset, recvBuffer, 0, offset);
                }
                Array.Clear(recvBuffer, offset, recvBuffer.Length - offset);
            }
            catch (Exception e)
            {
                LogTools.LogError(e.ToString());

                Destroy();
            }
            StartRecv();
        }




        byte[] sendBuffer = new byte[1024];
        int sendBufferOffset = 0;

        int LastLen
        {
            get
            {
                return sendBuffer.Length - sendBufferOffset;
            }
        }

        public bool Send(byte[] byteData)
        {
            if (!SocketValid())
            {
                return false;
            }

            if (LastLen < byteData.Length)
            {
                LogTools.LogError("SendFailed Too Long");
                return false;
            }
            Array.Copy(byteData, 0, sendBuffer, sendBufferOffset, byteData.Length);
            sendBufferOffset += byteData.Length;

            mSocket.BeginSend(sendBuffer, 0, sendBufferOffset, SocketFlags.None, OnSend, mSocket);
            return true;
        }


        void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket mSendSocket = mSocket;
                int curLen = mSendSocket.EndSend(ar);

                sendBufferOffset -= curLen;

                Array.Copy(sendBuffer, curLen, sendBuffer, 0, sendBufferOffset);
                Array.Clear(sendBuffer, sendBufferOffset, sendBuffer.Length - sendBufferOffset);

                if (sendBufferOffset <= 0)
                {
                    return;
                }
                mSendSocket.BeginSend(sendBuffer, 0, sendBufferOffset, SocketFlags.None, OnSend, mSendSocket);
            }
            catch (Exception e)
            {
                LogTools.LogError(e.ToString());
                Destroy();
            }
        }

        public void Destroy()
        {
            LogTools.Log("Close Socket");
            if (mSocket != null)
            {
                try
                {
                    mSocket.Shutdown(SocketShutdown.Both);
                    mSocket.Close();
                    mSocket = null;
                }
                catch
                {

                }
            }

            unCompleteData = null;
            if (onSocketCloseHandler != null)
            {
                onSocketCloseHandler.Invoke();
            }
        }

        bool SocketValid()
        {
            if(mSocket == null)
            {
                return false;
            }

            if (!mSocket.Connected)
            {
                return false;
            }
            return true;
        }
    }
}

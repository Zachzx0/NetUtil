using System.Collections.Generic;

namespace NetUtil
{
    public  class NetDataCache
    {
        Queue<NetData> recvDatas = new Queue<NetData>();
        Queue<NetData> sendDatas = new Queue<NetData>();

        public NetDataCache()
        {
            recvDatas.Clear();
            sendDatas.Clear();
        }

        public void Clear()
        {
            recvDatas.Clear();
            sendDatas.Clear();
        }


        public void EnqueueRecvData(NetData data)
        {
            recvDatas.Enqueue(data);
        }

        public void EnqueueSendData(NetData data)
        {
            recvDatas.Enqueue(data);
        }

        public NetData DequeueRecvData()
        {
            if (recvDatas.Count > 0)
            {
                return recvDatas.Dequeue();
            }
            return null;
        }

        public NetData DequeueSendData()
        {
            if (sendDatas.Count > 0)
            {
                return sendDatas.Dequeue();
            }
            return null;
        }

        public bool RecvDataHasValue()
        {
            return recvDatas.Count > 0;
        }

        public bool SendDataHasValue()
        {
            return sendDatas.Count > 0;
        }
    }
}

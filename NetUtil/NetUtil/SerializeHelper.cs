using Newtonsoft.Json;
using System;
using System.Text;

namespace NetUtil
{
    public class SerializeHelper
    {
        public static byte[] Serialize<T>(T tObj) where T : class
        {
            try
            {
                string objStr = JsonConvert.SerializeObject(tObj);
                return Encoding.UTF8.GetBytes(objStr);
            }
            catch(Exception e)
            {
                Log.LogTools.LogError(e.ToString());
            }
            return null;
        }

        public static T DeSerialize<T>(byte[] data) where T:class
        {
            try
            {
                string objStr = Encoding.UTF8.GetString(data);
                T obj = JsonConvert.DeserializeObject<T>(objStr);
                return obj;
            }
            catch (Exception e)
            {
                Log.LogTools.LogError(e.ToString());
            }
            return null;
        }

    }
}

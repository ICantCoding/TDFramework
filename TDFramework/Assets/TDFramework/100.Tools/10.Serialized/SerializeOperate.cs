
namespace TDFramework.Serialization
{   
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml.Serialization;
    using UnityEngine;

    public class SerializeOperate
    {
        //类对象序列化成Xml
        public static bool XmlSerialize(string path, System.Object obj)
        {
            bool ret = false;
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                if(null == fs) 
                    return false;
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                XmlSerializerNamespaces np = new XmlSerializerNamespaces();
                np.Add(String.Empty, String.Empty); 
                XmlSerializer xml = new XmlSerializer(obj.GetType());
                xml.Serialize(sw, obj, np);
                sw.Close();
                fs.Close();
                ret = true;
                Debug.Log(obj.GetType() + "类对象序列化成xml文件成功. 文件path: " + path);
            }
            catch(Exception exception)
            {
                Debug.LogError(obj.GetType() + "类对象序列化成xml文件失败. " + exception.Message);
            }
            return ret;
        }
        //Xml文件反序列化成类对象, 直接从编辑器模式下的相对xml文件路径读取, 该方法用在编辑器模式下
        public static T XmlDeserialize4Editor<T>(string path) where T : class
        {
            T t = default(T);
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                XmlSerializer xml = new XmlSerializer(typeof(T));
                t = xml.Deserialize(fs) as T;
                fs.Close();
                Debug.Log(path + "文件, 反序列化成功.");
            }
            catch(Exception exception)
            {
                Debug.LogError(path + "文件, 反序列化出现异常. " + exception.Message);
            }
            return t;
        }
        //非范型, Xml文件反序列化成类对象
        public static System.Object XmlDeserialize4Editor(string path, Type type)
        {
            System.Object obj = null;
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                XmlSerializer xml = new XmlSerializer(type);
                obj = xml.Deserialize(fs);
                fs.Close();
                Debug.Log(path + "文件, 反序列化成功.");
            }
            catch(Exception exception)
            {
                Debug.LogError(path + "文件, 反序列化出现异常. " + exception.Message);
            }
            return obj;
        }
        //Xml文件反序列化成类, xml文件是从资源中加载(Resources或Assetbundle), 该方法用在实际运行环境中
        public static T XmlDeserialize4Runtime<T>(string path) where T : class
        {
            T t = default(T);
            //目前从Resources中加载，其实最好应该从AssetBundle中加载, 如果是AssetBundle加载, 要注意记得卸载资源
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if(null == textAsset)
            {
                Debug.LogError(path + "反序列化成类对象失败. 没有找到该资源.");
                return null;
            }
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(textAsset.bytes);
                XmlSerializer xml = new XmlSerializer(typeof(T));
                t = xml.Deserialize(ms) as T;
                ms.Close();
                Debug.Log(path + "文件, 反序列化成功.");
            }
            catch(Exception exception)
            {
                Debug.LogError(path + "反序列化成类对象失败. " + exception.Message);
            }
            return t;
        }
        //类对象序列化成二进制
        public static bool BinarySerialize(string path, System.Object obj)
        {
            bool ret = false;
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, obj);
                fs.Close();
                ret = true;
                Debug.Log("类对象序列化成二进制文件成功.");
            }
            catch(Exception exception)
            {
                Debug.LogError(obj.GetType() + "类对象无法序列化成二进制." + exception.Message);
            }
            return ret;
        }
        //二进制文件反序列化为类对象
        public static T BinaryDeserialize<T>(string path) where T : class
        {
            T t = default(T);
            //目前从Resources中加载，其实最好应该从AssetBundle中加载, 如果是AssetBundle加载, 要注意记得卸载资源
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if(null == textAsset)
            {
                Debug.LogError(path + "反序列化成类对象失败. 没有找到该资源.");
                return null;
            }
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(textAsset.bytes);
                BinaryFormatter bf = new BinaryFormatter();
                t = bf.Deserialize(ms) as T;
                ms.Close();
                Debug.Log(path + "反序列化成类对象成功.");
            }
            catch(Exception exception)
            {
                Debug.LogError(path + "反序列化成类对象失败. " + exception.Message);
                return null;
            }
            return t;
        }
    }
}

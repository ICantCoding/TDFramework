
namespace TDFramework.Serialization
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class XmlDataMgr 
    {
        #region 字段
        //存储配置表
        private Dictionary<string, XmlDataBase> m_allXmlDataDict = new Dictionary<string, XmlDataBase>();
        #endregion

        #region 方法
        //加载配置表, path为二进制路径
        public T LoadXmlData<T>(string path) where T : XmlDataBase
        {
            if(string.IsNullOrEmpty(path)) 
                return null;
            if(m_allXmlDataDict.ContainsKey(path))
            {
                Debug.LogError("重复加载了相同的XmlData数据.");
                return m_allXmlDataDict[path] as T;
            }

            T xmlData = SerializeOperate.BinaryDeserialize<T>(path);
#if UNITY_EDITOR
            if(xmlData == null)
            {
                Debug.Log(path + "不存在, 从对应的Xml文件加载数据.");
                string xmlPath = path.Replace(".bytes", ".xml");
                xmlData = SerializeOperate.XmlDeserialize4Editor<T>(xmlPath);
            }
#endif
            if(null != xmlData)
            {
                xmlData.Init();
            }
            m_allXmlDataDict.Add(path, xmlData);
            return xmlData;
        }
        //获取配置表
        public T FindXmlData<T>(string path) where T : XmlDataBase
        {
            if(string.IsNullOrEmpty(path)) 
                return null;
            XmlDataBase xmlData = null;
            if(m_allXmlDataDict.TryGetValue(path, out xmlData) == false || xmlData == null)
            {
                xmlData = LoadXmlData<T>(path);
            }
            return xmlData as T;
        }
        #endregion
    }
}

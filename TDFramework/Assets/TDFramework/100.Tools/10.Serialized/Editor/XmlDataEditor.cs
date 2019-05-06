using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using OfficeOpenXml;

namespace TDFramework.Serialization
{

public class XmlDataEditor
{
    public static string XmlDataPath = "Assets/Config/Data/Xml/";
    public static string BinaryPath = "Assets/Config/Data/Binary/";
    public static string ScriptsPath = "Assets/Config/Data/Scripts/";

    [MenuItem("Assets/XmlData/类文件转Xml文件")]
    private static void AssetsClass2Xml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for(int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件夹下的类转成Xml", "正在扫描" + objs[i].name + "......", 
                (1.0f * i) / objs.Length);
            Class2Xml(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    private static void Class2Xml(string className)
    {
        if(string.IsNullOrEmpty(className)) 
            return;
        try{
            Type type = null;
            foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempTyep = asm.GetType(className);
                if(tempTyep != null)
                {
                    type = tempTyep;
                    break;
                }
            }
            if(type != null)
            {
                var temp = Activator.CreateInstance(type);
                if(temp is XmlDataBase)
                {
                    ((XmlDataBase)temp).Construction();
                }
                string pathStr = XmlDataPath + className + ".xml";
                bool status = TDFramework.Serialization.SerializeOperate.XmlSerialize(pathStr, temp);
                if(status)
                {
                    Debug.Log(className + "类转Xml成功.");
                }else
                {
                    Debug.LogError(className + "类转Xml失败.");
                }
            }
        }
        catch(Exception exception)
        {
            Debug.LogError("类转Xml失败. " + exception.Message);
        }
    }
    [MenuItem("Assets/XmlData/Xml文件转Binary文件")]
    private static void AssetsXml2Binary(string path)
    {
        UnityEngine.Object[] objs = Selection.objects;
        for(int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件夹下的Xml转成二进制文件", "正在扫描" + objs[i].name + "......", 
                (1.0f * i) / objs.Length);
            Xml2Binary(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    private static void Xml2Binary(string className)
    {
        if(string.IsNullOrEmpty(className)) 
            return;
        try
        {
            Type type = null;
            foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempTyep = asm.GetType(className);
                if(tempTyep != null)
                {
                    type = tempTyep;
                    break;
                }
            }
            if(type != null)
            {
                string xmlPath = XmlDataPath + className + ".xml";
                string binaryPath = BinaryPath + className + ".bytes";
                System.Object obj = SerializeOperate.XmlDeserialize4Editor(xmlPath, type);
                SerializeOperate.BinarySerialize(binaryPath, obj);
                Debug.Log("Xml转类，类再转二进制成功...");
            }
        }
        catch(Exception exception)
        {
            Debug.LogError("Xml转类，类再转二进制失败. " + exception.Message);
        }
    }
    [MenuItem("Tools/XmlData/XmlData文件全部转换成二进制文件")]
    private static void AllXml2Binary()
    {
        string path = Application.dataPath.Replace("Assets", "") + XmlDataPath;
        string[] fileAry = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        for(int i = 0; i < fileAry.Length; ++i)
        {
            EditorUtility.DisplayProgressBar("查找" + path + "文件夹下的Xml文件", "正在扫描" + fileAry[i] + "......", 
                (1.0f * i) / fileAry.Length);
            if(fileAry[i].EndsWith(".xml"))
            {
                string className = fileAry[i].Substring(fileAry[i].IndexOf('/') + 1);
                className.Replace(".xml", "");
                Xml2Binary(className);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/测试/1")]
    private static void AAAA()
    {
        TestInfo testInfo = new TestInfo()
        {
            Id = 1,
            Name = "ZhouShan",
            isA = false,
            strList = new List<string>(),
        };
        testInfo.strList.Add("AAAA");
        testInfo.strList.Add("BBBB");
        testInfo.strList.Add("CCCC");

        object obj = ReflectionTools.GetMemberValue(testInfo, "strList");
        List<string> list = (List<string>)obj;
        Debug.Log(list.Count);
        for(int i = 0; i < list.Count; i++)
        {
            Debug.Log(list[i]);
        }
    }
}
}


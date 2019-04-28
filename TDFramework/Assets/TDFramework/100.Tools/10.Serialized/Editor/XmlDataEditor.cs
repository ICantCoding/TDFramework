using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using TDFramework.Serialization;

public class XmlDataEditor
{
    public static string XmlDataPath = "Assets/Config/Data/Xml/";
    public static string BinaryPath = "Assets/Config/Data/Binary/";
    public static string ScriptsPath = "Assets/Config/Data/Scripts/";

    [MenuItem("Assets/XmlData/类转Xml")]
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
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;


public class ReflectionTools
{
    //反射创建类的实例
    public static object CreateInstance(string className)
    {
        if(string.IsNullOrEmpty(className)) 
            return null;
        object obj = null;
        Type type = null;
        foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(className);
            if(type != null)
                break;
        }
        if(type != null)
            obj = Activator.CreateInstance(type);
        return obj;
    }
    //根据反射，从某个实例对象中获取到该实例对象指定成员的值，感觉有必要吗，obj都拿到了，还要通过反射去拿值，多此一举吗？
    public static object GetMemberValue(object obj, string memberName, 
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
    {
        Type type = obj.GetType();
        MemberInfo[] memberInfos = type.GetMember(memberName, bindingFlags);
        if(memberInfos == null || memberInfos.Length == 0)
            return null;
        switch(memberInfos[0].MemberType)
        {
            case MemberTypes.Field:
                return type.GetField(memberName, bindingFlags).GetValue(obj);
            case MemberTypes.Property:
                return type.GetProperty(memberName, bindingFlags).GetValue(obj);
            default:
                return null;
        }
    }
    //根据反射，设置值
    public static void SetValue(PropertyInfo info, object var, string value, string type)
    {
        object val = (object)value;
        if(type == "int")
        {
            val = System.Convert.ToInt32(val);
        }
        else if(type == "bool")
        {
            val = System.Convert.ToBoolean(val);
        }
        else if(type == "float")
        {
            val = System.Convert.ToSingle(val);
        }
        else if(type == "enum")
        {
            val = TypeDescriptor.GetConverter(info.PropertyType).ConvertFromInvariantString(val.ToString());
        }
        info.SetValue(var, val);
    }
    //根据反射，创建List
    public static object CreateList(Type type)
    {
        Type listType = typeof(List<>);
        Type specType = listType.MakeGenericType(new System.Type[]{type});
        object list = Activator.CreateInstance(specType, new object[]{});
        return list;
    }
}

public class TestInfo
{
    public int Id;
    public string Name;
    public bool isA;
    public List<string> strList;
}

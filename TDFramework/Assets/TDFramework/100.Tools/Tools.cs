﻿namespace TDFramework {

    using System.Collections.Generic;
    using System.Collections;
    using System.IO;
    using System.Net.Sockets;
    using System.Net;
    using System;
    using UnityEngine;

    public class Tools {
        #region 查找子物体相关
        //查找子物体或组件
        public static T FindObject<T> (Transform parent, string name) where T : UnityEngine.Object {
            Transform obj = GetChild (parent, name);
            if (obj != null) {
                if (typeof (T).Equals (typeof (UnityEngine.GameObject)))
                    return obj.gameObject as T;
                if (typeof (T).Equals (typeof (UnityEngine.Transform)))
                    return obj as T;
                return obj.gameObject.GetComponent<T> ();
            }
            return null;
        }
        static Transform GetChild (Transform parent, string name) {
            if (parent.gameObject.name == name)
                return parent;
            for (int i = 0; i < parent.childCount; i++) {
                Transform curr = GetChild (parent.GetChild (i), name);
                if (curr != null && curr.gameObject.name == name)
                    return curr;
            }
            return null;
        }
        #endregion

        #region 销毁物体 相关
        public static void DestroyAllChildObject (Transform parent, float delayTime = 0.0f) {
            if (parent == null) return;
            Transform childTrans = null;
            while ((childTrans = parent.GetChild (0)) != null) {
                UnityEngine.GameObject.Destroy (childTrans.gameObject, delayTime);
            }
        }
        #endregion

        #region 单例相关
        //创建单例
        public static T GetInstance<T> (ref T instance, string name, bool isDontDestroy = true)
        where T : UnityEngine.Object {
            if (instance != null) return instance;

            if (GameObject.FindObjectOfType<T> () != null) {
                instance = GameObject.FindObjectOfType<T> ();
                return instance;
            }

            GameObject go = new GameObject (name, typeof (T));
            if (isDontDestroy)
                UnityEngine.Object.DontDestroyOnLoad (go);
            instance = go.GetComponent (typeof (T)) as T;
            return instance;
        }
        #endregion

        #region 平台路径 相关
        //各平台下路径
        public static string DeviceResPath () {
            switch (GameConfig.gamePlatform) {
                case GamePlatform.GamePlatform_Editor:
                    return string.Format ("{0}/", Application.dataPath);
                case GamePlatform.GamePlatform_PC:
                    return string.Format ("{0}/", Application.streamingAssetsPath);
                case GamePlatform.GamePlatform_Mobbile:
                    return string.Format ("{0}/", Application.persistentDataPath);
            }
            return string.Format ("{0}/", Application.dataPath);
        }
        #endregion

        #region 文件目录相关
        //获取文件目录下,所有的文件路径（出了.cs .meta .json .xml, 可添加）
        public static void Recursive (string path, ref List<string> list) {
            if (Directory.Exists (path)) {
                DirectoryInfo direction = new DirectoryInfo (path);
                FileInfo[] files = direction.GetFiles ("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++) {
                    if (files[i].Name.EndsWith (".cs") ||
                        files[i].Name.EndsWith (".meta") ||
                        files[i].Name.EndsWith (".json") ||
                        files[i].Name.EndsWith (".xml")) {
                        continue;
                    }
                    list.Add (files[i].FullName);
                }
            }
        }
        #endregion

        #region 获取本地局域网IP地址
        public static string GetLocalIpAddress_Fun1 () {
            try {
                string HostName = Dns.GetHostName ();
                IPHostEntry IpEntry = Dns.GetHostEntry (HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++) {
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork) {
                        return IpEntry.AddressList[i].ToString ();
                    }
                }
                return "127.0.0.1";
            } catch (Exception exception) {
                Debug.Log ("获取本地IP地址失败, Reason: " + exception.Message);
                return "127.0.0.1";
            }
        }
        public static string GetLocalIpAddress_Fun2 () {
            string localIP = "";
            string HostName = System.Net.Dns.GetHostName (); //得到主机名
            System.Net.IPHostEntry IpEntry = System.Net.Dns.GetHostEntry (HostName);
            for (int i = 0; i < IpEntry.AddressList.Length; i++) {
                if (IpEntry.AddressList[i].AddressFamily ==
                    System.Net.Sockets.AddressFamily.InterNetwork) {
                    localIP = IpEntry.AddressList[i].ToString ();
                    break;
                }
            }
            return localIP;
        }
        #endregion

        #region 角度计算
        //叫角度值转换在0-360度之间
        public static float ClampAngle (float angle, float min, float max) {
            do {
                if (angle < -360)
                    angle += 360;
                if (angle > 360)
                    angle -= 360;
            } while (angle < -360 || angle > 360);
            return Mathf.Clamp (angle, min, max);
        }
        #endregion
    }
}
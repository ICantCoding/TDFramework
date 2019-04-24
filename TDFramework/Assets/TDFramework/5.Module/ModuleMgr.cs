

//==================================================
/*
    模板集合管理类
*/
//==================================================

namespace TDFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class ModuleMgr : MonoSingleton<ModuleMgr>
    {
        #region 字段和属性
        private Dictionary<string, IModule> m_modules = new Dictionary<string, IModule>();
        #endregion

        #region 属性

        #endregion  

        #region  Unity生命周期
        void Awake()
        {
            //模块管理器不可销毁
            DontDestroyOnLoad(this.gameObject);
        }
        #endregion

        #region 数据管理方法
        public IModule RegisterModule(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) return null;
            if (m_modules.ContainsKey(moduleName) == false)
            {
                string moduleFullName = "TDFramework." + moduleName;
                Type type = Type.GetType(moduleFullName);
                IModule module = (IModule)Activator.CreateInstance(type);
                m_modules.Add(moduleName, module);
                module.Init();
                return module;
            }
            return null;
        }
        public void RemoveModule(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) return;
            IModule module = null;
            if (m_modules.TryGetValue(moduleName, out module) && module != null)
            {
                module.Release();
                m_modules.Remove(moduleName);
            }
        }
        public IModule GetModule(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) return null;
            IModule module = null;
            m_modules.TryGetValue(moduleName, out module);
            return module;
        }
        public T GetModule<T>(string moduleName) where T : IModule
        {
            IModule module = GetModule(moduleName);
            if(module == null) return null;
            return module as T;
        }
        #endregion
    }
}

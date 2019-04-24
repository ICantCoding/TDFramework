


//===============================================
/* 
    IModule 接口
*/
//===============================================

namespace TDFramework {

    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;

    //模块抽象类
    public abstract class IModule {
        #region 字段
        public string ModuleName;
        #endregion

        #region 抽象方法
        public abstract void Init (); //模板初始化接口方法
        public abstract void Release (); //模板释放接口方法
        #endregion
    }
}
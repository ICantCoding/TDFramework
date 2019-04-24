
//=========================================
/*
    普通单例的基类
*/
//=========================================

namespace TDFramework
{
    public class Singleton<T> where T : class, new()
    {
        #region 字段
        private static readonly object lockobj = new object();
        private static T m_instance = default(T);
        #endregion

        #region 静态方法
        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (lockobj)
                    {
                        if (m_instance == null)
                        {
                            m_instance = new T();
                        }
                    }
                }
                return m_instance;
            }
        }
        #endregion
    }
}

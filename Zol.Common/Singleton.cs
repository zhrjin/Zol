using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Zol.Common
{
    /// <summary>
    /// Singleton泛型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        protected Singleton() { Debug.Assert(null == Instance); }

        private readonly static T Instance = new T();

        /// <summary>
        /// 获取实例
        /// </summary> 
        public static T GetInstance()
        {
            return Instance;
        }

    }
}
﻿
namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class DictionaryExtension
    {
        public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key){
            TValue value;
            dict.TryGetValue(key, out value);
            return value;
        }
    }
}
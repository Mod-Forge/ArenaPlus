using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaSlugcatsConfigurator
{
    public static class CustomDataManager
    {
        public static readonly ConditionalWeakTable<object, CustomData> customData = new ConditionalWeakTable<object, CustomData>();
        public static T GetCustomData<T>(this object obj) where T : CustomData, new()
        {
            return (T)customData.GetValue(obj, _ => new T { owner = obj });
        }

    }
    public class CustomData
    {
        public object owner;
        public CustomData(object obj)
        {

            owner = obj;
        }
    }

    public class RoomCustomData : CustomData
    {
        public int frame;

        // Constructeur par défaut
        public RoomCustomData() : base(null) { }

        // Autres constructeurs si nécessaire
        public RoomCustomData(object obj) : base(obj) { }
    }
}

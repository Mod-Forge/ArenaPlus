using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArenaPlus.Features.UI;

namespace ArenaPlus.Utils
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

    public class PlayerCustomData : CustomData
    {
        public int customSpriteIndex;
        public bool initFinish = false;

        // Constructeur par défaut
        public PlayerCustomData() : base(null) { }

        // Autres constructeurs si nécessaire
        public PlayerCustomData(object obj) : base(obj) { }
    }

    public class PlayerResultBoxCustomData : CustomData
    {
        public VisualScrollButton scrollUpButton;
        public VisualScrollButton scrollDownButton;
        // Constructeur par défaut
        public PlayerResultBoxCustomData() : base(null) { }

        // Autres constructeurs si nécessaire
        public PlayerResultBoxCustomData(object obj) : base(obj) { }
    }
}

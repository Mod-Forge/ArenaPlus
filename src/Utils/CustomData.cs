using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArenaPlus.Features;
using ArenaPlus.Features.UI;
using ArenaPlus.Lib;
using Menu;

namespace ArenaPlus.Utils
{
    public static class CustomDataManager
    {
        public static readonly ConditionalWeakTable<object, object> customData = new ConditionalWeakTable<object, object>();
        public static T GetCustomData<T>(this object obj) where T : CustomData, new()
        {
            return (T)customData.GetValue(obj, _ => new T { owner = obj });
        }
    }

    public class CustomData
    {
        public object owner;
    }

    internal class RoomCustomData : CustomData
    {
        public HashSet<Spear> spearsRespawnExecption = new HashSet<Spear>();
    }


    internal class PhysicalObjectCustomData : CustomData
    {
        // attached features
        internal HashSet<AttachedFeature> attachedFeatures = new HashSet<AttachedFeature>();

    }

    internal class NootCustomData : CustomData
    {
    }

    internal class PlayerCustomData : CustomData
    {

        // SaintTongueThief
        public PlayerCarryableItem tongueGrabbedItem;
    }

    internal class PlayerResultBoxCustomData : CustomData
    {
        public VisualScrollButton scrollUpButton;
        public VisualScrollButton scrollDownButton;
    }

    internal class MultiplayerMenuData : CustomData
    {
        public SymbolButton[] nextClassButtons;
        public SymbolButton[] previousClassButtons;
        public SimpleButton[] levelChangeButtons;
    }
}

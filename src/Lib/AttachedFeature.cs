using ArenaPlus.Utils;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Lib
{
    public static class AttachedFeatureUtils
    {
        public static bool AddAttachedFeature(this PhysicalObject obj, AttachedFeature feature)
        {
            if (obj.room == null) return false;
            CreatureCustomData playerData = obj.GetCustomData<CreatureCustomData>();
            bool succes = playerData.attachedFeatures.Add(feature);
            if (succes)
            {
                obj.room.AddObject(feature);
            }
            return succes;
        }

        public static bool RemoveAttachedFeature(this PhysicalObject obj, AttachedFeature feature)
        {
            CreatureCustomData playerData = obj.GetCustomData<CreatureCustomData>();
            if (playerData.attachedFeatures.Contains(feature))
            {
                playerData.attachedFeatures.Remove(feature);
                if (!feature.slatedForDeletetion)
                {
                    feature.Destroy();
                }
                return true;
            }
            return false;
        }

        public static T GetAttachedFeatureType<T>(this PhysicalObject obj) where T : AttachedFeature
        {
            CreatureCustomData playerData = obj.GetCustomData<CreatureCustomData>();
            return (playerData.attachedFeatures.Any(f => f is T) ? playerData.attachedFeatures.First(f => f is T) : null) as T;
        }

        public static bool HasAttachedFeatureType<T>(this PhysicalObject obj)
        {
            CreatureCustomData playerData = obj.GetCustomData<CreatureCustomData>();
            return playerData.attachedFeatures.Any(f => f is T);
        }
    }

    public class AttachedFeature(PhysicalObject obj) : UpdatableAndDeletable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006")]
        public PhysicalObject owner => obj;

        public override void Destroy()
        {
            base.Destroy();
            obj.RemoveAttachedFeature(this);
        }
    }

    public class PlayerAttachedFeature(Player obj) : AttachedFeature(obj)
    {
        public new Player owner => obj;
        public Player player => owner;
    }
}

using ArenaPlus.Utils;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Lib
{
    public static class PlayerAttachedFeatureUtils
    {
        public static bool AddAttachedFeature(this Player player, PlayerAttachedFeature feature)
        {
            if (player.room == null) return false;
            PlayerCustomData playerData = player.GetCustomData<PlayerCustomData>();
            bool succes = playerData.attachedFeatures.Add(feature);
            if (succes)
            {
                player.room.AddObject(feature);
            }
            return succes;
        }

        public static bool RemoveAttachedFeature(this Player player, PlayerAttachedFeature feature)
        {
            PlayerCustomData playerData = player.GetCustomData<PlayerCustomData>();
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

        public static T GetAttachedFeatureType<T>(this Player player) where T : PlayerAttachedFeature
        {
            PlayerCustomData playerData = player.GetCustomData<PlayerCustomData>();
            return (playerData.attachedFeatures.Any(f => f is T) ? playerData.attachedFeatures.First(f => f is T) : null) as T;
        }

        public static bool HasAttachedFeatureType<T>(this Player player)
        {
            PlayerCustomData playerData = player.GetCustomData<PlayerCustomData>();
            return playerData.attachedFeatures.Any(f => f is T);
        }
    }

    public class PlayerAttachedFeature(Player player) : UpdatableAndDeletable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006")]
        public Player owner => player;

        public override void Destroy()
        {
            base.Destroy();
            player.RemoveAttachedFeature(this);
        }
    }
}

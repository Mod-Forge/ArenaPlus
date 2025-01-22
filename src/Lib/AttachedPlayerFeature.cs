using ArenaPlus.Utils;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Lib
{
    public static class AttachedPlayerFeatureUtils
    {
        public static bool AddAttachedFeature(this Player player, AttachedPlayerFeature feature)
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

        public static bool RemoveAttachedFeature(this Player player, AttachedPlayerFeature feature)
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

        public static T GetAttachedFeatureType<T>(this Player player) where T : AttachedPlayerFeature
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

    public class AttachedPlayerFeature : UpdatableAndDeletable
    {
        public readonly Player player;
        public Player owner => this.player;
        public AttachedPlayerFeature(Player player)
        {
            this.player = player;
        }

        public override void Destroy()
        {
            base.Destroy();
            player.RemoveAttachedFeature(this);
        }
    }
}

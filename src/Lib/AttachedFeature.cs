using ArenaPlus.Utils;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                feature.owner = obj;
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

    public class AttachedFeature() : UpdatableAndDeletable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006")]
        public PhysicalObject owner;

        public override void Destroy()
        {
            base.Destroy();
            owner?.RemoveAttachedFeature(this);
        }
    }

    public class PlayerAttachedFeature() : AttachedFeature()
    {
        public Player player => owner as Player;
    }

    public class PlayerCosmeticFeature() : PlayerAttachedFeature(), IDrawable
    {
        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || this.room != rCam.room))
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public virtual void PausedDrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || this.room != rCam.room))
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Midground");
            }
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }
    }
}

using ArenaPlus.Lib;
using ArenaPlus.Utils;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Slugcats
{
    [SlugcatFeatureInfo(
        id: "saintTongueThief",
        name: "Saint tongue thief",
        description: "Allow the Saint tongue to steal weapons",
        slugcat: "Saint"
    )]
    file class SaintTongueThief(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Unregister()
        {
            On.Player.Tongue.Shoot -= Tongue_Shoot;
            On.Player.Tongue.AutoAim -= Tongue_AutoAim;
            On.Player.Tongue.Update -= Tongue_Update;
            On.Player.Tongue.AttachToChunk -= Tongue_AttachToChunk;
        }

        protected override void Register()
        {
            On.Player.Tongue.Shoot += Tongue_Shoot;
            On.Player.Tongue.AutoAim += Tongue_AutoAim;
            On.Player.Tongue.Update += Tongue_Update;
            On.Player.Tongue.AttachToChunk += Tongue_AttachToChunk;
        }

        private void Tongue_AttachToChunk(On.Player.Tongue.orig_AttachToChunk orig, Player.Tongue self, BodyChunk chunk)
        {
            orig(self, chunk);

            if (GameUtils.IsCompetitiveOrSandboxSession && self.mode == Player.Tongue.Mode.AttachedToObject && self.attachedChunk.owner is Creature creautre && creautre.grasps != null && creautre.grasps.Length > 0 && creautre.grasps.Any(g => g != null && g.grabbed is Weapon))
            {
                Weapon weapon = creautre.grasps.First(g => g != null && g.grabbed is Weapon).grabbed as Weapon;
                if (weapon != null)
                {
                    if (weapon is Spear spear && (spear.mode == Weapon.Mode.StuckInWall || spear.mode == Weapon.Mode.StuckInCreature))
                    {
                        spear.resetHorizontalBeamState();
                        spear.PulledOutOfStuckObject();
                    }
                    weapon.AllGraspsLetGoOfThisObject(true);
                    weapon.ChangeMode(Weapon.Mode.Free);
                    self.attachedChunk = weapon.firstChunk;
                    self.pos = weapon.firstChunk.pos;
                }
            }
        }

        private void Tongue_Update(On.Player.Tongue.orig_Update orig, Player.Tongue self)
        {
            if (GameUtils.IsCompetitiveOrSandboxSession && self.mode == Player.Tongue.Mode.AttachedToObject && self.attachedChunk.owner.TotalMass < 0.2f)
            {
                BodyChunk attachedChunk = self.attachedChunk;
                self.Release();
                attachedChunk.vel = Custom.DirVec(attachedChunk.pos, self.baseChunk.pos) * 10;
                if (self.player.FreeHand() != -1 && self.player.Grabability(attachedChunk.owner) != Player.ObjectGrabability.CantGrab)
                {
                    self.player.SlugcatGrab(attachedChunk.owner, self.player.FreeHand());
                }
                self.player.wantToJump = 0;
            }
            orig(self);
        }

        private Vector2 Tongue_AutoAim(On.Player.Tongue.orig_AutoAim orig, Player.Tongue self, Vector2 originalDir)
        {
            if (GameUtils.IsCompetitiveOrSandboxSession && self.player.input[0].y <= 0)
            {
                float d = 230f;
                Vector2 endPos = self.baseChunk.pos + originalDir * d;
                var result = SharedPhysics.TraceProjectileAgainstBodyChunks(null, self.player.room, self.baseChunk.pos, ref endPos, 50f, 1, self.player, false);
                if (result.hitSomething)
                {
                    return Custom.DirVec(self.baseChunk.pos + self.baseChunk.vel, result.chunk.pos + result.chunk.vel).normalized;
                }
            }
            return orig(self, originalDir);
        }

        private void Tongue_Shoot(On.Player.Tongue.orig_Shoot orig, Player.Tongue self, Vector2 dir)
        {
            if (!GameUtils.IsCompetitiveOrSandboxSession)
            {
                orig(self, dir);
                return;
            }
            dir = new Vector2(self.player.input[0].x, self.player.input[0].y > 0 ? 1 : 0).normalized;
            if (dir.magnitude == 0)
            {
                dir = new Vector2(self.player.ThrowDirection, 0);
            }

            dir = (dir + self.player.mainBodyChunk.vel.normalized * (dir.y != 0 ? 0.2f : 0.05f)).normalized;
            orig(self, dir);
        }
    }
}
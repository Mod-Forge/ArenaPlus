using ArenaPlus.Lib;
using ArenaPlus.Utils;
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
        id: "spearMasterCustomSpears",
        name: "Spear Master random spears",
        description: "Whether the Artificer is nerfed in arena",
        slugcat: "Spearmaster"
    )]
    file class SpearMasterCustomSpears(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Unregister()
        {
        }

        protected override void Register()
        {
            On.AbstractSpear.ctor_World_Spear_WorldCoordinate_EntityID_bool += AbstractSpear_ctor_World_Spear_WorldCoordinate_EntityID_bool;
        }

        private void AbstractSpear_ctor_World_Spear_WorldCoordinate_EntityID_bool(On.AbstractSpear.orig_ctor_World_Spear_WorldCoordinate_EntityID_bool orig, AbstractSpear self, World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, bool explosive)
        {
            if (GameUtils.IsCompetitiveOrSandboxSession && !explosive && world?.game?.AlivePlayers != null)
            {
                foreach (var absPlayer in world.game.AlivePlayers)
                {
                    if (absPlayer.realizedCreature != null) LogDebug(absPlayer.realizedCreature.room.GetWorldCoordinate(absPlayer.realizedCreature.mainBodyChunk.pos), pos);
                    Player player = absPlayer.realizedCreature as Player;
                    bool needleCheck = player.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear && player.input[0].pckp && (player.grasps[0] == null || player.grasps[1] == null) && player.input[0].y == 0 && (player.graphicsModule as PlayerGraphics) != null && (player.graphicsModule as PlayerGraphics).tailSpecks.spearProg == 0f;
                    
                    LogDebug("spawn check", absPlayer.realizedCreature.room.GetWorldCoordinate(absPlayer.realizedCreature.mainBodyChunk.pos) == pos, player.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear, needleCheck);
                    if (player != null && absPlayer.realizedCreature.room.GetWorldCoordinate(absPlayer.realizedCreature.mainBodyChunk.pos) == pos && needleCheck)
                    {
                        LogDebug("needle created in arena");
                        if (Random.value < 0.25f)
                        {
                            LogDebug("spawning custom spear");
                            int spearType = Random.Range(0, 3);
                            switch (spearType)
                            {
                                case 0:
                                    explosive = true;
                                    break;
                                case 1:
                                    self.electric = true;
                                    self.electricCharge = 1;
                                    break;
                                case 2:
                                    self.hue = Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f));
                                break;
                            }
                        }
                    }
                }
            }
            orig(self, world, realizedObject, pos,ID, explosive);
        }
    }
}
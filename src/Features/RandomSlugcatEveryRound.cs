using ArenaPlus.Lib;
using ArenaPlus.Utils;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "randomSlugcatEveryRound",
        name: "Random slugcat every round",
        description: "Make the game choose a random slugcat every round when selecting random icon",
        enabledByDefault: false
    )]
    internal class RandomSlugcatEveryRound(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
            On.ArenaSitting.SessionEnded += ArenaSitting_SessionEnded;
            On.Menu.PlayerResultBox.ctor += PlayerResultBox_ctor;
        }

        protected override void Unregister()
        {
            On.ArenaSitting.SessionEnded -= ArenaSitting_SessionEnded;
            On.Menu.PlayerResultBox.ctor -= PlayerResultBox_ctor;
        }

        private void PlayerResultBox_ctor(On.Menu.PlayerResultBox.orig_ctor orig, Menu.PlayerResultBox self, Menu.Menu menu, Menu.MenuObject owner, UnityEngine.Vector2 pos, UnityEngine.Vector2 size, ArenaSitting.ArenaPlayer player, int index)
        {
            orig(self, menu, owner, pos, size, player, index);
            if (self is FinalResultbox && GameUtils.RainWorldInstance.processManager.arenaSetup.playerClass[player.playerNumber] == null)
            {
                self.subObjects.Remove(self.portrait);
                self.portrait = new MenuIllustration(menu, self, "", string.Concat([
                    "MultiplayerPortrait",
                    self.player.playerNumber.ToString(),
                    "2"
                ]), new Vector2(size.y / 2f, size.y / 2f), true, true);
                self.subObjects.Add(self.portrait);
            }
        }

        private void ArenaSitting_SessionEnded(On.ArenaSitting.orig_SessionEnded orig, ArenaSitting self, ArenaGameSession session)
        {
            orig(self, session);
            ArenaSetup arenaSetup = session.room.game.rainWorld.processManager.arenaSetup;
            for (int i = 0; i < RainWorld.PlayerObjectBodyColors.Length; i++)
            {
                if (arenaSetup.playersJoined[i] && arenaSetup.playerClass[i] == null)
                {
                    self.players.Find(player => player.playerNumber == i).playerClass = SlugcatsUtils.GetRandomSlugcat();
                }
            }
        }

    }
}

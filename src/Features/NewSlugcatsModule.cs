using ArenaPlus.Lib;
using ArenaPlus.Utils;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [ImmutableFeature]
    file class NewSlugcatsModule : ImmutableFeature
    {
        public static bool originalNewSlugcatsModuleState = false;
        protected override void Register()
        {
            // activate the module for new slugcats
            // cause my game to DIEEEEEE!!!
            // I may fix it someday, maybe...
            //new Hook(typeof(ModManager).GetProperty(nameof(ModManager.NewSlugcatsModule)).GetGetMethod(), (Func<bool> orig) => { originalNewSlugcatsModuleState = orig(); return true; });
        }
    }
}

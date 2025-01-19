using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.PlayerLoop;

namespace ArenaPlus.Options.Elements
{
    internal class OpCustomTab(OptionInterface owner, string name = "") : OpTab(owner, name)
    {
        internal virtual void Update() { }
    }
}

using ArenaPlus.Options;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ArenaPlus.Utils.HookRegister;
using ArenaPlus.Options.Tabs;
using ArenaPlus.Options.Elements;

namespace ArenaPlus.Lib
{
    public class FeatureInfoAttribute(string id, string name, string description, bool enabledByDefault, string category = "General", string[] requires = null, string[] incompatibilities = null, string color = "None", string[] requireDLC = null) : BaseFeatureInfoAttribute(id, name, description, enabledByDefault)
    {
        public string category = category;
        public string color = color;
        public string[] requires = requires;
        public string[] incompatibilities = incompatibilities;
        public string[] requireDLC = requireDLC;
    }

    public abstract class Feature(FeatureInfoAttribute featureInfo) : BaseFeature(featureInfo)
    {
        public bool EnabledByDefault { get; } = featureInfo.enabledByDefault;
        public string Category { get; } = featureInfo.category;
        public string[] Requires { get; } = featureInfo.requires;
        public string[] Incompatibilities { get; } = featureInfo.incompatibilities;
        public string HexColor { get; } = featureInfo.color;
        public string[] RequireDLC = featureInfo.requireDLC;


        internal Action<OpExpandable, Vector2> complementaryElementAction;

        internal void SetComplementaryElement(Action<OpExpandable, Vector2> func)
        {
            complementaryElementAction = func;
        }
    }
}
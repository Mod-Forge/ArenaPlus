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
    public class FeatureInfoAttribute(string id, string name, string description, bool enabledByDefault, string category = "General", string color = "None") : BaseFeatureInfoAttribute(id, name, description, enabledByDefault)
    {
        public string category = category;
        public string color = color;
    }

    public abstract class Feature(FeatureInfoAttribute featureInfo) : BaseFeature(featureInfo)
    {
        public bool EnabledByDefault { get; } = featureInfo.enabledByDefault;
        public string Category { get; } = featureInfo.category;
        public string HexColor { get; } = featureInfo.color;

        internal Action<Feature, OpExpandable, Vector2> complementaryElementAction;

        internal void SetComplementaryElement(Action<Feature, OpExpandable, Vector2> func)
        {
            complementaryElementAction = func;
        }
    }
}
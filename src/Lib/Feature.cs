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

namespace ArenaPlus.Lib
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FeatureInfoAttribute(string id, string name, string description, bool enabledByDefault, string category = "General", string color = "None") : Attribute
    {
        public string id = id;
        public string name = name;
        public string description = description;
        public bool enabledByDefault = enabledByDefault;
        public string category = category;
        public string color = color;
    }

    public abstract class Feature(FeatureInfoAttribute featureInfo)
    {
        public string Id { get; } = featureInfo.id;
        public string Name { get; } = featureInfo.name;
        public string Description { get; } = featureInfo.description;
        public bool EnabledByDefault { get; } = featureInfo.enabledByDefault;
        public string Category { get; } = featureInfo.category;
        public string HexColor { get; } = featureInfo.color;

        internal bool registered = false;

        public void Enable()
        {
            if (registered) return;

            configurable.Value = true;
            registered = true;

            Log("Register");
            Register();
        }

        public void Disable()
        {
            if (!registered) return;

            configurable.Value = false;
            registered = false;
            Log("Unregister");
            Unregister();
        }

        protected abstract void Register();
        protected abstract void Unregister();

        public Configurable<bool> configurable = OptionsInterface.instance.config.Bind(featureInfo.id, featureInfo.enabledByDefault, new ConfigurableInfo(featureInfo.description, null, "", []));

        public bool Enabled => configurable.Value;

        public static void AddFeature(Feature feature)
        {

        }

        public static void LoadFeatures()
        {
            Configurable_OnChange(null);
        }

        private static void Configurable_OnChange(Feature feature)
        {

        }
    }
}
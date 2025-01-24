using ArenaPlus.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Lib
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BaseFeatureInfoAttribute(string id, string name, string description, bool enabledByDefault) : Attribute
    {
        public string id = id;
        public string name = name;
        public string description = description;
        public bool enabledByDefault = enabledByDefault;
    }

    public abstract class BaseFeature(BaseFeatureInfoAttribute featureInfo)
    {
        public string Id { get; } = featureInfo.id;
        public string Name { get; } = featureInfo.name;
        public string Description { get; } = featureInfo.description;

        internal bool registered = false;

        public void Enable()
        {
            if (registered) return;

            configurable.Value = true;
            registered = true;

            LogInfo($"Enabling {Id}");
            Register();
        }

        public void Disable()
        {
            if (!registered) return;

            configurable.Value = false;
            registered = false;
            LogInfo($"Disabling {Id}");
            Unregister();
        }

        protected abstract void Register();
        protected abstract void Unregister();

        public Configurable<bool> configurable = OptionsInterface.instance.config.Bind(featureInfo.id, featureInfo.enabledByDefault, new ConfigurableInfo(featureInfo.description, null, "", []));

        public bool Enabled => configurable.Value;
    }
}

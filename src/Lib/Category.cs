using ArenaPlus.Options;
using ArenaPlus.Utils;
using IL.Menu.Remix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Lib
{
    public class Category(string name, List<Feature> features = null)
    {
        internal static string[] renderOrder = [
            "General",
            "Reworks",
            "Spoilers"
        ];

        public readonly string name = name;
        public readonly List<Feature> features = features ?? [];
        public Configurable<bool> configurable = OptionsInterface.instance.config.Bind(null, false, new ConfigurableInfo($"Enable {name}", null, "", []));

        public void AddFeature(Feature feature)
        {
            features.Add(feature);
        }

        internal static void Preload()
        {
            foreach (var name in renderOrder)
            {
                FeaturesManager.categories.Add(
                    new Category(name)
                );
            }
        }
    }

    internal class BuiltInCategory
    {
        internal const string General = "General";
        internal const string Reworks = "Reworks";
        internal const string Spoilers = "Spoilers";
    }
}

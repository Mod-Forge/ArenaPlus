using ArenaPlus.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Lib
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SlugcatFeatureInfoAttribute : Attribute
    {
        public string id;
        public string name;
        public string description;
        public string slugcat;

        public SlugcatFeatureInfoAttribute(string id, string name, string description, string slugcat)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.slugcat = slugcat;
        }
    }

    public class SlugcatFeature(SlugcatFeatureInfoAttribute featureInfo)
    {
        public string Id { get; } = featureInfo.id;
        public string Name { get; } = featureInfo.name;
        public string Description { get; } = featureInfo.description;
        public string Slugcat { get; } = featureInfo.slugcat;

        public Configurable<bool> configurable = OptionsInterface.instance.config.Bind(featureInfo.id, false, new ConfigurableInfo(featureInfo.description, null, "", []));
    }
}

using ArenaPlus.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Lib
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SlugcatFeatureInfoAttribute(string id, string name, string description, string slugcat) : BaseFeatureInfoAttribute(id, name, description, false)
    {
        public string slugcat = slugcat;
    }

    public abstract class SlugcatFeature(SlugcatFeatureInfoAttribute featureInfo) : BaseFeature(featureInfo)
    {
        public string Slugcat { get; } = featureInfo.slugcat;
    }
}

using ArenaPlus.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Features.Slugcats
{
    [SlugcatFeatureInfo(
        id: "placeholder",
        name: "Placeholder",
        description: "Placeholder",
        slugcat: "Rivulet"
    )]
    internal class Test(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Register()
        {
        }

        protected override void Unregister()
        {
        }
    }
}

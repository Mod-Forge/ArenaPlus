using ArenaPlus.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Features.Slugcats
{
    [SlugcatFeatureInfo(
        id: "saintSpear",
        name: "Saint can use spears",
        description: "Whether the Saint can use spears",
        slugcat: "Saint"
    )]
    internal class SaintUseSpears(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Register()
        {
            throw new NotImplementedException("TODO: Implement that");
        }

        protected override void Unregister()
        {
        }
    }
}
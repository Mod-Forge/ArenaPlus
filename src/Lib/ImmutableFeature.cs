using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Lib
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ImmutableFeatureAttribute() : Attribute
    {
    }

    public abstract class ImmutableFeature
    {
        protected ImmutableFeature()
        {
            Register();
        }

        protected abstract void Register();
    }
}

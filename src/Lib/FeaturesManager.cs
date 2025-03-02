﻿using ArenaPlus.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SlugBase.Features;
using Newtonsoft.Json;

namespace ArenaPlus.Lib
{
    public static class FeaturesManager
    {
        internal static List<Category> categories = [];
        internal static List<SlugcatFeature> slugcatFeatures = [];

        internal static void AddFeature(Feature feature)
        {
            Category category = categories.Find(c => c.name.ToLower() == feature.Category.ToLower());
            if (category == null)
            {
                category = new(feature.Category, []);
                categories.Add(category);
            }
            category.AddFeature(feature);
        }

        internal static void LoadFeatures()
        {
            MachineConnector.ReloadConfig(OptionsInterface.instance);

            Category.Preload();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                foreach (Type type in types.Where(t => t != null))
                {
                    try
                    {
                        BaseFeature baseFeature = null;
                        if (type.GetCustomAttribute<FeatureInfoAttribute>() is FeatureInfoAttribute featureInfo)
                        {
                            Assert(type.GetConstructors().Length > 0, $"Missing constructor in feature {featureInfo.name}");
                            Feature feature = type.GetConstructors()[0].Invoke([featureInfo]) as Feature;

                            baseFeature = feature;

                            AddFeature(feature);
                        }
                        else if (type.GetCustomAttribute<ImmutableFeatureAttribute>() is not null)
                        {
                            Assert(type.GetConstructors().Length > 0, message: $"Missing constructor in immutable feature {nameof(type)}");
                            type.GetConstructors()[0].Invoke([]);
                        }
                        else if (type.GetCustomAttribute<SlugcatFeatureInfoAttribute>() is SlugcatFeatureInfoAttribute slugcatFeatureInfo)
                        {
                            Assert(type.GetConstructors().Length > 0, message: $"Missing constructor in slugcat feature {slugcatFeatureInfo.name}");
                            SlugcatFeature feature = type.GetConstructors()[0].Invoke([slugcatFeatureInfo]) as SlugcatFeature;

                            baseFeature = feature;

                            slugcatFeatures.Add(feature);
                        }

                        if (baseFeature != null)
                        {
                            LogInfo($"Registering feature : {baseFeature.Id}");

                            if (baseFeature.configurable.Value)
                            {
                                baseFeature.Enable();
                            }

                            baseFeature.configurable.OnChange += () => Configurable_OnChange(baseFeature);
                        }
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                    }
                }
            }
        }

        private static void Configurable_OnChange(BaseFeature feature)
        {
            if (feature.configurable.Value && !feature.registered)
            {
                feature.Enable();
            }
            else if (!feature.configurable.Value && feature.registered)
            {
                feature.Disable();
            }
        }

        public static bool TryGetFeature(string id, out Feature feature)
        {
            feature = null;
            foreach (var category in categories)
            {
                foreach (var f in category.features)
                {
                    if (f.Id == id)
                    {
                        feature = f;
                        return true;
                    }
                }
            }
            return false;
        }

        public static Feature GetFeature(string id)
        {
            foreach (var category in categories)
            {
                foreach (var feature in category.features)
                {
                    if (feature.Id == id) return feature;
                }
            }
            throw new Exception($"{id} feature does not exist");
        }
    }
}

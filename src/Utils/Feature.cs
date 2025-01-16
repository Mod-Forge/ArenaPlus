﻿using ArenaPlus.Options;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ArenaPlus.Utils.HookRegister;

namespace ArenaPlus.Utils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FeatureInfoAttribute(string id, string name, string description, bool enabledByDefault) : Attribute
    {
        public string id = id;
        public string name = name;
        public string description = description;
        public bool enabledByDefault = enabledByDefault;
    }

    public abstract class Feature(FeatureInfoAttribute featureInfo)
    {
        public string Id { get; } = featureInfo.id;
        public string Name { get; } = featureInfo.name;
        public string Description { get; } = featureInfo.description;
        public bool EnabledByDefault { get; } = featureInfo.enabledByDefault;

        public static List<Feature> features = [];

        public static void LoadFeatures()
        {
            MachineConnector.ReloadConfig(OptionsInterface.instance);

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
                    if (type.GetCustomAttribute<FeatureInfoAttribute>() is FeatureInfoAttribute featureInfo)
                    {
                        Log($"Registering feature : {featureInfo.id}");

                        Feature feature = type.GetConstructors()[0].Invoke([featureInfo]) as Feature;

                        if (feature.configurable.Value)
                        {
                            feature.Enable();
                        }

                        feature.configurable.OnChange += () => Configurable_OnChange(feature);

                        features.Add(feature);
                    } else if (type.GetCustomAttribute<ImmutableFeatureAttribute>() is not null)
                    {
                        type.GetConstructors()[0].Invoke([]);
                    }
                }
            }
        }

        private static void Configurable_OnChange(Feature feature)
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

        private bool registered = false;

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
    }

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
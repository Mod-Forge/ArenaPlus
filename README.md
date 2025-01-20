# Arena+
### Welcome to the Arena+ repository

## Using the API to add features from your mod
1. Add the dll to your mod project libraries
2. Create a new file (probably named after your feature's name)
3. Paste one of [those](#feature-types-templates) class in the file

## Feature types templates
### Feature
```cs
[FeatureInfo(
    id: "yourFeatureId",
    name: "Your feature name (displayed in the features tab)",
    description: "Your feature description",
    category: "YourModName (or something like that)",
    enabledByDefault: false
)]
file class YourFeature(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
{
    // Called when the feature is enabled or if enabled at launch
    protected override void Register()
    {
        // Register your hooks here
        // On.Spear.HitSomething += Spear_HitSomething;
    }

    // Called when the feature is disabled
    protected override void Unregister()
    {
        // Unregister your hooks here
        // Ex: On.Spear.HitSomething -= Spear_HitSomething;
    }
}
```

### Slugcat feature
```cs
[SlugcatFeatureInfo(
    id: "yourFeatureId",
    name: "Your feature name (displayed below the slugcat checkox in the slugcats tab)",
    description: "Your feature description",
    slugcat: "The slugcat name (Ex: Hunter / Rivulet / Saint / SomeCustomSlugcat)"
)]
file class HunterPickupStuckSpears(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
{
    // Same as Feature.Register
    protected override void Register()
    {
    }

    // Same as Feature.Unregister
    protected override void Unregister()
    {
    }
}
```

### Immutable feature (always on feature)
```cs
[ImmutableFeature]
file class HunterScar : ImmutableFeature
{
    // Called when the feature is registered, during the mods init phase
    protected override void Register()
    {
        // Register your hooks here
        // On.Menu.PlayerResultBox.ctor += PlayerResultBox_ctor;
    }
}
```

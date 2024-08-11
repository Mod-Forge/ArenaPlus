
namespace ArenaSlugcatsConfigurator.Freatures
{
    internal static class FeaturesManager
    {
        internal static void Register()
        {
            logSource.LogInfo("FreaturesManager Register");
            KarmaFlowerBuff.Register();
            ElectroBoom.Register();
            EnergyCellBuff.Register();
            FlareBombBuff.Register();
            PlayerOneshot.Register();
            ScavengerUseGun.Register();
            ResultMenuSlugcatSelection.Register();
        }
    }
}

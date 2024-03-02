
namespace ArenaSlugcatsConfigurator.Freatures
{
    internal static class FreaturesManager
    {
        internal static void OnEnable()
        {
            logSource.LogInfo("FreaturesManager OnEnable");
            KarmaFlowerBuff.OnEnable();
            ElectroBoom.OnEnable();
            EnergyCellBuff.OnEnable();
            FlareBombBuff.OnEnable();
        }
    }
}

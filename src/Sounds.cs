namespace ArenaPlus
{
    public static class Sounds
    {
        public static SoundID Noot_Warp { get; private set; }
        internal static void Initialize()
        {
            Noot_Warp = new SoundID("Noot_Warp", true);

        }
    }
}

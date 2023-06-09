namespace AElf.Contracts.Timelock
{
    public static class TimelockContractConstants
    {
        public const string SYMBOL = "ELF";
        // 1 min
        public const long MIN_DELAY = 60;
        // 1 hour
        public const long MAX_DELAY = 3600;
        // 30 min
        public const long GRACE_PERIOD = 1800;
    }
}
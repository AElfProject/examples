namespace AElf.Contracts.Timelock
{
    public static class TimelockContractConstants
    {
        public const string SYMBOL = "ELF";
        // unit is second
        public const long MIN_DELAY = 1 * 24 * 60 * 60;
        public const long MAX_DELAY = 7 * 24 * 60 * 60;
        public const long GRACE_PERIOD = 3 * 24 * 60 * 60;
    }
}
namespace Shadowfront.Backend.Board.BoardPieces
{
    public static class DefaultObjectAttributes
    {
        public static class Keys
        {
            public const string HEALTH = "health";

            public const string EVASION = "evasion";
        }

        public static ObjectAttribute Health => new()
        {
            Name = "Health",
        };

        public static ObjectAttribute Evasion => new()
        {
            Name = "Evasion",
        };
    }
}

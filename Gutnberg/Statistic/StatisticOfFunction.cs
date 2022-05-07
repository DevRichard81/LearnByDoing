namespace Project_Gutenberg.Statistic
{
    public record StatisticOfFunction
    {
        public enum Type { Incoming, Outcoming, None}
        /// <summary>
        /// 0 = incoming, 1 = outcoming
        /// </summary>
        public Type type = Type.None;
        public uint handelDataLength = 0;
        public uint handelMessage = 0;

        public void Reset()
        {
            type = Type.None;
            handelDataLength = 0;
            handelMessage = 0;
        }
    }
}

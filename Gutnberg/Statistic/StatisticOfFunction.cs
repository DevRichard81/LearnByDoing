using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gutenberg.Statistic
{
    public record StatisticOfFunction
    {
        public enum Type { Incoming, Outcoming, None}
        /// <summary>
        /// 1 = incoming, 2 = outcoming
        /// </summary>
        public Type type = 0;
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

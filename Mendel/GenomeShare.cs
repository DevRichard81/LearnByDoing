using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mendel
{
    internal static class GenomeShare
    {
        public static Random Random = new Random((int)DateTime.Now.Ticks);
        public static bool ShowDebugMessages { get; set; }

        public static bool Percentage(decimal ChanceAsPercentage)
        {
            int randomNumber = Random.Next(1, 100);
            if (ChanceAsPercentage > 0 && randomNumber <= ChanceAsPercentage)
            {
                return true;
            }
            return false;
        }

        public static void Inform(string message)
        {
            if(ShowDebugMessages)
                Console.WriteLine(message);
        }
    }
}

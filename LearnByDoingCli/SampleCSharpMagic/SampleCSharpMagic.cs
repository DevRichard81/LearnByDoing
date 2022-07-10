using System.Text;
using System.Threading;
using CSharpMagic;
using Project_Gutenberg;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Types.File;

namespace LearnByDoingCli
{
    internal class SampleCSharpMagic : SampleShared, ISamples
    {
        private DecodeCSharp decodeCSharp = new DecodeCSharp();

        public int valueIntPu = 1;
        private int valueIntPr = 2;
        public string valueStringPu = "String 1";
        private string valueStringPr = "String 2";
        public int valueIntPrPu { get; set; } = 3;
        public string valueStringPrPu { get; set; } = "String 3";
        private int valueIntPrPu2 { get; set; } = 4;
        public string valueStringPrPu2 { get; set; } = "String 4";
        public List<int> listIntPu { get; set; } = new List<int>() { 1, 2, 3, 4, 5};

        public SampleCSharpMagic()
        {
        }

        public void Run()
        {
            Console.WriteLine("--------------------------");
            Console.WriteLine("Doing a CSharpMagic Sample");
            Console.WriteLine("--------------------------");

            decodeCSharp.GetObject(this);
        }
    }
}

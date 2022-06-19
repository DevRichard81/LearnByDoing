using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mendel
{
    public class Genome
    {
        private static readonly Random _random = new();

        private bool[] _genes;
        public IEnumerable<bool> Genes
        {
            get { return _genes; }
        }
        public Guid Id;
        public int Total
        {
            get
            {
                // For the dice example
                return TotalForDiceExample();

                // return GetTotal();
            }
        }
        public int Length { get { return _genes.Length; } }
        public int Fitness { get; set; }

        public Genome Clone()
        {
            Genome clonedGenome = new(_genes.Length);
            List<bool> genes = Genes.ToList();
            for (int i = 0; i < _genes.Length; i++)
            {
                clonedGenome._genes[i] = genes[i];
            }

            return clonedGenome;
        }
        public override bool Equals(object? obj)
        {
            if (obj is not Genome genome)
                return false;

            return genome.Id == Id;
        }
        /// <summary>
        /// Creates a <see cref="Genome"/> from a string of bits.
        /// </summary>
        /// <param name="bitString">A bit string, e.g. 100 001</param>
        /// <returns></returns>
        public static Genome FromString(string bitString)
        {
            if (string.IsNullOrEmpty(bitString))
                throw new ArgumentNullException(nameof(bitString), "bitString parameter is empty");

            bitString = bitString.Replace(" ", "");

            Genome genome = new(bitString.Length);
            for (int i = 0; i < bitString.Length; i++)
            {
                if (bitString[i] != '0')
                    genome.SetGeneOn(i);
            }

            return genome;
        }
        public Genome(int maxGenes)
        {
            _genes = new bool[maxGenes];
            Id = Guid.NewGuid();
        }
        //private int GetTotal()
        //{
        //    string bitstring = "".PadLeft(32 - _genes.Length, '0');
        //    bitstring += string.Join("", _genes.Select(g => g == true ? "1" : "0"));

        //    return Convert.ToInt32(bitstring);
        //}
        public void RandomizeGeneValues()
        {
            for (int i = 0; i < _genes.Length; i++)
            {
                _genes[i] = RandomizeGeneValue();
            }
        }
        public static bool RandomizeGeneValue()
        {
            // The gene is "1" when the rng is over 50
            if (_random.Next(1, 100) > 50)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SetGeneOff(int gene)
        {
            _genes[gene] = false;
        }
        public void SetGeneOn(int gene) 
        {
            _genes[gene] = true;
        }
        public void SwapGenes(int position1, int position2)
        {
            bool position1Value = _genes[position1];
            bool position2Value = _genes[position2];
            _genes[position1] = position2Value;
            _genes[position2] = position1Value;
        }
        public void InsertGenes(int position)
        {
            if (position < 0 || position >= _genes.Length)
                return;

            bool[] oldGenes = new bool[_genes.Length];
            Array.Copy(_genes, oldGenes, _genes.Length);

            _genes = new bool[oldGenes.Length + 1];
            if(position-1 > 0) // Special case when we insert at first position
                Array.Copy(oldGenes, 0, _genes, 0, position - 1);
            _genes[position] = RandomizeGeneValue(); // Insert Random new Gen
            Array.Copy(oldGenes, position, _genes, position+1, oldGenes.Length-position);
        }
        public void DeleteGenes(int position)
        {
            if (position < 0 || position >= _genes.Length || _genes.Length <= 1)
                return;

            bool[] oldGenes = new bool[_genes.Length];
            Array.Copy(_genes, oldGenes, _genes.Length);
            _genes = new bool[oldGenes.Length - 1];

            if (position == 0)
            {
                Array.Copy(_genes, 1, oldGenes, 0, _genes.Length-1);
                return;
            }

            Array.Copy(oldGenes, 0, _genes, 0, position - 1);
            Array.Copy(oldGenes, position, _genes, position, _genes.Length-position);
        }
        public void SwitchGenRandom(int position)
        {
            if (position < 0 || position >= _genes.Length)
                return;

            _genes[position] = RandomizeGeneValue();
        }

        public void SwapWith(Genome genome, int toPosition)
        {
            List<bool> sourceGenes = genome.Genes.ToList();
            for (int i = 0; i < toPosition; i++)
            {
                _genes[i] = sourceGenes[i];
            }
        }
        public override string ToString()
        {
            string currentGene;
            string allGenes = String.Empty;
            string currentTotal = String.Empty;
            List<int> genomeTotal = new();

            // Splits the genome into blocks of 3 bits
            for (int i = 0; i < _genes.Length; i++)
            {
                if (i > 0 && i % 3 == 0)
                {
                    genomeTotal.Add(Convert.ToInt32(currentTotal, 2));
                    currentTotal = "";
                }

                if (i > 0 && i % 3 == 0 && i < _genes.Length)
                {
                    allGenes += " ";
                }

                currentGene = _genes[i] ? "1" : "0";
                currentTotal += currentGene;

                if (i == _genes.Length - 1)
                {
                    genomeTotal.Add(Convert.ToInt32(currentTotal, 2));
                }

                allGenes += currentGene;
            }

            allGenes += string.Format(" ({0})", string.Join(",", genomeTotal));
            return allGenes;
        }
        /// <summary>
        /// Sums up the total of all the genes by converting the bit string into a 
        /// 32 bit binary value. This method assumes the length of the genome is always 6.
        /// </summary>
        private int TotalForDiceExample()
        {
            string bitstring = "".PadLeft(32 - 3, '0');

            // First block
            string firstChunk = bitstring;
            firstChunk += _genes[0] ? "1" : "0";

            if (_genes.Length > 1)
                firstChunk += _genes[1] ? "1" : "0";

            if (_genes.Length > 2)
                firstChunk += _genes[2] ? "1" : "0";

            int total1 = Convert.ToInt32(firstChunk, 2);

            // Second block
            string secondChunk = bitstring;

            if (_genes.Length > 3)
                secondChunk += _genes[3] ? "1" : "0";

            if (_genes.Length > 4)
                secondChunk += _genes[4] ? "1" : "0";

            if(_genes.Length > 5)
                secondChunk += _genes[5] ? "1" : "0";

            int total2 = Convert.ToInt32(secondChunk, 2);

            return total1 + total2;
        }

    }
}

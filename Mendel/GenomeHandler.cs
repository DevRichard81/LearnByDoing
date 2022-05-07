using System;

namespace Mendel
{
    public class GenomeHandler
    {
		private static Random _random = new Random();
		private static int _geneSize;
		private static bool AllowMutateSwitch = false;
		private static bool AllowMutateInsert = false;
		private static bool AllowMutateDelete = false;
		private int _populationSize;
		private int _crossOverChance;
		private int _mutationChance;
		private int _mutationChanceIndel;
		private int _mutationChancePoint;

		public IList<Genome> Population { get; set; }

		public void CrossOver(Random random = null)
		{
			EnsurePopulationIsCreated();

			if (random == null)
				random = _random;

			// Figure out if crossover should occur for this generation, based on a roll of a random number
			decimal percentage = _crossOverChance;
			int randomNumber = random.Next(1, 100);

			if (percentage > 0 && randomNumber <= percentage)
			{
				// Loop through all genome pairs
				for (int i = 0; i < Population.Count; i += 2)
				{
					if (i > Population.Count)
						break;

					Genome genome1 = Population[i];
					Genome genome2 = Population[i + 1];

					// Pick a random position to swap at
					int position = random.Next(0, Math.Min(genome1.Length, genome2.Length));

					// Create 2 new genomes with the two parts swapped
					Genome newGenome1 = genome1.Clone();
					Genome newGenome2 = genome2.Clone();

					newGenome1.SwapWith(genome2, position);
					newGenome2.SwapWith(genome1, position);

					Population[i] = newGenome1;
					Population[i + 1] = newGenome2;
				}
			}
			else
			{
				// (No cross over, return)
				GenomeShare.Inform("No crossover performed - the random {"+ randomNumber + "}% was over the {"+percentage+"}% threshold.");
			}
		}
		private void EnsurePopulationIsCreated()
		{
			if (Population.Count == 0)
				throw new InvalidOperationException("The population is empty! Use InitializePopulation() first");
		}
		public virtual int FitnessFunction(Genome genome) 
        {
            return genome.Total;
        }
		public Genome GetChampion()
		{
			for (int i = 0; i < Population.Count; i++)
			{
				if (FitnessFunction(Population[i]) == 14)
					return Population[i];
			}
			return null;
		}
		public void InitializePopulation()
		{
			Population = new List<Genome>();

			for (int i = 0; i < _populationSize; i++)
			{
				Genome genome = new Genome(_geneSize);
				genome.RandomizeGeneValues();

				Population.Add(genome);
			}
		}

		/// <summary>
		/// Mutate on one point
		/// </summary>
		/// <param name="genome"></param>
		public void MutatePoint(ref Genome genome)
        {
			if (genome.Length == 0)
				return;

			GenomeShare.Inform(nameof(MutateIndels) + ":" + nameof(genome.SwitchGenRandom));
			int position1 = _random.Next(0, _geneSize);
			genome.SwitchGenRandom(position1);
		}
		/// <summary>
		/// Mutation to insert, delete or switch genes
		/// </summary>
		/// <param name="genome"></param>
		public static void MutateIndels(ref Genome genome)
        {			
			int position1;
			int position2;

			switch (_random.Next(0, 3))
			{
				case 0 when AllowMutateSwitch:
					GenomeShare.Inform(nameof(MutateIndels) + ":" + nameof(genome.SwapGenes));
					position1 = _random.Next(0, genome.Length - 1);
					position2 = _random.Next(0, genome.Length - 1);
					genome.SwapGenes(position1, position2);
					break;
				case 1 when AllowMutateInsert:
					GenomeShare.Inform(nameof(MutateIndels) + ":" + nameof(genome.InsertGenes));
					position1 = _random.Next(0, genome.Length - 1);
					genome.InsertGenes(position1);
					Interlocked.Increment(ref _geneSize);
					break;
				case 2 when AllowMutateDelete:
				case 3 when AllowMutateDelete:
					GenomeShare.Inform(nameof(MutateIndels) + ":" + nameof(genome.DeleteGenes));
					position1 = _random.Next(0, genome.Length - 1);
					genome.DeleteGenes(position1);
					Interlocked.Decrement(ref _geneSize);
					break;
			}			
		}

		public void Mutate(Random random = null)
		{
			EnsurePopulationIsCreated();

			if (random == null)
				random = _random;

            // Figure out if mutation should occur for this generation, based on a roll of a random number
            if (GenomeShare.Percentage(_mutationChance))
            {
				GenomeShare.Inform(nameof(Mutate));
				// Loop through all genome pairs
				for (int i = 0; i < Population.Count; i += 2)
				{
					if (i > Population.Count)
						break;

					Genome genome = Population[i];

                    if (GenomeShare.Percentage(_mutationChanceIndel))
                    {
						MutateIndels(ref genome);
					}
                    if(GenomeShare.Percentage(_mutationChancePoint))
                    {
						MutatePoint(ref genome);
					}
				}
			}
		}
		public void NextGeneration()
		{
			EnsurePopulationIsCreated();

			List<Genome> nextGeneration = new List<Genome>();

			for (int i = 0; i < Population.Count; i++)
			{
				Genome genome = SpinBiasedRouletteWheel();
				nextGeneration.Add(genome);
			}

			Population = nextGeneration;
		}
		public Genome SpinBiasedRouletteWheel(Random random = null)
		{
			EnsurePopulationIsCreated();

			if (random == null)
				random = _random;

			// Get the total fitness value of all genomes, !! warning can be also 0 !!
			int populationTotal = Population.Sum(x => x.Total);

			for (int i = 0; i < Population.Count; i++)
			{
				Genome genome = Population[i];

				// Weighted % value of each genome: genome fitness value/ total
				// This % represents the chance the genome is picked.				
				// Secure there is no death populationTotal
				decimal percentage = (decimal)genome.Total / (populationTotal == 0 ? 1 : populationTotal) * 100;

				// Roll 1-100. If the % lies within in this number, return it.
				// For example: 
				//	percentage is 60%
				//	random number is 75
				//  = doesn't get picked
				if(GenomeShare.Percentage(percentage))
				{
					return genome;
				}
			}

			return Population.First();
		}
		public override string ToString()
		{
			string result = "";

			// Total up all genomes (the entire population)
			for (int i = 0; i < Population.Count; i++)
			{
				result += Population[i].ToString() + Environment.NewLine;
			}

			return result;
		}

		/// <summary>
		/// Creates a new instance of a GA world.
		/// </summary>
		/// <param name="geneSize">The number of genes in a genome</param>
		/// <param name="populationSize">The size of the population</param>
		/// <param name="crossOverChance">The percentage chance (1-100) of a cross-over occuring (a set
		/// of genes being swapped) between two genomes that mate.</param>
		/// <param name="mutationChance">The percentage chance (1-100) of a mutation occuring (a single
		/// gene being swapped) between two genomes that mate.</param>
		public GenomeHandler(int geneSize, int populationSize, int crossOverChance)
		{
			_geneSize = geneSize;
			_populationSize = populationSize;
			_crossOverChance = crossOverChance;
			
			Population = new List<Genome>();
			InitializePopulation();
			SetMutation((byte)_random.Next(0, 100), (byte)_random.Next(0, 100), (byte)_random.Next(0, 100));
		}
		public GenomeHandler SetMutation(byte mutationChance, byte mutationChanceIndel, byte mutationChancePoint)
        {
			_mutationChance = mutationChance;
			_mutationChanceIndel = mutationChanceIndel;
			_mutationChancePoint = mutationChancePoint;
			return this;
		}
		public GenomeHandler SetAllowMutation(bool swap, bool insert, bool delete)
        {
			AllowMutateSwitch = swap;
			AllowMutateInsert = insert;
			AllowMutateDelete = delete;
			return this;
        }
		public GenomeHandler SetDebug(bool ifDebuging)
        {
			GenomeShare.ShowDebugMessages = ifDebuging;
			return this;
        }
	}
}
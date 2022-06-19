using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mendel;

namespace LearnByDoingCli
{
    public class SampleMendel : ISamples
    {
        public void Run()
        {
			// Arrange
			int worldSize = 4;
			int geneCount = 6;
			int crossOverChance = 30;
			GenomeHandler world = new GenomeHandler(geneCount, worldSize, crossOverChance)
				.SetMutation(mutationChance: 10, mutationChanceIndel: 2, mutationChancePoint: 1)
				.SetAllowMutation(true, true, true)
				.SetDebug(true);
			world.InitializePopulation();

			// Act
			int generation = 0;
			Genome? champion = null;
			while (champion == null)
			{
				world.Mutate(new Random());
				world.CrossOver(new Random());
				world.NextGeneration();
				Console.WriteLine(world);

				generation++;
				champion = world.GetChampion();
			}

			Console.WriteLine("A new leader is born! generation {0} - {1}", generation, champion);
		}
    }
}

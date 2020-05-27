using System.Threading;
using System.Collections;
using System.Collections.Generic;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using UnityEngine;

public class GAController : MonoBehaviour {
    private GeneticAlgorithm m_ga;
    private Thread m_gaThread;

    public Object PointPrefab;
    [HideInInspector]
    public List<Point> finalPoints;
    public delegate void TSPSolved();
    public TSPSolved tspSolved;

    public void TSPSolve(int numberOfPoints, List<Point> stationList) {
        var fitness = new TspFitness(stationList);
        var chromosome = new TspChromosome(numberOfPoints);

        var crossover = new OrderedCrossover();
        var mutation = new ReverseSequenceMutation();
        var selection = new RouletteWheelSelection();
        var population = new Population(50, 100, chromosome);

        m_ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
        m_ga.Termination = new FitnessStagnationTermination(100);

        m_ga.TaskExecutor = new ParallelTaskExecutor {
            MinThreads = 100,
            MaxThreads = 200
        };

        m_ga.GenerationRan += delegate {
            var distance = ((TspChromosome)m_ga.BestChromosome).Distance;
        };

        m_ga.TerminationReached += delegate {
            var c = m_ga.Population.CurrentGeneration.BestChromosome as TspChromosome;
            var genes = c.GetGenes();
            var points = ((TspFitness)m_ga.Fitness).Points;
            finalPoints = new List<Point>();
            for (int i = 0; i < genes.Length; i++) {
                finalPoints.Add(points[(int)genes[i].Value]);
            }
            tspSolved();
        };

        m_gaThread = new Thread(() => m_ga.Start());
        m_gaThread.Start();
    }

    private void OnDestroy() {
        if (m_ga != null) {
            m_ga.Stop();
            m_gaThread.Abort();
        } 
    }
}
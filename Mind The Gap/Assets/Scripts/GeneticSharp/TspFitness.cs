using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using UnityEngine;

public class TspFitness : IFitness {
    private Rect m_area;

    public TspFitness(List<Point> inputPoints) {
        Points = inputPoints;
    }

    public IList<Point> Points { get; private set; }

    public double Evaluate(IChromosome chromosome) {
        var genes = chromosome.GetGenes();
        var distanceSum = 0.0;
        var lastPointIndex = Convert.ToInt32(genes[0].Value, CultureInfo.InvariantCulture);
        var pointsIndexes = new List<int>();

        foreach (var g in genes) {
            var currentPointIndex = Convert.ToInt32(g.Value, CultureInfo.InvariantCulture);
            distanceSum += CalcDistanceTwoPoints(Points[currentPointIndex], Points[lastPointIndex]);
            lastPointIndex = currentPointIndex;

            pointsIndexes.Add(lastPointIndex);
        }

        distanceSum += CalcDistanceTwoPoints(Points[pointsIndexes.Last()], Points[pointsIndexes.First()]);

        var fitness = 1.0 - (distanceSum / (Points.Count * 1000.0));

        ((TspChromosome)chromosome).Distance = distanceSum;

        var diff = Points.Count - pointsIndexes.Distinct().Count();

        if (diff > 0) {
            fitness /= diff;
        }

        if (fitness < 0) {
            fitness = 0;
        }

        return fitness;
    }

    private static double CalcDistanceTwoPoints(Point one, Point two) {
        return Vector2.Distance(new Vector2(one.xCoord, one.zCoord), new Vector2(two.xCoord, two.zCoord));
    }
}
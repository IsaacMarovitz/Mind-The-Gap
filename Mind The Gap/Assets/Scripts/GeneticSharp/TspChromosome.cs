using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;

public class TspChromosome : ChromosomeBase {
    public readonly int m_numberOfPoints;

    public TspChromosome(int numberOfPoints) : base(numberOfPoints) {
        m_numberOfPoints = numberOfPoints;
        var pointsIndex = RandomizationProvider.Current.GetUniqueInts(numberOfPoints, 0, numberOfPoints);

        for (int i = 0; i < numberOfPoints; i++) {
            ReplaceGene(i, new Gene(pointsIndex[i]));
        }
    }

    public double Distance {get; internal set; }

    public override Gene GenerateGene(int geneIndex) {
        return new Gene(RandomizationProvider.Current.GetInt(0, m_numberOfPoints));
    }

    public override IChromosome CreateNew() {
        return new TspChromosome(m_numberOfPoints);
    }

    public override IChromosome Clone() {
        var clone = base.Clone() as TspChromosome;
        clone.Distance = Distance;

        return clone;
    }
}

using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

/// <summary>Tests for ProjectionTypes domain types.</summary>
[TestFixture]
public class ProjectionTypesTests
{

    [Test]
    public void TestAllProjectionTypesCases()
    {
        // Test that all cases exist
        var allCases = Enum.GetValues<ProjectionTypes>();
        Assert.That(allCases, Contains.Item(ProjectionTypes.TwoDimensional));
        Assert.That(allCases, Contains.Item(ProjectionTypes.ObliqueLongitude));
    }

    [Test]
    public void TestAllCasesCompleteness()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        Assert.That(allCases, Contains.Item(ProjectionTypes.TwoDimensional));
        Assert.That(allCases, Contains.Item(ProjectionTypes.ObliqueLongitude));
        Assert.That(allCases, Has.Length.EqualTo(2)); // Total number of projection types
    }

    // MARK: - LocalizedName Tests

    [Test]
    public void TestLocalizedNameTwoDimensional()
    {
        Assert.That(ProjectionTypes.TwoDimensional.LocalizedName(), Is.EqualTo("enum.projectiontype.twodimensional"));
    }

    [Test]
    public void TestLocalizedNameObliqueLongitude()
    {
        Assert.That(ProjectionTypes.ObliqueLongitude.LocalizedName(), Is.EqualTo("enum.projectiontype.obliquelongitude"));
    }

    [Test]
    public void TestLocalizedNameAll()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ProjectionTypes.TwoDimensional.LocalizedName(), Is.EqualTo("enum.projectiontype.twodimensional"));
            Assert.That(ProjectionTypes.ObliqueLongitude.LocalizedName(), Is.EqualTo("enum.projectiontype.obliquelongitude"));
        }
    }

    [Test]
    public void TestLocalizedNameAllProjectionTypes()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        foreach (var projectionType in allCases)
        {
            var name = projectionType.LocalizedName();
            Assert.That(name, Is.Not.Empty, $"Projection type {projectionType} has empty localized name");
            Assert.That(name, Does.StartWith("enum.projectiontype"), 
                $"Projection type {projectionType} localized name does not start with 'enum.projectiontype'");
        }
    }

    // MARK: - FromIndex Tests

    [Test]
    public void TestFromIndexValid()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        for (var index = 0; index < allCases.Length; index++)
        {
            var expectedProjectionType = allCases[index];
            var projectionType = ProjectionTypesExtensions.FromIndex(index);
            Assert.That(projectionType, Is.EqualTo(expectedProjectionType), 
                $"Index {index} should return {expectedProjectionType}");
        }
    }

    [Test]
    public void TestFromIndexFirst()
    {
        var projectionType = ProjectionTypesExtensions.FromIndex(0);
        var allCases = Enum.GetValues<ProjectionTypes>();
        Assert.That(projectionType, Is.EqualTo(allCases.First()));
        Assert.That(projectionType, Is.EqualTo(ProjectionTypes.TwoDimensional));
    }

    [Test]
    public void TestFromIndexLast()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        var lastIndex = allCases.Length - 1;
        var projectionType = ProjectionTypesExtensions.FromIndex(lastIndex);
        Assert.That(projectionType, Is.EqualTo(allCases.Last()));
        Assert.That(projectionType, Is.EqualTo(ProjectionTypes.ObliqueLongitude));
    }

    [Test]
    public void TestFromIndexNegative()
    {
        var projectionType = ProjectionTypesExtensions.FromIndex(-1);
        Assert.That(projectionType, Is.Null);
    }

    [Test]
    public void TestFromIndexTooLarge()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        var tooLargeIndex = allCases.Length;
        var projectionType = ProjectionTypesExtensions.FromIndex(tooLargeIndex);
        Assert.That(projectionType, Is.Null);
    }

    [Test]
    public void TestFromIndexBoundary()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        var boundaryIndex = allCases.Length;
        var projectionType = ProjectionTypesExtensions.FromIndex(boundaryIndex);
        Assert.That(projectionType, Is.Null);

        var validBoundaryIndex = allCases.Length - 1;
        var validProjectionType = ProjectionTypesExtensions.FromIndex(validBoundaryIndex);
        Assert.That(validProjectionType, Is.Not.Null);
        Assert.That(validProjectionType, Is.EqualTo(ProjectionTypes.ObliqueLongitude));
    }

    [Test]
    public void TestFromIndexSpecific()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ProjectionTypesExtensions.FromIndex(0), Is.EqualTo(ProjectionTypes.TwoDimensional));
            Assert.That(ProjectionTypesExtensions.FromIndex(1), Is.EqualTo(ProjectionTypes.ObliqueLongitude));
        }
    }

    // MARK: - Raw Value Tests

    [Test]
    public void TestRawValuesSequential()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        for (var index = 0; index < allCases.Length; index++)
        {
            var projectionType = allCases[index];
            Assert.That((int)projectionType, Is.EqualTo(index), 
                $"Projection type {projectionType} should have raw value {index}");
        }
    }

    [Test]
    public void TestRawValuesMatchExpected()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That((int)ProjectionTypes.TwoDimensional, Is.EqualTo(0));
            Assert.That((int)ProjectionTypes.ObliqueLongitude, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestRawValuesUnique()
    {
        var rawValues = new HashSet<int>();
        var allCases = Enum.GetValues<ProjectionTypes>();
        foreach (var projectionType in allCases)
        {
            var rawValue = (int)projectionType;
            Assert.That(rawValues, Does.Not.Contain(rawValue), 
                $"Duplicate raw value {rawValue} found for projection type {projectionType}");
            rawValues.Add(rawValue);
        }
    }

    // MARK: - Comprehensive Tests

    [Test]
    public void TestAllProjectionTypesHaveLocalizedName()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        foreach (var projectionType in allCases)
        {
            var name = projectionType.LocalizedName();
            Assert.That(name, Is.Not.Empty, $"Projection type {projectionType} has empty localized name");
        }
    }

    [Test]
    public void TestCaseIterable()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        Assert.That(allCases, Has.Length.EqualTo(2));

        // Verify we can iterate
        var count = 0;
        foreach (var _ in allCases)
        {
            count++;
        }
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void TestIntBacked()
    {
        // Test that we can create from raw value
        const ProjectionTypes twoDimensional = 0;
        Assert.That(twoDimensional, Is.EqualTo(ProjectionTypes.TwoDimensional));

        const ProjectionTypes obliqueLongitude = (ProjectionTypes)1;
        Assert.That(obliqueLongitude, Is.EqualTo(ProjectionTypes.ObliqueLongitude));
    }

    [Test]
    public void TestAllProjectionTypesDistinct()
    {
        Assert.That(ProjectionTypes.TwoDimensional, Is.Not.EqualTo(ProjectionTypes.ObliqueLongitude));
        Assert.That(ProjectionTypes.TwoDimensional.LocalizedName(), 
            Is.Not.EqualTo(ProjectionTypes.ObliqueLongitude.LocalizedName()));
    }

    [Test]
    public void TestPopularProjectionTypes()
    {
        // Test the most commonly used projection types
        var allCases = Enum.GetValues<ProjectionTypes>();
        Assert.That(allCases, Contains.Item(ProjectionTypes.TwoDimensional));
        Assert.That(allCases, Contains.Item(ProjectionTypes.ObliqueLongitude));
    }

    [Test]
    public void TestLocalizedNameFormat()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        foreach (var projectionType in allCases)
        {
            var name = projectionType.LocalizedName();
            // Verify format: enum.projectiontype.<name>
            Assert.That(name, Does.Match(@"^enum\.projectiontype\.[a-z]+$"), 
                $"Projection type {projectionType} localized name should match format 'enum.projectiontype.<name>'");
        }
    }

    [Test]
    public void TestFromIndexAllValidIndices()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        for (var i = 0; i < allCases.Length; i++)
        {
            var projectionType = ProjectionTypesExtensions.FromIndex(i);
            Assert.That(projectionType, Is.Not.Null, $"Index {i} should return a valid projection type");
            Assert.That(projectionType, Is.EqualTo(allCases[i]), 
                $"Index {i} should return {allCases[i]}");
        }
    }

    [Test]
    public void TestFromIndexInvalidIndices()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ProjectionTypesExtensions.FromIndex(-1), Is.Null);
            Assert.That(ProjectionTypesExtensions.FromIndex(2), Is.Null);
            Assert.That(ProjectionTypesExtensions.FromIndex(100), Is.Null);
        }
    }
}


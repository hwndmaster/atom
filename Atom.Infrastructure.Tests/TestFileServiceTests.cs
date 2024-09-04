using Genius.Atom.Infrastructure.TestingUtil;
using Genius.Atom.Infrastructure.TestingUtil.Io;

namespace Genius.Atom.Infrastructure.Tests;

public sealed class TestFileServiceTests
{
    private readonly IFixture _fixture = InfrastructureTestHelper.CreateFixture();
    private readonly FakeFileService _sut = new();

    [Fact]
    public void CreateFile()
    {
        // Arrange
        var sample = _fixture.CreateMany<byte>().ToArray();

        // Act
        using (var stream = _sut.CreateFile(@"C:\Test\file.ext"))
        {
            stream.Write(sample, 0, sample.Length);
        }

        // Verify
        Assert.Equal(sample, _sut.Files.Single().Content);
    }

    [Fact]
    public void EnumerateDirectories()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act
        var actual = _sut.EnumerateDirectories(@"C:\Test").ToArray();

        // Verify
        var expected = new [] {
            @"C:\Test\SubDirectory1",
            @"C:\Test\SubDirectory2",
            @"C:\Test\SubDirectory3",
            @"C:\Test\AnotherSubDirectory2"
        };
        Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateDirectories_WithSearchPattern_ScenarioWithLeadingWildcard()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act
        var actual = _sut.EnumerateDirectories(@"C:\Test", "*2").ToArray();

        // Verify
        var expected = new [] {
            @"C:\Test\SubDirectory2",
            @"C:\Test\AnotherSubDirectory2"
        };
        Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateDirectories_WithSearchPattern_ScenarioWithEndingWildcard()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act
        var actual = _sut.EnumerateDirectories(@"C:\Test", "Sub*").ToArray();

        // Verify
        var expected = new [] {
            @"C:\Test\SubDirectory1",
            @"C:\Test\SubDirectory2",
            @"C:\Test\SubDirectory3"
        };
        Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateDirectories_WithSearchPattern_AndRecurseSubdirectories()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act
        var actual = _sut.EnumerateDirectories(@"C:\Test", "*Directory2", SearchOption.AllDirectories).ToArray();

        // Verify
        var expected = new [] {
            @"C:\Test\SubDirectory1\DeeperDirectory2",
            @"C:\Test\SubDirectory2",
            @"C:\Test\SubDirectory2\DeeperDirectory2",
            @"C:\Test\AnotherSubDirectory2"
        };
        Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateFiles()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act
        var actual = _sut.EnumerateFiles(@"C:\Test\SubDirectory2").ToArray();

        // Verify
        var expected = new [] {
            @"C:\Test\SubDirectory2\file2.ext",
            @"C:\Test\SubDirectory2\file2a.ext",
            @"C:\Test\SubDirectory2\file2b.bar"
        };
        Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateFiles_WithSearchPattern_ScenarioWithLeadingWildcard()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act
        var actual = _sut.EnumerateFiles(@"C:\Test\SubDirectory2", "*.ext").ToArray();

        // Verify
        var expected = new [] {
            @"C:\Test\SubDirectory2\file2.ext",
            @"C:\Test\SubDirectory2\file2a.ext"
        };
        Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateFiles_WithSearchPattern_ScenarioWithEndingWildcard()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act
        var actual = _sut.EnumerateFiles(@"C:\Test\SubDirectory1", "file11*").ToArray();

        // Verify
        var expected = new [] {
            @"C:\Test\SubDirectory1\file11b.bar",
            @"C:\Test\SubDirectory1\file11c.bar"
        };
        Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateFiles_WithSearchPattern_AndRecurseSubdirectories()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act
        var actual = _sut.EnumerateFiles(@"C:\Test", "*.bar", SearchOption.AllDirectories).ToArray();

        // Verify
        var expected = new [] {
            @"C:\Test\SubDirectory1\file11b.bar",
            @"C:\Test\SubDirectory1\file11c.bar",
            @"C:\Test\SubDirectory1\DeeperDirectory2\file12e.bar",
            @"C:\Test\SubDirectory2\file2b.bar",
            @"C:\Test\SubDirectory2\DeeperDirectory2\file22d.bar",
            @"C:\Test\topfile3.bar"
        };
        Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeleteDirectory_Recursive()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act
        _sut.DeleteDirectory("C:\\Test", recursive: true);

        // Verify
        var expectedDirs = new [] {
            @"C:\CompletelyDifferentPath",
        };
        var expectedFiles = new [] {
            @"C:\CompletelyDifferentPath\undesirable.ext",
            @"C:\CompletelyDifferentPath\undesirable.bar"
        };
        Assert.Equal(expectedDirs, _sut.Directories.Select(x => x.FullName), StringComparer.OrdinalIgnoreCase);
        Assert.Equal(expectedFiles, _sut.Files.Select(x => x.FullName), StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeleteDirectory_NonRecursive_NonEmpty_ThrowsException()
    {
        // Arrange
        SetupSampleFolderStructure();

        // Act & Verify
        Assert.Throws<IOException>(() => _sut.DeleteDirectory("C:\\Test", recursive: false));
    }

    private void SetupSampleFolderStructure()
    {
        _sut.AddFile(@"C:\Test\SubDirectory1\file1.ext");
        _sut.AddFile(@"C:\Test\SubDirectory1\file1a.ext");
        _sut.AddFile(@"C:\Test\SubDirectory1\file11b.bar");
        _sut.AddFile(@"C:\Test\SubDirectory1\file11c.bar");
        _sut.AddFile(@"C:\Test\SubDirectory1\DeeperDirectory1\file12d.ext");
        _sut.AddFile(@"C:\Test\SubDirectory1\DeeperDirectory2\file12e.bar");
        _sut.AddFile(@"C:\Test\SubDirectory2\file2.ext");
        _sut.AddFile(@"C:\Test\SubDirectory2\file2a.ext");
        _sut.AddFile(@"C:\Test\SubDirectory2\file2b.bar");
        _sut.AddFile(@"C:\Test\SubDirectory2\DeeperDirectory1\file22c.ext");
        _sut.AddFile(@"C:\Test\SubDirectory2\DeeperDirectory2\file22d.bar");
        _sut.AddFile(@"C:\Test\SubDirectory3\file3.ext");
        _sut.AddFile(@"C:\Test\AnotherSubDirectory2\file2b.ext");
        _sut.AddFile(@"C:\Test\topfile.ext");
        _sut.AddFile(@"C:\Test\topfile2.ext");
        _sut.AddFile(@"C:\Test\topfile3.bar");
        _sut.AddFile(@"C:\CompletelyDifferentPath\undesirable.ext");
        _sut.AddFile(@"C:\CompletelyDifferentPath\undesirable.bar");
    }
}

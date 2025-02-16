using Moq;
using Nuke.Common.IO;
using Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations;
using Rachkov.InspectaQueue.AutoUpdater.Core.Services.Paths;
using Rachkov.InspectaQueue.AutoUpdater.Migrations.Models.Interfaces;

namespace AutoUpdater.Tests;

public class MigrationServiceTests
{
    private Mock<IApplicationPathsConfiguration> _pathsConfigMock;
    private Mock<IInstallerRunner> _installerRunnerMock;
    private MigrationService _migrationService;
    private AbsolutePath _tempConfigPath;

    [SetUp]
    public void Setup()
    {
        _pathsConfigMock = new Mock<IApplicationPathsConfiguration>();
        _installerRunnerMock = new Mock<IInstallerRunner>();
        _migrationService = new MigrationService(_pathsConfigMock.Object, _installerRunnerMock.Object);
        _tempConfigPath = Path.GetTempFileName();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_tempConfigPath))
        {
            File.Delete(_tempConfigPath);
        }
    }

    [Test]
    public async Task GivenNoConfigFile_WhenMigrateConfigFiles_ThenReturnsTrue()
    {
        // Arrange
        _pathsConfigMock.Setup(x => x.ConfigFilePath).Returns(() => _tempConfigPath / "nonexistent.json");

        // Act
        var result = await _migrationService.MigrateConfigFiles();

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GivenEmptyConfigFile_WhenMigrateConfigFiles_ThenReturnsTrue()
    {
        // Arrange
        await File.WriteAllTextAsync(_tempConfigPath, "");
        _pathsConfigMock.Setup(x => x.ConfigFilePath).Returns(_tempConfigPath);

        // Act
        var result = await _migrationService.MigrateConfigFiles();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(await File.ReadAllTextAsync(_tempConfigPath), Is.Empty);
    }

    [Test]
    public async Task GivenConfigFileWithContent_WhenNoMigrations_ThenLeavesContentUnchanged()
    {
        // Arrange
        const string originalContent = "{\"setting\":\"value\"}";
        await File.WriteAllTextAsync(_tempConfigPath, originalContent);
        _pathsConfigMock.Setup(x => x.ConfigFilePath).Returns(_tempConfigPath);

        // Act
        var result = await _migrationService.MigrateConfigFiles();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(await File.ReadAllTextAsync(_tempConfigPath), Is.EqualTo(originalContent));
    }

    [Test]
    public async Task GivenConfigFile_WhenMigrationsExist_ThenAppliesMigrationsInOrder()
    {
        // Arrange
        const string originalContent = "{\"oldSetting\":\"value\"}";
        await File.WriteAllTextAsync(_tempConfigPath, originalContent);
        _pathsConfigMock.Setup(x => x.ConfigFilePath).Returns(_tempConfigPath);

        var migration1 = new Mock<IMigration>();
        migration1.Setup(x => x.MigrateConfig).Returns(json => json.Replace("oldSetting", "newSetting"));

        var migration2 = new Mock<IMigration>();
        migration2.Setup(x => x.MigrateConfig).Returns(json => json.Replace("value", "newValue"));

        var pendingMigrations = GetPendingMigrations();
        pendingMigrations.Add(migration1.Object);
        pendingMigrations.Add(migration2.Object);

        // Act
        var result = await _migrationService.MigrateConfigFiles();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(await File.ReadAllTextAsync(_tempConfigPath), Is.EqualTo("{\"newSetting\":\"newValue\"}"));
    }

    [Test]
    public async Task GivenConfigFile_WhenMigrationThrows_ThenReturnsFalse()
    {
        // Arrange
        const string originalContent = "{\"setting\":\"value\"}";
        await File.WriteAllTextAsync(_tempConfigPath, originalContent);
        _pathsConfigMock.Setup(x => x.ConfigFilePath).Returns(_tempConfigPath);

        var migration = new Mock<IMigration>();
        migration.Setup(x => x.MigrateConfig).Returns(() => (Func<string, string>)(json => throw new Exception("Migration failed")));

        var pendingMigrations = GetPendingMigrations();
        pendingMigrations.Add(migration.Object);

        // Act
        var result = await _migrationService.MigrateConfigFiles();

        // Assert
        Assert.That(result, Is.False);
        Assert.That(await File.ReadAllTextAsync(_tempConfigPath), Is.EqualTo(originalContent));
    }

    [Test]
    public async Task GivenConfigFile_WhenMigrationHasNullMigrateConfig_ThenSkipsMigration()
    {
        // Arrange
        const string originalContent = "{\"setting\":\"value\"}";
        await File.WriteAllTextAsync(_tempConfigPath, originalContent);
        _pathsConfigMock.Setup(x => x.ConfigFilePath).Returns(_tempConfigPath);

        var migration1 = new Mock<IMigration>();
        migration1.Setup(x => x.MigrateConfig).Returns(() => null);

        var migration2 = new Mock<IMigration>();
        migration2.Setup(x => x.MigrateConfig).Returns(json => json.Replace("value", "newValue"));

        var pendingMigrations = GetPendingMigrations();
        pendingMigrations.Add(migration1.Object);
        pendingMigrations.Add(migration2.Object);

        // Act
        var result = await _migrationService.MigrateConfigFiles();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(await File.ReadAllTextAsync(_tempConfigPath), Is.EqualTo("{\"setting\":\"newValue\"}"));
    }

    private List<IMigration> GetPendingMigrations()
    {
        var field = typeof(MigrationService).GetField("_pendingMigrations",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (List<IMigration>)field.GetValue(_migrationService);
    }
}

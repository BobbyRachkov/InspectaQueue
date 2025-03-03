using Moq;
using Nuke.Common.IO;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;
using Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations;
using Rachkov.InspectaQueue.AutoUpdater.Core.Services.Paths;

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

        var pendingMigrations = GetPendingMigrations();
        pendingMigrations.Add(new TestMigrationReplaceOldSetting());
        pendingMigrations.Add(new TestMigrationReplaceValue());

        // Act
        var result = await _migrationService.MigrateConfigFiles();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(await File.ReadAllTextAsync(_tempConfigPath), Is.EqualTo("{\r\n  \"newSetting\": \"newValue\",\r\n  \"AppVersion\": \"2.0.0\"\r\n}"));
    }

    [Test]
    public async Task GivenConfigFile_WhenMigrationThrows_ThenReturnsFalse()
    {
        // Arrange
        const string originalContent = "{\"setting\":\"value\"}";
        await File.WriteAllTextAsync(_tempConfigPath, originalContent);
        _pathsConfigMock.Setup(x => x.ConfigFilePath).Returns(_tempConfigPath);

        var pendingMigrations = GetPendingMigrations();
        pendingMigrations.Add(new TestMigrationThatThrows());

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

        var pendingMigrations = GetPendingMigrations();
        pendingMigrations.Add(new TestMigrationWithNullMigrateConfig());
        pendingMigrations.Add(new TestMigrationReplaceValue());

        // Act
        var result = await _migrationService.MigrateConfigFiles();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(await File.ReadAllTextAsync(_tempConfigPath), Is.EqualTo("{\r\n  \"setting\": \"newValue\",\r\n  \"AppVersion\": \"2.0.0\"\r\n}"));
    }

    private List<IMigration> GetPendingMigrations()
    {
        var field = typeof(MigrationService).GetField("_pendingMigrations",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (List<IMigration>)field.GetValue(_migrationService);
    }

    #region Test Migration Implementations

    private class TestMigrationReplaceOldSetting : MigrationBase
    {
        public override (int major, int minor, int patch) AppVersion => (1, 0, 0);
        public override bool ClearAllProviders => false;
        public override bool KeepOnlyLatestProviderVersion => false;
        public override Func<string, string>? MigrateConfig => json => json.Replace("oldSetting", "newSetting");
        public override IPrerequisite[] Prerequisites { get; init; } = [];
    }

    private class TestMigrationThatThrows : MigrationBase
    {
        public override (int major, int minor, int patch) AppVersion => (1, 0, 0);
        public override bool ClearAllProviders => false;
        public override bool KeepOnlyLatestProviderVersion => false;
        public override Func<string, string>? MigrateConfig => json => throw new Exception("Migration failed");
        public override IPrerequisite[] Prerequisites { get; init; } = [];
    }

    private class TestMigrationWithNullMigrateConfig : MigrationBase
    {
        public override (int major, int minor, int patch) AppVersion => (1, 5, 0);
        public override bool ClearAllProviders => false;
        public override bool KeepOnlyLatestProviderVersion => false;
        public override Func<string, string>? MigrateConfig => null;
        public override IPrerequisite[] Prerequisites { get; init; } = [];
    }

    private class TestMigrationReplaceValue : MigrationBase
    {
        public override (int major, int minor, int patch) AppVersion => (2, 0, 0);
        public override bool ClearAllProviders => false;
        public override bool KeepOnlyLatestProviderVersion => false;
        public override Func<string, string>? MigrateConfig => json => json.Replace("value", "newValue");
        public override IPrerequisite[] Prerequisites { get; init; } = [];
    }

    #endregion
}

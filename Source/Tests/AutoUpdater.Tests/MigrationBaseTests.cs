using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;
using System.Text.Json;

namespace AutoUpdater.Tests;

[TestFixture]
public class MigrationBaseTests
{
    [Test]
    public void GivenJsonWithAppVersion_WhenPerformConfigMigration_ThenConvertsAppVersionToString()
    {
        // Arrange
        var migration = new TestMigration_1_0_0();
        var config = @"{
            ""IsAutoUpdaterEnabled"": true,
            ""IsAutoUpdaterBetaReleaseChannel"": false,
            ""SelectedActionIndex"": 0,
            ""AppVersion"": {
                ""major"": 0,
                ""minor"": 9,
                ""patch"": 5
            },
            ""Sources"": []
        }";

        // Act
        var result = migration.PerformConfigMigration(config);

        // Assert
        var jsonDoc = JsonDocument.Parse(result);
        var appVersion = jsonDoc.RootElement.GetProperty("AppVersion").GetString();
        Assert.That(appVersion, Is.EqualTo("1.0.0"));
    }

    [Test]
    public void GivenJsonWithoutAppVersion_WhenPerformConfigMigration_ThenAddsAppVersionAsString()
    {
        // Arrange
        var migration = new TestMigration_2_3_1();
        var config = @"{
            ""IsAutoUpdaterEnabled"": true,
            ""IsAutoUpdaterBetaReleaseChannel"": false,
            ""SelectedActionIndex"": 0,
            ""Sources"": []
        }";

        // Act
        var result = migration.PerformConfigMigration(config);

        // Assert
        var jsonDoc = JsonDocument.Parse(result);
        var appVersion = jsonDoc.RootElement.GetProperty("AppVersion").GetString();
        Assert.That(appVersion, Is.EqualTo("2.3.1"));
    }

    [Test]
    public void GivenJsonWithStringAppVersion_WhenPerformConfigMigration_ThenUpdatesAppVersionString()
    {
        // Arrange
        var migration = new TestMigration_3_0_0();
        var config = @"{
            ""IsAutoUpdaterEnabled"": true,
            ""IsAutoUpdaterBetaReleaseChannel"": false,
            ""SelectedActionIndex"": 0,
            ""AppVersion"": ""1.5.2"",
            ""Sources"": []
        }";

        // Act
        var result = migration.PerformConfigMigration(config);

        // Assert
        var jsonDoc = JsonDocument.Parse(result);
        var appVersion = jsonDoc.RootElement.GetProperty("AppVersion").GetString();
        Assert.That(appVersion, Is.EqualTo("3.0.0"));
    }

    [Test]
    public void GivenComplexJsonWithSources_WhenPerformConfigMigration_ThenPreservesOtherProperties()
    {
        // Arrange
        var migration = new TestMigration_2_0_0();
        var config = @"{
            ""IsAutoUpdaterEnabled"": true,
            ""IsAutoUpdaterBetaReleaseChannel"": true,
            ""SelectedActionIndex"": 2,
            ""AppVersion"": ""0.0.1"",
            ""Sources"": [
                {
                    ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""Name"": ""Test Source"",
                    ""ProviderType"": ""RabbitMQ"",
                    ""Settings"": [
                        {
                            ""PropertyName"": ""Host"",
                            ""Value"": ""localhost""
                        },
                        {
                            ""PropertyName"": ""Port"",
                            ""Value"": 5672
                        }
                    ]
                }
            ]
        }";

        // Act
        var result = migration.PerformConfigMigration(config);

        // Assert
        var jsonDoc = JsonDocument.Parse(result);

        // Check AppVersion was updated
        var appVersion = jsonDoc.RootElement.GetProperty("AppVersion").GetString();
        Assert.That(appVersion, Is.EqualTo("2.0.0"));

        // Check other properties are preserved
        Assert.That(jsonDoc.RootElement.GetProperty("IsAutoUpdaterEnabled").GetBoolean(), Is.True);
        Assert.That(jsonDoc.RootElement.GetProperty("IsAutoUpdaterBetaReleaseChannel").GetBoolean(), Is.True);
        Assert.That(jsonDoc.RootElement.GetProperty("SelectedActionIndex").GetInt32(), Is.EqualTo(2));

        // Check Sources array is preserved
        var sources = jsonDoc.RootElement.GetProperty("Sources");
        Assert.That(sources.GetArrayLength(), Is.EqualTo(1));

        var source = sources[0];
        Assert.That(source.GetProperty("Name").GetString(), Is.EqualTo("Test Source"));
        Assert.That(source.GetProperty("ProviderType").GetString(), Is.EqualTo("RabbitMQ"));

        var settings = source.GetProperty("Settings");
        Assert.That(settings.GetArrayLength(), Is.EqualTo(2));

        var hostSetting = settings[0];
        Assert.That(hostSetting.GetProperty("PropertyName").GetString(), Is.EqualTo("Host"));
        Assert.That(hostSetting.GetProperty("Value").GetString(), Is.EqualTo("localhost"));

        var portSetting = settings[1];
        Assert.That(portSetting.GetProperty("PropertyName").GetString(), Is.EqualTo("Port"));
        Assert.That(portSetting.GetProperty("Value").GetInt32(), Is.EqualTo(5672));
    }

    [Test]
    public void GivenInvalidJson_WhenPerformConfigMigration_ThenReturnsOriginalConfig()
    {
        // Arrange
        var migration = new TestMigration_1_0_0();
        var config = "This is not valid JSON";

        // Act
        var result = migration.PerformConfigMigration(config);

        // Assert
        Assert.That(result, Is.EqualTo(config));
    }

    [Test]
    public void GivenMigrationWithCustomMigrateConfig_WhenPerformConfigMigration_ThenAppliesMigrateConfigFirst()
    {
        // Arrange
        var migration = new TestMigrationWithCustomMigrateConfig();
        var config = @"{
            ""IsAutoUpdaterEnabled"": true,
            ""OldProperty"": ""value"",
            ""AppVersion"": ""1.0.0"",
            ""Sources"": []
        }";

        // Act
        var result = migration.PerformConfigMigration(config);

        // Assert
        var jsonDoc = JsonDocument.Parse(result);

        // Check AppVersion was updated
        var appVersion = jsonDoc.RootElement.GetProperty("AppVersion").GetString();
        Assert.That(appVersion, Is.EqualTo("2.5.0"));

        // Check custom migration was applied (OldProperty renamed to NewProperty)
        Assert.That(jsonDoc.RootElement.TryGetProperty("OldProperty", out _), Is.False);
        Assert.That(jsonDoc.RootElement.GetProperty("NewProperty").GetString(), Is.EqualTo("value"));
    }

    #region Test Migration Implementations

    private class TestMigration_1_0_0 : MigrationBase
    {
        public override (int major, int minor, int patch) AppVersion => (1, 0, 0);
        public override bool ClearAllProviders => false;
        public override bool KeepOnlyLatestProviderVersion => false;
        public override Func<string, string>? MigrateConfig => null;
        public override IPrerequisite[] Prerequisites { get; init; } = [];
    }

    private class TestMigration_2_0_0 : MigrationBase
    {
        public override (int major, int minor, int patch) AppVersion => (2, 0, 0);
        public override bool ClearAllProviders => true;
        public override bool KeepOnlyLatestProviderVersion => false;
        public override Func<string, string>? MigrateConfig => null;
        public override IPrerequisite[] Prerequisites { get; init; } = [];
    }

    private class TestMigration_2_3_1 : MigrationBase
    {
        public override (int major, int minor, int patch) AppVersion => (2, 3, 1);
        public override bool ClearAllProviders => false;
        public override bool KeepOnlyLatestProviderVersion => true;
        public override Func<string, string>? MigrateConfig => null;
        public override IPrerequisite[] Prerequisites { get; init; } = [];
    }

    private class TestMigration_3_0_0 : MigrationBase
    {
        public override (int major, int minor, int patch) AppVersion => (3, 0, 0);
        public override bool ClearAllProviders => true;
        public override bool KeepOnlyLatestProviderVersion => true;
        public override Func<string, string>? MigrateConfig => null;
        public override IPrerequisite[] Prerequisites { get; init; } = [];
    }

    private class TestMigrationWithCustomMigrateConfig : MigrationBase
    {
        public override (int major, int minor, int patch) AppVersion => (2, 5, 0);
        public override bool ClearAllProviders => false;
        public override bool KeepOnlyLatestProviderVersion => false;

        public override Func<string, string>? MigrateConfig => json =>
        {
            // Replace "OldProperty" with "NewProperty"
            return json.Replace("\"OldProperty\":", "\"NewProperty\":");
        };

        public override IPrerequisite[] Prerequisites { get; init; } = [];
    }

    #endregion
}

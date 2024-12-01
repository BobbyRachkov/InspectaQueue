using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.Abstractions.Attributes;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models.Modifiers;
using System.Reflection;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public class SettingsManager : ISettingsManager
{
    public IEnumerable<ISettingPack> ExtractSettings(IQueueProvider queueProvider)
    {
        var exposedProperties = GetExposedProperties(queueProvider.Settings.GetType());

        foreach (var property in exposedProperties)
        {
            yield return PackExposedProperty(property, property.GetValue(queueProvider.Settings));
        }
    }

    public IEnumerable<ISettingPack> MergePacks(
        IEnumerable<ISettingPack> @base,
        IEnumerable<ISettingPack> overriding)

    {
        return MergePacks(@base, overriding.Select(x => new SettingDetachedPack
        {
            PropertyName = x.PropertyName,
            Value = x.Value
        }));
    }

    public IEnumerable<ISettingPack> MergePacks(
        IEnumerable<ISettingPack> @base,
        IEnumerable<SettingDetachedPack> overriding)

    {
        var baseList = @base.ToList();

        foreach (var overridingPack in overriding)
        {
            var correspondingProp = baseList.FirstOrDefault(x => x.PropertyName == overridingPack.PropertyName);

            if (correspondingProp is not null)
            {
                correspondingProp.Value = overridingPack.Value;

                //For backwards compatibility; remove after few versions
                if (correspondingProp is { PropertyName: "HideMessagesAfter" }
                    && correspondingProp.Value?.ToString() == "0")
                {
                    correspondingProp.Value = 1000;
                }
            }
        }

        return baseList;
    }

    private static PropertyInfo[] GetExposedProperties(Type providerSettingsType)
    {
        return providerSettingsType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x =>
                x.CustomAttributes.Any(a => a.AttributeType == typeof(ExposedAttribute))
                && x.GetSetMethod(false) is not null)
            .ToArray();
    }

    private static ISettingPack PackExposedProperty(PropertyInfo property, object? value)
    {
        var exposedAttribute = (ExposedAttribute)
            property
                .GetCustomAttributes(typeof(ExposedAttribute))
                .First();

        var modifiers = GetModifiers(property);

        if (property.PropertyType.IsEnum)
        {
            return HandleEnum(property, exposedAttribute, modifiers, value);
        }

        return new BasicSettingPack
        {
            ReflectedProperty = property,
            Name = exposedAttribute.DisplayName ?? property.Name,
            ToolTip = exposedAttribute.ToolTip,
            Type = property.PropertyType,
            PropertyName = property.Name,
            Value = value,
            Modifiers = modifiers
        };
    }

    private static Modifiers GetModifiers(PropertyInfo property)
    {
        var exposedAttribute = property.GetCustomAttribute<ExposedAttribute>();
        SecretModifier? secretModifier = null;
        FilePathModifier? filePathModifier = null;

        var secretAttribute = property.GetCustomAttribute<SecretAttribute>();
        if (secretAttribute is not null)
        {
            secretModifier = new SecretModifier
            {
                CanBeRevealed = secretAttribute.CanBeRevealed,
                PasswordChar = secretAttribute.PasswordChar
            };
        }

        var filePathAttribute = property.GetCustomAttribute<FilePathAttribute>();
        if (filePathAttribute is not null)
        {
            filePathModifier = new FilePathModifier
            {
                Filter = filePathAttribute.AllowedExtensions ?? "All files (*.*)|*.*",
                Title = filePathAttribute.Title ?? $"Select {exposedAttribute?.DisplayName ?? property.Name}"
            };
        }

        return new Modifiers
        {
            FilePath = filePathModifier,
            Secret = secretModifier
        };
    }

    private static ISettingPack HandleEnum(PropertyInfo property, ExposedAttribute exposedAttribute, Modifiers modifiers, object? value)
    {
        var enumType = property.PropertyType;
        var isFlags = enumType.GetCustomAttributes<FlagsAttribute>().Any();

        var exposedEnums = enumType.GetMembers()
            .Where(x => x.DeclaringType == enumType
                        && x.GetCustomAttributes(typeof(ExposedAttribute)).Count() == 1);

        var options = exposedEnums.Select(x => new MultipleChoiceEntry
        {
            DisplayName = x.GetCustomAttributes<ExposedAttribute>().Single().DisplayName ?? x.Name,
            Value = Enum.Parse(enumType, x.Name)
        });

        return new MultipleChoiceSettingPack()
        {
            ReflectedProperty = property,
            Name = exposedAttribute.DisplayName ?? property.Name,
            ToolTip = exposedAttribute.ToolTip,
            Type = property.PropertyType,
            PropertyName = property.Name,
            Value = value,
            Options = options.ToArray(),
            MultipleSelectionEnabled = isFlags,
            Modifiers = modifiers
        };
    }

    public IEnumerable<ISettingPack> EnsureCorrectTypes(IEnumerable<ISettingPack> settings)
    {
        foreach (var setting in settings)
        {
            if (setting.Value is null
                || setting.Value.GetType() == setting.ReflectedProperty.PropertyType)
            {
                yield return setting;
                continue;
            }

            if (setting.ReflectedProperty.PropertyType.IsEnum)
            {
                Enum.TryParse(setting.ReflectedProperty.PropertyType, setting.Value.ToString(), true, out var convertedValue);
                setting.Value = convertedValue;
            }

            if (typeof(int) == setting.ReflectedProperty.PropertyType)
            {
                if (!int.TryParse(setting.Value?.ToString(), out var convertedValue))
                {
                    convertedValue = Convert.ToInt32(setting.Value);
                }

                setting.Value = convertedValue;
            }

            if (typeof(double) == setting.ReflectedProperty.PropertyType)
            {
                double.TryParse(setting.Value?.ToString(), out var convertedValue);
                setting.Value = convertedValue;
            }

            if (typeof(decimal) == setting.ReflectedProperty.PropertyType)
            {
                decimal.TryParse(setting.Value?.ToString(), out var convertedValue);
                setting.Value = convertedValue;
            }

            yield return setting;
        }
    }
}
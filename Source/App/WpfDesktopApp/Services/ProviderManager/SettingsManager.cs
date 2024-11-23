using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.Abstractions.Attributes;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;
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

        if (property.PropertyType.IsEnum)
        {
            return HandleEnum(property, exposedAttribute, value);
        }

        return new BasicSettingPack
        {
            ReflectedProperty = property,
            Name = exposedAttribute.DisplayName ?? property.Name,
            ToolTip = exposedAttribute.ToolTip,
            Type = property.PropertyType,
            PropertyName = property.Name,
            Value = value
        };
    }

    private static ISettingPack HandleEnum(PropertyInfo property, ExposedAttribute exposedAttribute, object? value)
    {
        var enumType = property.PropertyType;
        var isFlags = enumType.GetCustomAttributes<FlagsAttribute>().Any();
        var options = enumType.GetEnumNames().Select(x => new MultipleChoiceEntry
        {
            DisplayName = x,
            Value = Enum.Parse(enumType, x)
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
            MultipleSelectionEnabled = isFlags
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
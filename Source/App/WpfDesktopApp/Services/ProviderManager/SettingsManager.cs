using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.Abstractions.Attributes;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;
using System.Reflection;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public class SettingsManager : ISettingsManager
{
    public IEnumerable<SettingPack> ExtractSettings(IQueueProvider queueProvider)
    {
        var exposedProperties = GetExposedProperties(queueProvider.Settings.GetType());

        foreach (var property in exposedProperties)
        {
            yield return PackExposedProperty(property, property.GetValue(queueProvider.Settings));
        }
    }

    public IEnumerable<SettingPack> MergePacks(
        IEnumerable<SettingPack> @base,
        IEnumerable<SettingPack> overriding)

    {
        return MergePacks(@base, overriding.Select(x => new SettingDetachedPack
        {
            PropertyName = x.PropertyName,
            Value = x.Value
        }));
    }

    public IEnumerable<SettingPack> MergePacks(
        IEnumerable<SettingPack> @base,
        IEnumerable<SettingDetachedPack> overriding)

    {
        var baseList = @base.ToList();

        foreach (var overridingPack in overriding)
        {
            var correspondingProp = baseList.FirstOrDefault(x => x.PropertyName == overridingPack.PropertyName);

            if (correspondingProp is not null)
            {
                correspondingProp.Value = overridingPack.Value;
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

    private static SettingPack PackExposedProperty(PropertyInfo property, object? value)
    {
        var exposedAttribute = (ExposedAttribute)
            property
                .GetCustomAttributes(typeof(ExposedAttribute))
                .First();

        return new SettingPack
        {
            ReflectedProperty = property,
            Name = exposedAttribute.DisplayName ?? property.Name,
            ToolTip = exposedAttribute.ToolTip,
            Type = property.PropertyType,
            PropertyName = property.Name,
            Value = value
        };
    }
}
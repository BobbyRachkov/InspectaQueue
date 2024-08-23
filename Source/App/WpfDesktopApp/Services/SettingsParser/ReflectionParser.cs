using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.Abstractions.Attributes;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using System.Reflection;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.SettingsParser;

public class ReflectionParser : ISettingsParser
{
    public IEnumerable<SettingEntryViewModel> ParseMembers(IQueueProvider queueProvider)
    {
        var instance = queueProvider.Settings;
        var providerSettingsType = queueProvider.Settings.GetType();

        var exposedProperties = providerSettingsType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CustomAttributes.Any(a => a.AttributeType == typeof(ExposedAttribute))
                        && x.GetSetMethod(false) is not null)
            .ToArray();

        foreach (var property in exposedProperties)
        {
            var exposedAttribute = (ExposedAttribute)
                property
                    .GetCustomAttributes(typeof(ExposedAttribute))
                    .First();

            yield return new SettingEntryViewModel
            {
                ReflectedProperty = property,
                Name = exposedAttribute.DisplayName ?? property.Name,
                ToolTip = exposedAttribute.ToolTip,
                Type = property.PropertyType,
                PropertyName = property.Name,
                Value = property.GetValue(instance)
            };
        }
    }
}
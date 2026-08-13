using System.Collections.Generic;

// <summary>
// Interface for a settings category provider, which defines a category of settings and provides the settings rows for that category.
// </summary>
public interface ISettingsCategoryProvider
{
    string CategoryName { get; }
    List<ISettingRow> BuildSettings();
}
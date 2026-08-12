using System.Collections.Generic;

public interface ISettingsCategoryProvider
{
    string CategoryName { get; }
    List<ISettingRow> BuildSettings();
}

using Shimakaze.Models.Ini;

namespace Shimakaze.MSBuild.Model;

internal class UnitSection : IniSection
{
    public UnitSection(string name) : base(name)
    {
        Types = Array.Empty<(string, string)>();
    }

    public (string Type, string Key)[] Types { get; set; }
}

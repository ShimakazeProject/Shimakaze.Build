using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

using Shimakaze.Models.Ini;

namespace Shimakaze.MSBuild.Model;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class UnitIniDocument : DynamicObject, IEnumerable<UnitSection>, IEnumerable
{
    private readonly Dictionary<string, UnitSection> sections = new();

    public ICollection<UnitSection> Sections => sections.Values;

    public UnitSection this[string section]
    {
        get => sections[section];
        set => sections[section] = value;
    }

    public IniValue this[string section, string key]
    {
        get => sections[section][key];
        set
        {
            if (!sections.ContainsKey(section))
                Add(section)[key] = value;
            else
                sections[section][key] = value;
        }
    }
#pragma warning disable CA1822 // Mark members as static
    public UnitSection Default => new("Default");
#pragma warning restore CA1822 // Mark members as static
    public int Count => sections.Count;

    internal UnitIniDocument(Dictionary<string, UnitSection> sections) => this.sections = sections;

    public UnitIniDocument(IEnumerable<UnitSection> sections)
        : this(sections.ToDictionary((UnitSection x) => x.Name))
    {
    }

    public UnitIniDocument()
    {
    }

    public override string ToString() => string.Format("{0}: {1}", "IniDocument", Count);

    public void Add(UnitSection section) => sections.Add(section.Name, section);

    public UnitSection Add(string section)
    {
        UnitSection UnitSection = new(section);
        Add(UnitSection);
        return UnitSection;
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out UnitSection value) => sections.TryGetValue(key, out value);

    public bool Remove(string section) => sections.Remove(section);

    public IEnumerator<UnitSection> GetEnumerator() => Sections.GetEnumerator();

    private string GetDebuggerDisplay() => ToString();

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        string name = binder.Name;
        bool result2 = TryGetValue(name, out var value);
        result = value;
        return result2;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        UnitSection? UnitSection = value as UnitSection;
        if (UnitSection != null)
        {
            this[binder.Name] = UnitSection;
            return true;
        }
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Sections).GetEnumerator();
}


using System.Reflection.PortableExecutable;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Build.Tasks.Extensions;
using Shimakaze.Build.Tasks.Services.Ini;
using Shimakaze.Models.Ini;

namespace Shimakaze.Build.Tasks;

public class IniMergeTask : Microsoft.Build.Utilities.Task
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    [Required]
    public string Sources { get; set; }

    [Required]
    public string TargetName { get; set; }

    [Required]
    public string OutputPath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

    public override bool Execute()
    {
        Log.LogMessage("Starting {0}", nameof(IniMergeTask));
        if (string.IsNullOrEmpty(Sources))
        {
            Log.LogError("Sources is null or empty");
            return false;
        }
        var files = Sources.Split(';').Select(x => new FileInfo(x)).ToList();

        List<IniSection> output = new();
        Dictionary<string, IniSection> types = new();

        files.ForEach(x =>
        {
            Log.LogMessage("Parsing \"{0}\"", x.FullName);
            using var reader = x.OpenText();
            var ini = UnitIniSerializer.Deserialize(reader);
            ini.Sections.Each(x =>
            {
                // regist section.
                x.Types.Each(y =>
                {
                    if (!types.ContainsKey(y.Type))
                    {
                        Log.LogMessage("New Regist Section {0}", y.Type);
                        types.Add(y.Type, new(y.Type));
                    }
                    types[y.Type].Add(y.Key, x.Name);
                });

                // add section.
                output.Add(x);
            });
        });

        IniDocument ini = new();
        // merge types.
        types.OrderBy(x => x.Key)
            .Select(x => (Section: x.Value, Output: ini.Add(x.Value.Name)))
            .Each(x => x.Section.Datas
                .OrderBy(x => (string)x.Value)
                .OrderBy(x => x.Key)
                .Each(x.Output.Add)
            );
        output.Each(ini.Add);

        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        using var fs = File.Create(Path.Combine(OutputPath, TargetName + ".ini"));
        using StreamWriter sw = new(fs);
        Models.Ini.Serialization.IniSerializer.Serialize(ini, sw);
        fs.Flush();

        Log.LogMessage(MessageImportance.High, $"Successful build \"{Path.GetFullPath(Path.Combine(OutputPath, $"{TargetName}.ini"))}\"");

        return true;
    }
}
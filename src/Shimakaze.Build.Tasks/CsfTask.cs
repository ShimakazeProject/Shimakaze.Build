using Microsoft.Build.Framework;

using Shimakaze.Build.Tasks.Extensions;
using Shimakaze.Build.Tasks.Services.Csf;
using Shimakaze.Models.Csf;
using Shimakaze.Tools.Csf.Serialization.Csf;

namespace Shimakaze.Build.Tasks;

public class CsfTask : Microsoft.Build.Utilities.Task
{
    public string? Sources { get; set; }
    public string? TargetName { get; set; }
    public string? OutputPath { get; set; }
    public string? Version { get; set; }
    public string? Language { get; set; }
    public string? Unknown { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrEmpty(Sources))
            Log.LogMessage("{0} is empty, Skip{1}", nameof(Sources), nameof(CsfTask));
        else if (string.IsNullOrEmpty(TargetName))
            Log.LogMessage("{0} is empty, Skip{1}", nameof(TargetName), nameof(CsfTask));
        else if (string.IsNullOrEmpty(OutputPath))
            Log.LogMessage("{0} is empty, Skip{1}", nameof(OutputPath), nameof(CsfTask));
        else
        {
            Log.LogMessage("Starting {0}", nameof(CsfTask));

            var files = Sources.Split(';').Select(x => new FileInfo(x));

            Dictionary<string, CsfLabel> labels = new();

            files.GroupBy(x => x.Extension).Each(x =>
            {
                GetCsfLabels GetCsfLabels = x.Key switch
                {
                    ".csf" => CsfConverter.GetCsfLabels,
                    ".xml" => XmlConverter.GetCsfLabels,
                    ".json" => JsonConverter.GetCsfLabels,
                    _ => throw new NotSupportedException($"{x.Key} is not supported."),
                };
                x.Each(y => y.OpenRead().Use(fs => GetCsfLabels(fs, labels).Each(x => Log.LogWarning("{0}: {1}", y.FullName, x))));
            });

            CsfHead head = new()
            {
                LabelCount = labels.Count,
                StringCount = labels.Values.Select((CsfLabel x) => x.Values.Length).Sum(),
                Version = int.TryParse(Version, out int version) ? version : 3,
                Language = int.TryParse(Language, out int language) ? language : 0,
                Unknown = int.TryParse(Unknown, out int unknown) ? unknown : 0,
            };

            var bytes = new CsfStructSerializer().Serialize(new()
            {
                Head = head,
                Datas = labels.Values.ToArray(),
            });

            Log.LogMessage("Label Count: {0}", head.LabelCount);
            Log.LogMessage("String Count: {0}", head.StringCount);

            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);

            File.WriteAllBytes(Path.Combine(OutputPath, TargetName + ".csf"), bytes);

            Log.LogMessage(MessageImportance.High, $"Successful build \"{Path.GetFullPath(Path.Combine(OutputPath, $"{TargetName}.csf"))}\"");
        }

        return true;
    }

}

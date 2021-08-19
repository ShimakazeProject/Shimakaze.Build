using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Models.Csf;
using Shimakaze.MSBuild.Model;
using Shimakaze.Tools.Csf.Serialization.Csf;

using V1 = Shimakaze.Tools.Csf.Serialization.Json.V1;
using V2 = Shimakaze.Tools.Csf.Serialization.Json.V2;

namespace Shimakaze.MSBuild;

public class CsfJsonTask : Microsoft.Build.Utilities.Task
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
        Log.LogMessage("Starting {0}", nameof(CsfJsonTask));
        if (string.IsNullOrEmpty(Sources))
        {
            Log.LogError("Sources is null or empty");
            return false;
        }
        var files = Sources.Split(';').Select(x => new FileInfo(x)).ToList();

        CsfHead head = new();
        List<CsfLabel> data = new();

        files.ForEach(x =>
        {
            using var fs = x.OpenRead();
            var protocol = JsonSerializer.Deserialize<JsonProtocol>(fs)?.Protocol;
            fs.Seek(0, SeekOrigin.Begin);

            Log.LogMessage("Parsing: \"{0}\"", x.FullName);
            Log.LogMessage("Protocol: {0}", protocol);

            if (protocol switch
            {
                1 => JsonSerializer.Deserialize<CsfStruct>(fs, V1::CsfJsonConverterUtils.CsfJsonSerializerOptions),
                2 => JsonSerializer.Deserialize<CsfStruct>(fs, V2::CsfJsonConverterUtils.CsfJsonSerializerOptions),
                _ => default,
            } is CsfStruct _csf)
            {
                head = _csf.Head;
                data.AddRange(_csf.Datas);
            }
            else
                Log.LogWarning("Cannot Load File \"{0}\"", x.FullName);
        });
        head.LabelCount = data.Count;
        head.StringCount = data.Select((CsfLabel x) => x.Values.Length).Sum();

        var bytes = new CsfStructSerializer().Serialize(new()
        {
            Head = head,
            Datas = data.ToArray(),
        });

        Log.LogMessage("Label Count: {0}", head.LabelCount);
        Log.LogMessage("String Count: {0}", head.StringCount);

        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        File.WriteAllBytes(Path.Combine(OutputPath, TargetName + ".csf"), bytes);

        Log.LogMessage(MessageImportance.High, $"Successful build \"{Path.GetFullPath(Path.Combine(OutputPath, $"{TargetName}.csf"))}\"");
        return true;
    }

}

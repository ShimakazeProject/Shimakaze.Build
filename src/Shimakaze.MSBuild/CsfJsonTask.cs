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
    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.High, "Executing CsfJsonV2Task");
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
            Log.LogMessage(MessageImportance.Low, "Compiling {0}", x.FullName);
            CsfStruct? _csf = default;
            using var fs = x.OpenRead();

            var protocol = JsonSerializer.Deserialize<JsonProtocol>(fs)?.Protocol;

            switch (protocol)
            {
                case 1:
                    _csf = JsonSerializer.Deserialize<CsfStruct>(fs, V1::CsfJsonConverterUtils.CsfJsonSerializerOptions);
                    break;
                case 2:
                    _csf = JsonSerializer.Deserialize<CsfStruct>(fs, V2::CsfJsonConverterUtils.CsfJsonSerializerOptions);
                    break;
            }

            if (_csf is null)
            {
                Log.LogError("Cannot Complet {0}", x.FullName);
                throw new();
            }

            head = _csf.Head;
            data.AddRange(_csf.Datas);

        });
        head.LabelCount = data.Count;
        head.StringCount = data.Select((CsfLabel x) => x.Values.Length).Sum();

        var bytes = new CsfStructSerializer().Serialize(new()
        {
            Head = head,
            Datas = data.ToArray(),
        });

        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        File.WriteAllBytes(Path.Combine(OutputPath, CsfName + ".csf"), bytes);

        Log.LogMessage(MessageImportance.High, "Completed CsfJsonV2Task");
        return true;
    }

#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    [Required]
    public string Sources { get; set; }

    [Required]
    public string CsfName { get; set; }

    [Required]
    public string OutputPath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}

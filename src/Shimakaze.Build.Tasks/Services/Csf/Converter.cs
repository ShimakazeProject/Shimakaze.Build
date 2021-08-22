using Shimakaze.Models.Csf;

namespace Shimakaze.Build.Tasks.Services.Csf;

delegate string[] GetCsfLabels(Stream stream, Dictionary<string, CsfLabel> labels);
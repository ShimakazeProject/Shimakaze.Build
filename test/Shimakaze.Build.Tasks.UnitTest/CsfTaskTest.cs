using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shimakaze.Models.Csf;

namespace Shimakaze.Build.Tasks.UnitTest;

[TestClass]
public class CsfTaskTest
{
    [TestMethod]
    public void TestXmlV1()
    {
        using var fs = File.OpenRead(Path.Combine("Assets", "v1.xml"));
        Dictionary<string, CsfLabel> labels = new();
        var assembly = typeof(CsfTask).Assembly;
        var type = assembly.GetType("Shimakaze.Build.Tasks.Services.Csf.XmlConverter");
        var method = type!.GetMethod("GetCsfLabels");

        foreach (var s in (string[])method!.Invoke(null, new object[] { fs, labels }))
            Console.WriteLine(s);
    }
}


using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shimakaze.Models.Csf;

namespace Shimakaze.Build.Tasks.UnitTest;

[TestClass]
public class I18nCsfTaskTest
{
    [TestMethod]
    public void TestGenerate()
    {
        using var fs = File.OpenRead(Path.Combine("Assets", "v1.xml"));
        Dictionary<string, CsfLabel> labels = new();
        var assembly = typeof(CsfTask).Assembly;
        var type = assembly.GetType("Shimakaze.Build.Tasks.Services.Csf.XmlConverter");
        var method = type!.GetMethod("GetCsfLabels");

        foreach (var s in (string[])method!.Invoke(null, new object[] { fs, labels }))
            Console.WriteLine(s);

        var xml = Services.I18n.Generator.GeneratI18nDocument(labels.Values.ToArray());

        if (!Directory.Exists("I18n"))
            Directory.CreateDirectory("I18n");
        xml.Save(Path.Combine("I18n", "gen.i18n"));
    }
    
    [TestMethod]
    public void TestAppend()
    {
        using var fs = File.OpenRead(Path.Combine("Assets", "v1.xml"));
        Dictionary<string, CsfLabel> labels = new();
        var assembly = typeof(CsfTask).Assembly;
        var type = assembly.GetType("Shimakaze.Build.Tasks.Services.Csf.XmlConverter");
        var method = type!.GetMethod("GetCsfLabels");

        foreach (var s in (string[])method!.Invoke(null, new object[] { fs, labels }))
            Console.WriteLine(s);

        XmlDocument xml = new();
        xml.Load(Path.Combine("Assets", "v1.i18n"));

        Services.I18n.Generator.AppendI18nDocument(labels.Values.ToArray(), xml);

        if (!Directory.Exists("I18n"))
            Directory.CreateDirectory("I18n");
        xml.Save(Path.Combine("I18n", "append.i18n"));
    }
}


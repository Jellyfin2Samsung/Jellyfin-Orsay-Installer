using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.Services;

public sealed class OrsayPackager : IOrsayPackager
{
    private readonly string _templateRoot;

    public OrsayPackager()
    {
        _templateRoot = Path.Combine(AppContext.BaseDirectory, "Template");
    }

    public string GetDefaultOutputPath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "assets",
            "orsay-output"
        );
    }

    public Result<PackageResult> BuildWidget(string outputRoot, string appName, string localIp, int port)
    {
        try
        {
            // Validate template exists
            var jellyfinTemplatePath = Path.Combine(_templateRoot, "Jellyfin");
            if (!Directory.Exists(jellyfinTemplatePath))
            {
                return Result<PackageResult>.Failure($"Template directory not found: {jellyfinTemplatePath}");
            }

            // Clean output directory
            if (Directory.Exists(outputRoot))
                Directory.Delete(outputRoot, true);
            Directory.CreateDirectory(outputRoot);

            // Create temp widget folder
            var tempWidget = Path.Combine(outputRoot, "_temp_widget");
            Directory.CreateDirectory(tempWidget);

            // Copy template files
            CopyDir(jellyfinTemplatePath, tempWidget);

            // Extract version from config.xml
            var version = ExtractVersion(Path.Combine(tempWidget, "config.xml"));

            // Remove config.xml.template if exists
            var templateFile = Path.Combine(tempWidget, "config.xml.template");
            if (File.Exists(templateFile))
                File.Delete(templateFile);

            // Generate widget ID and ZIP filename
            var dateStamp = DateTime.Now.ToString("yyyyMMdd");
            var widgetId = $"{appName}_{dateStamp}";
            var zipFileName = $"{appName}_v{version}_{dateStamp}.zip";
            var zipPath = Path.Combine(outputRoot, zipFileName);

            // Create ZIP archive
            ZipFile.CreateFromDirectory(tempWidget, zipPath, CompressionLevel.Optimal, false);

            // Get ZIP size
            var zipSize = new FileInfo(zipPath).Length;

            // Clean up temp folder
            Directory.Delete(tempWidget, true);

            // Generate download URL
            var downloadUrl = port == 80
                ? $"http://{localIp}/{zipFileName}"
                : $"http://{localIp}:{port}/{zipFileName}";

            // Generate widgetlist.xml in Samsung Orsay format
            var widgetlistXml = $"""
<?xml version="1.0" encoding="utf-8"?>
<rsp stat="ok">
<list>
<widget id="{widgetId}">
    <title>{widgetId}</title>
    <compression size="{zipSize}" type="zip" />
    <description>Jellyfin Media Player for Samsung Orsay Smart TVs</description>
    <download>{downloadUrl}</download>
</widget>
</list>
</rsp>
""";

            File.WriteAllText(
                Path.Combine(outputRoot, "widgetlist.xml"),
                widgetlistXml,
                new UTF8Encoding(false) // UTF-8 without BOM
            );

            return Result<PackageResult>.Success(new PackageResult
            {
                ZipFileName = zipFileName,
                ZipSize = zipSize,
                WidgetId = widgetId,
                OutputPath = outputRoot,
                DownloadUrl = downloadUrl
            });
        }
        catch (Exception ex)
        {
            return Result<PackageResult>.Failure($"Failed to build widget: {ex.Message}");
        }
    }

    private static string ExtractVersion(string configXmlPath)
    {
        try
        {
            var doc = XDocument.Load(configXmlPath);
            var verElement = doc.Root?.Element("ver");
            if (verElement != null)
                return verElement.Value;
        }
        catch
        {
            // Fallback if parsing fails
        }
        return "1.0.0";
    }

    private static void CopyDir(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var f in Directory.GetFiles(src))
            File.Copy(f, Path.Combine(dst, Path.GetFileName(f)), true);
        foreach (var d in Directory.GetDirectories(src))
            CopyDir(d, Path.Combine(dst, Path.GetFileName(d)));
    }
}

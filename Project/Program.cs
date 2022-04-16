using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using asardotnetasync;

namespace CrackUnityHub
{
    class Program
    {
        static ProgressBar progressBar;

        static async Task Main(string[] args)
        {
            var processes = Process.GetProcessesByName("Unity Hub");
            if (processes.Length > 0)
            {
                LogAndPause("请结束Unity Hub后再试试.");
                return;
            }

            Console.WriteLine($"请输入UnityHub目录的绝对路径：");
            var unityHubPath = Console.ReadLine();
            unityHubPath = unityHubPath.Trim('"');

            var unityHubVersion = string.Empty;
            try
            {
                var version = FileVersionInfo.GetVersionInfo(Path.Combine(unityHubPath, "Unity Hub.exe"));
                unityHubVersion = version.ProductVersion;
            }
            catch
            { }

            if (string.IsNullOrEmpty(unityHubVersion))
            {
                LogAndPause($"不是 UnityHub 目录.");
                return;
            }

            unityHubPath = Path.Combine(unityHubPath, "resources");
            var exportFolder = Path.Combine(unityHubPath, "app");
            var asarPath = Path.Combine(unityHubPath, "app.asar");
            var asarUnpackPath = Path.Combine(unityHubPath, "app.asar.unpacked");

            if (Directory.Exists(exportFolder) || !File.Exists(asarPath))
            {
                Console.WriteLine("UnityHub已经被破解了.是否要去除破解？(y/n)");
                var input = Console.ReadLine().Trim();
                if (string.Compare(input, "y", true) == 0)
                {
                    Directory.Delete(exportFolder, true);
                    File.Move(asarPath + ".bak", asarPath);
                    LogAndPause("去除破解完成.");
                }
                return;
            }

            var success = true;
            Console.WriteLine("开始破解");

            try
            {
                Directory.CreateDirectory(exportFolder);

                var archive = new AsarArchive(asarPath);
                var extractor = new AsarExtractor();
                progressBar = new ProgressBar();

                extractor.FileExtracted += (sender, e) => progressBar.Report(e.Progress);
                await extractor.ExtractAll(archive, exportFolder + "/");

                var patcher = PatcherManager.GetPatcher(unityHubVersion);
                success = patcher?.Patch(exportFolder) ?? false;

                if (success)
                {
                    if (Directory.Exists(asarUnpackPath))
                        CopyDirectory(asarUnpackPath, exportFolder, true);

                    File.Move(asarPath, asarPath + ".bak");
                }
                else
                {
                    throw new Exception($"当前版本不能破解. ver: {unityHubVersion}");
                }
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex);
                Directory.Delete(exportFolder, true);
            }
            finally
            {
                Console.WriteLine();
                progressBar.Dispose();
                LogAndPause(success ? "破解完成" : "破解失败");
            }
        }

        static void LogAndPause(string msg)
        {
            Console.WriteLine(msg);
            Console.ReadKey();
        }

        static bool CopyDirectory(string sourcePath, string destinationPath, bool overwrite)
        {
            var ret = true;
            try
            {
                sourcePath = sourcePath.EndsWith(@"\") ? sourcePath : sourcePath + @"\";
                destinationPath = destinationPath.EndsWith(@"\") ? destinationPath : destinationPath + @"\";

                if (Directory.Exists(sourcePath))
                {
                    if (!Directory.Exists(destinationPath))
                        Directory.CreateDirectory(destinationPath);

                    foreach (var filePath in Directory.GetFiles(sourcePath))
                    {
                        var file = new FileInfo(filePath);
                        file.CopyTo(destinationPath + file.Name, overwrite);
                    }
                    foreach (var directoryPath in Directory.GetDirectories(sourcePath))
                    {
                        var directory = new DirectoryInfo(directoryPath);
                        if (!CopyDirectory(directoryPath, destinationPath + directory.Name, overwrite))
                            ret = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ret = false;
                Console.WriteLine(ex);
            }

            return ret;
        }
    }
}

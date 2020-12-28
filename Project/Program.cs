using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using asardotnetasync;

namespace Project
{
    class Program
    {
        static ProgressBar progressBar;
        static string getLicenseInfoBody = @"licenseInfo.activated = true;
        licenseInfo.flow = licenseCore.licenseKinds.PRO;
        licenseInfo.label = licenseCore.licenseKinds.PRO;
        licenseInfo.offlineDisabled = false;
        licenseInfo.transactionId = licenseCore.getTransactionId();
        licenseInfo.startDate = new Date('1993-01-01T08:00:00.000Z');
        licenseInfo.stopDate = licenseCore.getInfinityDate();
        licenseInfo.displayedStopDate = false;
        licenseInfo.canExpire = false;
        const licenseInfoString = JSON.stringify(licenseInfo);
        if (callback !== undefined) {
            callback(undefined, licenseInfoString);
        }
        return Promise.resolve(licenseInfoString);";
        static string getDefaultUserInfoBody = @"return {
            accessToken: '',
            displayName: 'anonymous',
            organizationForeignKeys: '',
            primaryOrg: '',
            userId: 'anonymous',
            name: 'anonymous',
            valid: false,
            whitelisted: true
        };";


        static async Task Main(string[] args)
        {
            Console.WriteLine($"请输入UnityHub目录的绝对路径：");
            var unityHubPath = Console.ReadLine();
            unityHubPath = unityHubPath.Trim('"');

            var isUnityHub = false;
            try
            {
                isUnityHub = File.Exists(Path.Combine(unityHubPath, "Unity Hub.exe"));
            }
            catch
            {
            }

            if (!isUnityHub)
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
                LogAndPause($"已经破解或破解失败.");
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

                var licenseClientPath = Path.Combine(exportFolder, "build/services/licenseService/licenseClient.js");
                var authPath = Path.Combine(exportFolder, "build/services/localAuth/auth.js");

                var licenseClientContent = File.ReadAllText(licenseClientPath);
                var authContent = File.ReadAllText(authPath);

                ReplaceMehthodBody(ref licenseClientContent, getLicenseInfoBody, @"getLicenseInfo\(\w+\)\s*{(?<body>.*?return.*?)}");
                ReplaceMehthodBody(ref authContent, getDefaultUserInfoBody, @"getDefaultUserInfo\(\)\s*{(?<body>.*?return.*?};.*?)}");

                File.WriteAllText(licenseClientPath, licenseClientContent);
                File.WriteAllText(authPath, authContent);

                if (Directory.Exists(asarUnpackPath))
                {
                    CopyDirectory(asarUnpackPath, exportFolder, true);
                }

                File.Move(asarPath, asarPath + ".bak");
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine();
                progressBar.Dispose();
                LogAndPause(success ? "破解完成" : "破解失败");
            }
        }

        static void ReplaceMehthodBody(ref string scriptContent, string body, string regex)
        {
            scriptContent = Regex.Replace(scriptContent, regex, evaluator =>
            {
                return evaluator.Value.Replace(evaluator.Groups["body"].Value, "\n" + body + "\n");
            }, RegexOptions.Singleline);
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

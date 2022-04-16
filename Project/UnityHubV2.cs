using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrackUnityHub
{
    internal class UnityHubV2 : Patcher
    {
        const string getLicenseInfoBody = @"licenseInfo.activated = true;
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

        const string getDefaultUserInfoBody = @"return {
            accessToken: '',
            displayName: 'anonymous',
            organizationForeignKeys: '',
            primaryOrg: '',
            userId: 'anonymous',
            name: 'anonymous',
            valid: false,
            whitelisted: true
        };";

        public override bool IsMatch(string version)
        {
            return version.StartsWith("2.");
        }

        public override bool Patch(string exportFolder)
        {
            var licenseClientPath = Path.Combine(exportFolder, "build/services/licenseService/licenseClient.js");
            var authPath = Path.Combine(exportFolder, "build/services/localAuth/auth.js");

            var licenseClientContent = File.ReadAllText(licenseClientPath);
            var authContent = File.ReadAllText(authPath);

            ReplaceMehthodBody(ref licenseClientContent, getLicenseInfoBody, @"getLicenseInfo\(\w+\)\s*{(?<body>.*?return.*?)}");
            ReplaceMehthodBody(ref authContent, getDefaultUserInfoBody, @"getDefaultUserInfo\(\)\s*{(?<body>.*?return.*?};.*?)}");

            File.WriteAllText(licenseClientPath, licenseClientContent);
            File.WriteAllText(authPath, authContent);

            return true;
        }
    }
}

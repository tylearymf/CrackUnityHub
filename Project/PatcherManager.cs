using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrackUnityHub
{
    public class PatcherManager
    {
        static Patcher[] s_Patchers = new Patcher[]
        {
            new UnityHubV2(),
            new UnityHubV3(),
        };

        public static Patcher GetPatcher(string version)
        {
            foreach (var patcher in s_Patchers)
                if (patcher.IsMatch(version))
                    return patcher;

            return null;
        }
    }
}

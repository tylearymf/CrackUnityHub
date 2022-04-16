using System.Text.RegularExpressions;

namespace CrackUnityHub
{
    public abstract class Patcher
    {
        public abstract bool IsMatch(string version);

        public abstract bool Patch(string exportFolder);

        public static void ReplaceMethod(ref string scriptContent, string methodIdentifier, string newMethodContent)
        {
            scriptContent = Regex.Replace(scriptContent, methodIdentifier + @"(?=\{)(?:(?<open>\{)|(?<-open>\})|[^\{\}])+?(?(open)(?!))", evaluator =>
            {
                return newMethodContent;
            }, RegexOptions.Singleline);
        }

        public static void ReplaceMehthodBody(ref string scriptContent, string body, string regex)
        {
            scriptContent = Regex.Replace(scriptContent, regex, evaluator =>
            {
                return evaluator.Value.Replace(evaluator.Groups["body"].Value, "\n" + body + "\n");
            }, RegexOptions.Singleline);
        }
    }
}

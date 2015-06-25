using System;
using System.Reflection;

namespace Facebook
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class FBBuildVersionAttribute : Attribute
    {
        private DateTime buildDate;
        private string buildHash;
        private string buildVersion;

        private string sdkVersion;

        public DateTime Date { get { return buildDate; } }
        public string Hash { get { return buildHash; } }
        public string SdkVersion { get { return sdkVersion; } }
        public string BuildVersion { get { return buildVersion; } }

        public FBBuildVersionAttribute(string sdkVersion, string buildVersion)
        {
            this.buildVersion = buildVersion;
            var parts = buildVersion.Split('.');
            buildDate = DateTime.ParseExact(parts[0], "yyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            buildHash = parts[1];

            this.sdkVersion = sdkVersion;
        }

        public override string ToString()
        {
            return buildVersion;
        }

        public static FBBuildVersionAttribute GetVersionAttributeOfType(Type type)
        {
            foreach (FBBuildVersionAttribute attribute in getAttributes(type))
            {
                return attribute;
            }
            return null;
        }

        private static FBBuildVersionAttribute[] getAttributes(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Assembly assembly = type.Assembly;
            return (FBBuildVersionAttribute[])(assembly.GetCustomAttributes(typeof(FBBuildVersionAttribute), false));
        }
    }
}

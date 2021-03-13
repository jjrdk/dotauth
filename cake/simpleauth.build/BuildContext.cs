namespace simpleauth.build
{
    using System.Linq;
    using Cake.Core;
    using Cake.Frosting;

    public class BuildContext : FrostingContext
    {
        public string BuildConfiguration { get; set; } = "Release";
        public string BuildVersion { get; set; }
        public string InformationalVersion { get; set; }

        public string SolutionName = "simpleauth.sln";

        public BuildContext(ICakeContext context)
            : base(context)
        {
            Environment.WorkingDirectory = Environment.WorkingDirectory.Combine("..").Combine("..");
            BuildConfiguration = context.Arguments.GetArguments("configuration").FirstOrDefault() ?? "Debug";
        }
    }
}

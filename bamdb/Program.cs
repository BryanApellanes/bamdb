using Bam.CommandLine;
using Bam.Console;

namespace Bam.Application
{
    [Serializable]
    class Program
    {
        static void Main(string[] args)
        {
            BamConsoleContext.Current.AddValidArgument("config", description: "The path to the config file used for generation.");
            BamConsoleContext.Current.Main(args);
        }
    }
}

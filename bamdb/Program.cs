using Bam.Console;

namespace Bam.Application
{
    [Serializable]
    class Program
    {
        static void Main(string[] args)
        {
            BamConsoleContext.Current.AddValidArgument("config", description: "The path to the config file used for generation.");
            BamConsoleContext.Current.AddValidArgument("output", false, true, description: "The path where source files are written.");
            BamConsoleContext.Current.Main(args);
        }
    }
}

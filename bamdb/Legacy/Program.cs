using Bam.Net.Logging;
using Bam.Net.Testing;
using System;
using Bam.Net.CommandLine;
using Bam.Shell;
using Bam.Shell.CodeGen;
using Bam.Shell.Data;

namespace Bam.Net.Application
{
    [Serializable]
    class Program : CommandLineTool
    {
        static void Main(string[] args)
        {
            Log.Default = Workspace.Current.CreateLogger<TextFileLogger>();
            TryWritePid();

            IsolateMethodCalls = false;
            
            BamEnvironmentVariables.SetBamVariable("ApplicationName", "bamdb");
            
            ArgumentAdder.AddArguments(args);
            
            ExecuteMainOrInteractive(args);
        }
    }
}

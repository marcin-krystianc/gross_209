using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.FileSystem;
using Microsoft.Build.Locator;
using CommandLine;

namespace MSBuildIssue
{
    public class Options
    {
        [Option(Required = false, Default = true, HelpText = "")]
        public bool? Manual { get; set; }

        [Option(Required = false, Default = null, HelpText = "Pause after N iterations")]
        public int? PauseAfterIterations { get; set; }

        [Option(Required = false, Default = null, HelpText = "Path to the MSBuild")]
        public string MSBuildPath { get; set; }

        [Option(Required = true, Default = null, HelpText = "Path to the solution to be processed")]
        public string SolutionPath { get; set; }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            //Environment.SetEnvironmentVariable("MSBUILDDISABLESDKCACHE", "1");
            
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                if (options.MSBuildPath == null)
                    MSBuildLocator.RegisterDefaults();
                else
                    MSBuildLocator.RegisterMSBuildPath(options.MSBuildPath);

                var directoryName = Path.GetDirectoryName(options.SolutionPath);
                var solutionFileName = Path.GetFileName(options.SolutionPath);
                var originalDirectoryName = directoryName;
                for (var i = 0;; i++)
                {
                    Console.WriteLine($"i:{i}");
                    Console.WriteLine($"Press key to process solution");
                    if (options.Manual.Value ||
                        (options.PauseAfterIterations != null && i % options.PauseAfterIterations == 0 && i > 0))
                        Console.ReadKey();
                    Console.WriteLine($"Processing solution");
                    var result = ProcessSolution(Path.Combine(directoryName, solutionFileName), options.Manual.Value);
                    if (result == 0)
                        throw new Exception("Unexpected result");

                    var newDirectoryName = $"{originalDirectoryName}_{i}";
                    Directory.Move(directoryName, newDirectoryName);
                    directoryName = newDirectoryName;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            });
        }

        static int ProcessSolution(string path, bool manual)
        {
            EvaluationContext.Create(EvaluationContext.SharingPolicy.Isolated);
            var result = 0;
            using (var projectCollection = new ProjectCollection())
            {
                var projectOptions = new ProjectOptions
                {
                    ProjectCollection = projectCollection,
                    LoadSettings = ProjectLoadSettings.IgnoreEmptyImports | ProjectLoadSettings.IgnoreInvalidImports |
                                   ProjectLoadSettings.RecordDuplicateButNotCircularImports | ProjectLoadSettings.IgnoreMissingImports,
                    // IMPORTANT! - EvaluationContext.SharingPolicy.Shared - prevents from creating new evaluation context for each project (avoids memory leak)
                    // IMPORTANT! - new MyFs() - custom FileSystem prevents from using CachingFileSystemWrapper (avoids memory leak)
                    // IMPORTANT! - EvaluationContext is created for each project collection (it is not static) - this lets GC to release some memory when projects are unloaded
                    EvaluationContext = EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared, new MyFs()),
                };
                
                var solutionFile = SolutionFile.Parse(path);
                var projects = solutionFile.ProjectsInOrder
                    .Where(x => x.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
                    .Select(x => Project.FromFile(x.AbsolutePath, projectOptions));
            
                foreach (var project in projects)
                {
                    result += project.Items.Count;
                }

                Console.WriteLine($"Press key to unload projects.");
                if (manual) Console.ReadKey();
                Console.WriteLine($"Unloading all projects.");
                
                projectCollection.UnloadAllProjects();
            }

            return result;
        }
    }
}
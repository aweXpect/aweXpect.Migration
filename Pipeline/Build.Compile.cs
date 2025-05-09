using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	string BranchName;
	AssemblyVersion MainVersion;
	string SemVer;

	Target CalculateNugetVersion => _ => _
		.Unlisted()
		.Executes(() =>
		{
			string preRelease = "-CI";
			if (GitHubActions == null)
			{
				preRelease = "-DEV";
			}
			else if (GitHubActions.Ref.StartsWith("refs/tags/", StringComparison.OrdinalIgnoreCase))
			{
				int preReleaseIndex = GitHubActions.Ref.IndexOf('-');
				preRelease = preReleaseIndex > 0 ? GitHubActions.Ref[preReleaseIndex..] : "";
			}

			SemVer = GitVersion.SemVer;
			BranchName = GitVersion.BranchName;
			MainVersion = AssemblyVersion.FromGitVersion(GitVersion, preRelease);

			if (GitHubActions?.IsPullRequest == true)
			{
				string buildNumber = GitHubActions.RunNumber.ToString();
				Console.WriteLine(
					$"Branch spec is a pull request. Adding build number {buildNumber}");

				SemVer = string.Join('.', GitVersion.SemVer.Split('.').Take(3).Union([buildNumber,]));
			}

			Console.WriteLine($"SemVer = {SemVer}");
		});

	Target Clean => _ => _
		.Unlisted()
		.Before(Restore)
		.Executes(() =>
		{
			ArtifactsDirectory.CreateOrCleanDirectory();
			Log.Information("Cleaned {path}...", ArtifactsDirectory);

			TestResultsDirectory.CreateOrCleanDirectory();
			Log.Information("Cleaned {path}...", TestResultsDirectory);
		});

	Target Restore => _ => _
		.Unlisted()
		.DependsOn(Clean)
		.Executes(() =>
		{
			DotNetRestore(s => s
				.SetProjectFile(Solution)
				.EnableNoCache()
				.SetConfigFile(RootDirectory / "nuget.config"));
		});

	Target Compile => _ => _
		.DependsOn(Restore)
		.DependsOn(CalculateNugetVersion)
		.Executes(() =>
		{
			ReportSummary(s => s
				.WhenNotNull(MainVersion, (summary, mainVersion) => summary
					.AddPair("Version", mainVersion.FileVersion + mainVersion.PreRelease)));

			Dictionary<Project, Configuration> projects = new()
			{
				{
					Solution.aweXpect_Migration_Analyzers, Configuration
				},
				{
					Solution.aweXpect_Migration_Analyzers_CodeFixers, Configuration
				},
			};

			foreach (var (project, configuration) in projects)
			{
				DotNetBuild(s => s
					.SetProjectFile(project)
					.SetConfiguration(configuration)
					.EnableNoLogo()
					.SetProcessAdditionalArguments($"/p:SolutionDir={RootDirectory}")
					.SetVersion(MainVersion.FileVersion + MainVersion.PreRelease)
					.SetAssemblyVersion(MainVersion.FileVersion)
					.SetFileVersion(MainVersion.FileVersion)
					.SetInformationalVersion(MainVersion.InformationalVersion));
			}
			
			DotNetBuild(s => s
				.SetProjectFile(Solution)
				.SetConfiguration(Configuration)
				.EnableNoLogo()
				.EnableNoRestore()
				.SetVersion(MainVersion.FileVersion + MainVersion.PreRelease)
				.SetAssemblyVersion(MainVersion.FileVersion)
				.SetFileVersion(MainVersion.FileVersion)
				.SetInformationalVersion(MainVersion.InformationalVersion));
		});

	public record AssemblyVersion(string FileVersion, string InformationalVersion, string PreRelease)
	{
		public static AssemblyVersion FromGitVersion(GitVersion gitVersion, string preRelease)
		{
			if (gitVersion is null)
			{
				return null;
			}

			return new AssemblyVersion(gitVersion.AssemblySemVer, gitVersion.InformationalVersion, preRelease);
		}
	}
}

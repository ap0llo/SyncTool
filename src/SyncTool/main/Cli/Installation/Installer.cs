using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Squirrel;
using SyncTool.Cli.Configuration;

namespace SyncTool.Cli.Installation
{
    static class Installer
    {
        const string s_DefaultConfigResourceName = "SyncTool.config.json";

        public static bool IsInstalled => File.Exists(InstallationFlagFilePath);

        static string InstallationFlagFilePath => Path.Combine(InstallationDirectory, "IsInstalled");

        static string InstallationRoot => Path.GetFullPath(Path.Combine(InstallationDirectory, "..")).TrimEnd(Path.DirectorySeparatorChar);

        static string InstallationDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).TrimEnd(Path.DirectorySeparatorChar);

        static string LauncherFilePath => Path.Combine(InstallationRoot, ApplicationName.ToLowerInvariant() + ".bat");

        static string ApplicationName => Assembly.GetExecutingAssembly().GetName().Name;

        static string ConfigFilePath => Path.Combine(InstallationRoot, ContainerBuilderExtensions.ConfigFileName);

        public static void HandleInstallationEvents()
        {            
            SquirrelAwareApp.HandleEvents(
                onInitialInstall: WithExceptionLogging<Version>(v =>
                {
                    CreateDefaultConfigFile();
                    CreateInstallationFlagFile();
                    CreateLauncherFile();
                    AddToPath();
                }),
                onAppUpdate: WithExceptionLogging<Version>(v =>
                {
                    CreateInstallationFlagFile();
                    CreateLauncherFile();
                }),
                onAppUninstall: WithExceptionLogging<Version>(v =>
                {
                    RemoveDefaultConfigFile();
                    RemoveInstallationFlagFile();
                    RemoveLauncherFile();
                    RemoveFromPath();
                }),
                onFirstRun: WithExceptionLogging(() =>
                {
                    Console.WriteLine($"{ApplicationName} was installed and added to PATH.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }));

        }

        
        static void CreateLauncherFile()
        {
            using (var fileStream = File.OpenWrite(LauncherFilePath))
            using (var writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("@ECHO OFF");
                writer.WriteLine($"\"{Assembly.GetExecutingAssembly().Location}\" %*");
                
            }
        }

        static void RemoveLauncherFile()
        {
            File.Delete(LauncherFilePath);                
        }

        static void AddToPath()
        {
            var value = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var currentValues = new HashSet<string>(value.Split(';'), StringComparer.InvariantCultureIgnoreCase);

            if (!currentValues.Contains(InstallationRoot))
            {
                Environment.SetEnvironmentVariable("PATH", value + ";" + InstallationRoot, EnvironmentVariableTarget.User);
            }            
        }

        static void RemoveFromPath()
        {
            var value = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var currentValues = new HashSet<string>(value.Split(';'), StringComparer.InvariantCultureIgnoreCase);


            var valuesToRemove = currentValues.Where(v => StringComparer.InvariantCultureIgnoreCase.Equals(v, InstallationDirectory));

            foreach (var path in valuesToRemove)
            {
                value = value.Replace(path, "");                
            }

            while (value.Contains(";;"))
            {
                value = value.Replace(";;", ";");
            }

            Environment.SetEnvironmentVariable("PATH", value + ";" + InstallationRoot, EnvironmentVariableTarget.User);
        }

        static void CreateInstallationFlagFile()
        {
            File.WriteAllText(InstallationFlagFilePath, "");
        }

        static void RemoveInstallationFlagFile()
        {
            File.Delete(InstallationFlagFilePath);
        }

        static void CreateDefaultConfigFile()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(s_DefaultConfigResourceName))
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                File.WriteAllText(ConfigFilePath, content);
            }           
        }

        static void RemoveDefaultConfigFile()
        {
            File.Delete(ConfigFilePath);
        }

        static Action<T> WithExceptionLogging<T>(Action<T> action)
        {
            return arg =>
            {
                try
                {
                    action(arg);
                }
                catch (Exception e)
                {
                    LogException(e);
                    throw;
                }
            };
        }
        
        static Action WithExceptionLogging(Action action)
        {
            return () =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {

                    LogException(e);
                    throw;
                }
            };
        }

        static void LogException(Exception e)
        {
            try
            {
                using (var stream = File.OpenWrite(Path.Combine(InstallationDirectory, $"Exception_{Guid.NewGuid()}.txt")))
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(DateTime.Now);
                    writer.Write(e);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
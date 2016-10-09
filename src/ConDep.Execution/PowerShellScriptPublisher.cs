using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ConDep.Dsl.Config;
using ConDep.Execution.PSScripts.ConDep;
using ConDep.Execution.Resources;

namespace ConDep.Execution
{
    internal class PowerShellScriptPublisher
    {
        private readonly ConDepSettings _settings;
        private string _localTargetPath;

        public PowerShellScriptPublisher(ConDepSettings settings)
        {
            _settings = settings;
            _localTargetPath = Path.Combine(Path.GetTempPath(), @"PSScripts\ConDep");
        }

        public void PublishScripts(ServerConfig server)
        {

            if (Directory.Exists(_localTargetPath))
            {
                Directory.Delete(_localTargetPath, true);
            }

            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad_UploadAssemblyScripts;

            SaveConDepScriptModuleResourceToFolder(_localTargetPath);
            SaveConDepScriptResourcesToFolder(_localTargetPath);
            SaveExternalScriptResourcesToFolder(_localTargetPath);
            SaveExecutionPathScriptsToFolder(_localTargetPath, _settings.Config);

            SyncDir(_localTargetPath, server.GetServerInfo().ConDepScriptsFolderDos, server, _settings);
        }

        private void OnAssemblyLoad_UploadAssemblyScripts(object sender, AssemblyLoadEventArgs args)
        {
            var assembly = args.LoadedAssembly;
            if (!assembly.IsDynamic && !( assembly.FullName.StartsWith("System.") || assembly.FullName.StartsWith("Microsoft.") || assembly.FullName.StartsWith("mscorlib")))
            {
                GetResourcesFromAssembly(assembly, _localTargetPath);
            }
        }

        public void SyncDir(string srcDir, string dstDir, ServerConfig server, ConDepSettings settings)
        {
            var filePublisher = new FilePublisher();
            filePublisher.PublishDirectory(srcDir, dstDir, server, settings);
        }

        public void PublishRemoteHelperAssembly(ServerConfig server)
        {
            var src = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "ConDep.Dsl.Remote.Helpers.dll");
            CopyFile(src, server, _settings);
        }

        private void SaveConDepScriptModuleResourceToFolder(string localTargetPath)
        {
            var resource = ConDepResources.ConDepModule;
            
            var regex = new Regex(@".+\.(.+\.(ps1|psm1))");
            var match = regex.Match(resource);
            if (match.Success)
            {
                var resourceName = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    var resourceNamespace = resource.Replace("." + resourceName, "");
                    ConDepResourceFiles.GetFilePath(GetType().Assembly, resourceNamespace, resourceName, dirPath: localTargetPath, keepOriginalFileName: true);
                }
            }
        }

        private void CopyFile(string srcPath, ServerConfig server, ConDepSettings settings)
        {
            var dstPath = Path.Combine(server.GetServerInfo().TempFolderDos, Path.GetFileName(srcPath));
            CopyFile(srcPath, dstPath, server, settings);
        }

        private void CopyFile(string srcPath, string dstPath, ServerConfig server, ConDepSettings settings)
        {
            var filePublisher = new FilePublisher();
            filePublisher.PublishFile(srcPath, dstPath, server, settings);
        }

        private void SaveExecutionPathScriptsToFolder(string localTargetPath, ConDepEnvConfig config)
        {
            var currDir = Directory.GetCurrentDirectory();

            foreach (var psDir in config.PowerShellScriptFolders)
            {
                var absPath = Path.Combine(currDir, psDir);
                if(!Directory.Exists(absPath)) throw new DirectoryNotFoundException(absPath);

                var dirInfo = new DirectoryInfo(absPath);
                var files = dirInfo.GetFiles("*.ps1", SearchOption.TopDirectoryOnly);
                foreach (var file in files.Select(x => x.FullName))
                {
                    File.Copy(file, Path.Combine(localTargetPath, Path.GetFileName(file)));
                }
            }
        }

        private void SaveConDepScriptResourcesToFolder(string localTargetPath)
        {
            var files = new List<string>();
            foreach (
                var childAssembly in

                    AppDomain.CurrentDomain.GetAssemblies()
                        .Where(
                            x =>
                                !x.IsDynamic &&
                                x.FullName.StartsWith("ConDep.")))
            {
                files.AddRange(GetResourcesFromAssembly(childAssembly, localTargetPath));
            }
        }

        private void SaveExternalScriptResourcesToFolder(string localTargetPath)
        {
            var files = new List<string>();
            foreach (
                var childAssembly in
                    AppDomain.CurrentDomain.GetAssemblies()
                        .Where(
                            x =>
                                !x.IsDynamic &&
                                !(x.FullName.StartsWith("ConDep.") || x.FullName.StartsWith("System.") || x.FullName.StartsWith("Microsoft.") || x.FullName.StartsWith("mscorlib") )))
            {
                files.AddRange(GetResourcesFromAssembly(childAssembly, localTargetPath));
            }
        }

        private IEnumerable<string> GetResourcesFromAssembly(Assembly assembly, string localTargetPath)
        {
            return assembly.GetManifestResourceNames().Select(resource => ExtractPowerShellFileFromResource(assembly, localTargetPath, resource)).Where(path => !string.IsNullOrWhiteSpace(path));
        }

        private string ExtractPowerShellFileFromResource(Assembly assembly, string localTargetPath, string resource)
        {
            var regex = new Regex(@".+\.(.+\.ps1)");
            var match = regex.Match(resource);
            if (match.Success)
            {
                var resourceName = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    var resourceNamespace = resource.Replace("." + resourceName, "");
                    return ConDepResourceFiles.GetFilePath(assembly, resourceNamespace, resourceName, dirPath: localTargetPath, keepOriginalFileName: true);
                }
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows.Input;
#nullable enable
namespace Saxon.Api.Support {
    /// <summary>
    /// Loads and invokes a user-supplied <c>IProcessorInitializer</c>
    /// </summary>
    public class InitializationHandler  {
        
        // Heavily reliant on https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support

        public static void doInitialization(Processor processor, string initializer) {
            Assembly plugin = LoadPlugin(initializer);
            int count = 0;

            IProcessorInitializer? initializationHook = null;
            foreach (Type type in plugin.GetTypes()) {
                if (typeof(IProcessorInitializer).IsAssignableFrom(type)) {
                    initializationHook = Activator.CreateInstance(type) as IProcessorInitializer;
                    count++;
                }
            }

            if (count != 1) {
                string plurality = count == 0 ? "No" : "Multiple";
                string availableTypes = string.Join(",", plugin.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"{plurality} types in {plugin} implement Saxon.Api.IProcessorInitializer.\n" +
                    $"Available types: {availableTypes}");
            }

            Debug.Assert(initializationHook != null);
            initializationHook.Initialize(processor);
        }
        
        private static Assembly LoadPlugin(string relativePath)
        {
            // Navigate up to the solution root
            // string root = Path.GetFullPath(Path.Combine(
            //     Path.GetDirectoryName(
            //         Path.GetDirectoryName(
            //             Path.GetDirectoryName(
            //                 Path.GetDirectoryName(
            //                     Path.GetDirectoryName(typeof(InitializationHandler).Assembly.Location)))))));
            //
            // string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
            string pluginLocation = relativePath;
            Console.WriteLine($"Loading initializer from: {pluginLocation}");
            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));

        }
        

    }
    
    class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
        
    }
}
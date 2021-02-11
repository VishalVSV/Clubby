using Clubby.GeneralUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Clubby.Plugins
{
    /// <summary>
    /// Manages plugins by importing them from dlls.
    /// </summary>
    /// <typeparam name="T">The type to extract from plugins</typeparam>
    /// <typeparam name="U">The type of data to provide the initializer</typeparam>
    public class PluginManager<T,U>
        where U: PluginData
    {
        /// <summary>
        /// Dictionary of all types loaded from plugins
        /// </summary>
        public Dictionary<string, List<Type>> LoadedTypes = new Dictionary<string, List<Type>>();

        

        /// <summary>
        /// Load plugins from a folder with dlls
        /// </summary>
        /// <param name="folder">The folder to search</param>
        /// <param name="data">The data object to provide to the plugin on initialization</param>
        public void Load(string folder,U data)
        {
            data.OnReload();

            // Clear all the current types to prevent conflicts
            LoadedTypes.Clear();

            // Clear the periodic activity closures set by the plugins
            // Note: This is a consquence of the specific implementation and use case. If this code is ever used as an example for
            //       how to implement plugins this line is not needed.
            Program.config.scheduler.PeriodicActivities.Clear();

            // Get type of plugin to import
            Type plugin_type = typeof(T);

            // Loop over all the dll files in the given folder
            foreach (string dll in Directory.EnumerateFiles(folder, "*.dll", SearchOption.TopDirectoryOnly))
            {
                // Read the dll into a byte array
                // Note: This is done to let the files be changed after loading to allow for hotloading.
                //       It is the best way I knew how to do it, not the actual best way to do it. This is memory intensive for large dlls.
                byte[] b = File.ReadAllBytes(dll);

                // Load the bytes into a assembly
                Assembly plugin_assembly = Assembly.Load(b);

                // List of types to assign to the current plugin
                List<Type> plugin_types = new List<Type>();

                // Get all public classes
                Type[] types = plugin_assembly.GetExportedTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    // If the class is static [types[i].IsAbstract && types[i].IsSealed] it could be the initializer.
                    // If it is provide it with the data given
                    if (types[i].IsAbstract && types[i].IsSealed && types[i].GetCustomAttribute<PluginInit>() != null)
                    {
                        try
                        {
                            // Try and get the initializer function
                            MethodInfo mi = types[i].GetMethod("Init");
                            if (mi != null)
                            {
                                mi.Invoke(null, new object[] { data });
                            }
                        }
                        catch (Exception) { }
                    }

                    // If the type is an exported type and is of the type required add it to the list.
                    if (plugin_type.IsAssignableFrom(types[i]) && types[i].GetCustomAttribute<PluginExport>() != null)
                    {
                        plugin_types.Add(types[i]);
                    }
                }

                Logger.Log(this, $"Loaded {Path.GetFileNameWithoutExtension(dll)}");

                // Register all the types into the dictionary under the plugin name
                LoadedTypes.Add(Path.GetFileNameWithoutExtension(dll), plugin_types);
            }
        }

        /// <summary>
        /// Gets an instance of the type that satisfies the given predicate
        /// </summary>
        /// <param name="predicate">The predicate to use to identify the type</param>
        /// <returns></returns>
        public T GetInstance(Predicate<(string,Type)> predicate)
        {
            foreach (var (plugin,types) in LoadedTypes)
            {
                for (int i = 0; i < types.Count; i++)
                {
                    if (predicate((plugin,types[i])))
                    {
                        return (T)Activator.CreateInstance(types[i]);
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Get instances of all types that satisfy the predicate
        /// </summary>
        /// <param name="predicate">The predicate used to identify the types required</param>
        /// <returns></returns>
        public List<T> GetInstances(Predicate<Type> predicate)
        {
            List<T> plugins = new List<T>();

            foreach (List<Type> types in LoadedTypes.Values)
            {
                for (int i = 0; i < types.Count; i++)
                {
                    if (predicate(types[i]))
                    {
                        plugins.Add((T)Activator.CreateInstance(types[i]));
                    }
                }
            }

            return plugins;
        }

        /// <summary>
        /// Get all the types that satisfy the predicate
        /// </summary>
        /// <param name="predicate">The predicated used to identfy the types</param>
        /// <returns></returns>
        public List<Type> GetPlugins(Predicate<Type> predicate)
        {
            List<Type> plugins = new List<Type>();

            foreach (List<Type> types in LoadedTypes.Values)
            {
                for (int i = 0; i < types.Count; i++)
                {
                    if (predicate(types[i]))
                    {
                        plugins.Add(types[i]);
                    }
                }
            }

            return plugins;
        }
    }
}

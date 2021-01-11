using Clubby.GeneralUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Clubby.Plugins
{
    public class PluginManager<T,U>
    {
        public Dictionary<string, List<Type>> LoadedTypes = new Dictionary<string, List<Type>>();

        public void Load(string folder,U data)
        {
            LoadedTypes.Clear();
            Program.config.scheduler.PeriodicActivities.Clear();

            Type plugin_type = typeof(T);

            foreach (string dll in Directory.EnumerateFiles(folder, "*.dll", SearchOption.TopDirectoryOnly))
            {
                Logger.Log(this, $"Loading {Path.GetFileNameWithoutExtension(dll)}");
                byte[] b = File.ReadAllBytes(dll);
                Assembly plugin_assembly = Assembly.Load(b);

                List<Type> plugin_types = new List<Type>();

                Type[] types = plugin_assembly.GetExportedTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    if(types[i].IsAbstract && types[i].IsSealed && types[i].GetCustomAttribute<PluginInit>() != null)
                    {
                        try
                        {
                            MethodInfo mi = types[i].GetMethod("Init");
                            if (mi != null)
                            {
                                mi.Invoke(null, new object[] { data });
                            }
                        }
                        catch (Exception) { }
                    }
                    if (plugin_type.IsAssignableFrom(types[i]) && types[i].GetCustomAttribute<PluginExport>() != null)
                    {
                        plugin_types.Add(types[i]);
                        Logger.Log(this, $"  Loaded {types[i].Name}");
                    }
                }

                Logger.Log(this, $"Loaded {Path.GetFileNameWithoutExtension(dll)}");

                LoadedTypes.Add(Path.GetFileNameWithoutExtension(dll), plugin_types);
            }
        }

        public T GetInstance(Func<Type,bool> identifier)
        {
            foreach (List<Type> types in LoadedTypes.Values)
            {
                for (int i = 0; i < types.Count; i++)
                {
                    if (identifier(types[i]))
                    {
                        return (T)Activator.CreateInstance(types[i]);
                    }
                }
            }

            return default;
        }

        public List<T> GetInstances(Func<Type,bool> identifier)
        {
            List<T> plugins = new List<T>();

            foreach (List<Type> types in LoadedTypes.Values)
            {
                for (int i = 0; i < types.Count; i++)
                {
                    if (identifier(types[i]))
                    {
                        plugins.Add((T)Activator.CreateInstance(types[i]));
                    }
                }
            }

            return plugins;
        }

        public List<Type> GetPlugins(Func<Type,bool> identifier)
        {
            List<Type> plugins = new List<Type>();

            foreach (List<Type> types in LoadedTypes.Values)
            {
                for (int i = 0; i < types.Count; i++)
                {
                    if (identifier(types[i]))
                    {
                        plugins.Add(types[i]);
                    }
                }
            }

            return plugins;
        }
    }
}

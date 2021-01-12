using System;

namespace Clubby.Plugins
{
    /// <summary>
    /// The attribute that marks the current static class as the initializer class for the whole plugin
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginInit : Attribute { }
}

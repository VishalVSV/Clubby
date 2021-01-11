using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.Plugins
{
    /// <summary>
    /// The attribute that signals the current class to be exported with the plugin
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginExport : Attribute { }
}

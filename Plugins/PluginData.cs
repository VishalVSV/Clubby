using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.Plugins
{
    /// <summary>
    /// Class that all class passed as data to plugins must implement
    /// </summary>
    public abstract class PluginData
    {
        /// <summary>
        /// Used to delete any invalid references on reload
        /// </summary>
        public virtual void OnReload()
        {

        }
    }
}

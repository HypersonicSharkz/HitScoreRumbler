using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace HitScoreRumbler.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual bool Enabled { get; set; } = true;
        public virtual float StrengthMultiplier { get; set; } = 1.2f; // Must be 'virtual' if you want BSIPA to detect a value change and save the config automatically.
        public virtual float DurationMultiplier { get; set; } = 1f;

        [UseConverter(typeof(ListConverter<Vector2>))]
        public virtual List<Vector2> Points { get; set; } = new List<Vector2>() { new Vector2(0, 0), new Vector2(1, 1) };

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }
    }
}
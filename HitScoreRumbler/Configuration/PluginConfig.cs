using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Data;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;
using System.Drawing;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace HitScoreRumbler.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual bool Enabled { get; set; } = true;
        public virtual string ChosenPreset { get; set; } = "default";

        [Ignore]
        public Preset LoadedPreset {  get; set; }
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

    public class Preset
    {
        public float StrengthMultiplier { get; set; } = 1.2f;
        public float DurationMultiplier { get; set; } = 1f;

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<PointF> Points { get; set; } = new List<PointF>() { new PointF(0, 0), new PointF(1, 1) };
    }
}
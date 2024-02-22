using HitScoreRumbler.Configuration;
using IPA.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitScoreRumbler.Utils
{
    internal class PresetHelper
    {
        public static string PresetsFolder { get => Path.Combine(UnityGame.UserDataPath, "HitScoreRumbler"); }

        public static Preset GetPreset(string presetName)
        {
            string presetFile = Path.Combine(PresetsFolder, presetName + ".json") ;
            string[] fileNames = Directory.GetFiles(PresetsFolder, @"*.json");

            if (!fileNames.Contains(presetFile))
            {
                Preset preset = new Preset();
                SavePreset(preset, Path.GetFileNameWithoutExtension(presetFile));
                return preset;
            }

            string json = File.ReadAllText(presetFile);
            return JsonConvert.DeserializeObject<Preset>(json);
        }

        public static void SavePreset(Preset preset, string presetName)
        {
            string text = JsonConvert.SerializeObject(preset, Formatting.Indented);
            File.WriteAllText(Path.Combine(PresetsFolder, presetName + ".json"), text);
        }

        public static void RemovePreset(string presetName)
        {
            string presetFile = Path.Combine(PresetsFolder, presetName + ".json");
            if (!File.Exists(presetFile))
                return;

            File.Delete(presetFile);
        }

        public static List<string> GetAllPresets()
        {
            string[] fileNames = Directory.GetFiles(PresetsFolder, @"*.json");
            List<string> presets = new List<string>();
            foreach (string file in fileNames)
            {
                presets.Add(Path.GetFileNameWithoutExtension(file));
            }

            if (presets.Count == 0)
                presets.Add("default");

            return presets;
        }
    }
}

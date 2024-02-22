using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HitScoreRumbler.Configuration;
using HitScoreRumbler.HarmonyPatches;
using HitScoreRumbler.Utils;
using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using Zenject;

namespace HitScoreRumbler.UI
{
    [HotReload(RelativePathToLayout = @"ConfigMenu.bsml")]
    [ViewDefinition("HitScoreRumbler.UI.ConfigMenu.bsml")]
    internal class ConfigMenuController : IInitializable, INotifyPropertyChanged, IDisposable
    {
        [Inject]
        private HapticFeedbackController hapticFeedbackController;

        private static PluginConfig Config => PluginConfig.Instance;

        public event PropertyChangedEventHandler PropertyChanged;

        #region BSML Fields

        [UIParams]
        private readonly BSMLParserParams parserParams;

        [UIValue("StrengthMultiplier")]
        private float StrengthMultiplier
        {
            get => Config.LoadedPreset.StrengthMultiplier;
            set
            {
                Config.LoadedPreset.StrengthMultiplier = value;
                PresetHelper.SavePreset(Config.LoadedPreset, Config.ChosenPreset);
            }
        }

        [UIValue("DurationMultiplier")]
        private float DurationMultiplier
        {
            get => Config.LoadedPreset.DurationMultiplier;
            set 
            { 
                Config.LoadedPreset.DurationMultiplier = value;
                PresetHelper.SavePreset(Config.LoadedPreset, Config.ChosenPreset);
            }
        }

        [UIValue("Enabled")]
        private bool Enable
        {
            get => Config.Enabled;
            set => Config.Enabled = value;
        }

        [UIComponent("PresetsDropDown")]
        private DropDownListSetting dropDown;

        [UIValue("PresetList")]
        private List<object> presets = PresetHelper.GetAllPresets().Cast<object>().ToList();

        [UIValue("PresetChoice")]
        private string presetChoice
        {
            get => Config.ChosenPreset;
            set
            {
                Config.ChosenPreset = value;
                Preset preset = PresetHelper.GetPreset(value);
                Config.LoadedPreset = preset;
                NotifyPropertyChanged(nameof(DurationMultiplier));
                NotifyPropertyChanged(nameof(StrengthMultiplier));
                NotifyPropertyChanged(nameof(CanRemove));
                UpdateGrid();
            }
        }

        [UIComponent("Graph")]
        private ClickableImage GraphImage;

        [UIAction("AddProfileClick")]
        private void addProfile()
        {
            KeyboardText = "New Profile";

            parserParams.EmitEvent("close-keyboard");
            parserParams.EmitEvent("open-keyboard");
        }

        [UIAction("RemoveProfileClick")]
        private void removeProfile()
        {
            if (!CanRemove)
                return;

            presets.Remove(Config.ChosenPreset);
            PresetHelper.RemovePreset(Config.ChosenPreset);
            presetChoice = (string)presets[0];

            NotifyPropertyChanged(nameof(presetChoice));

            dropDown.UpdateChoices();
        }

        [UIAction("keyboard-enter")]
        private void KeyboardEnter(string keyboardText)
        {
            //Create new preset
            if (presets.Contains(keyboardText)) 
                return;

            Preset preset = new Preset();
            PresetHelper.SavePreset(preset, keyboardText);
            presets.Add(keyboardText);

            presetChoice = keyboardText;
            NotifyPropertyChanged(nameof(presetChoice));

            dropDown.UpdateChoices();
        }

        [UIValue("keyboard-text")]
        private string KeyboardText;

        [UIValue("canRemove")]
        private bool CanRemove => Config.ChosenPreset != "default";

        [UIAction("#post-parse")]
        private void PostParse()
        {
            SetUp();
        }


        #endregion

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetUp()
        {
            GraphImage.OnClickEvent = GraphClicked;

            UpdateGrid();

            Plugin.Log.Info("HitScore Rumbler Setup");
        }

        private void GraphClicked(PointerEventData pointerEventData)
        {
            Vector2 cp = pointerEventData.pointerPressRaycast.screenPosition;
            Vector3[] v = new Vector3[4];
            GraphImage.rectTransform.GetWorldCorners(v);
            //Get point on graph from (0,0) to (1,1)
            //Top right being (1,1), bottom left being (0,0)
            Vector2 p = FitVectorToGrid(new Vector2(
                (cp.x - v[0].x) / (v[2].x - v[0].x),
                (cp.y - v[0].y) / (v[2].y - v[0].y)
                ), 28, 20);

            bool add = true;

            foreach (PointF vector2 in Config.LoadedPreset.Points)
            {
                if (EqualsRound(vector2.X, p.x))
                {
                    Config.LoadedPreset.Points.Remove(vector2);
                    if (EqualsRound(vector2.Y, p.y))
                        add = false;
                    break;
                }
            }

            if (add)
            {
                Config.LoadedPreset.Points.Add(new PointF(p.x, p.y));
                PreviewRumble(p);
            }

            UpdateGrid();

            PresetHelper.SavePreset(Config.LoadedPreset, Config.ChosenPreset);
        }

        private void PreviewRumble(Vector2 p)
        {
            Rumble.normalPreset._duration = 0.5f;
            Rumble.normalPreset._strength = p.y * PluginConfig.Instance.LoadedPreset.StrengthMultiplier;

            hapticFeedbackController.PlayHapticFeedback(XRNode.RightHand, Rumble.normalPreset);
            hapticFeedbackController.PlayHapticFeedback(XRNode.LeftHand, Rumble.normalPreset);
        }

        private void UpdateGrid()
        {
            Config.LoadedPreset.Points = Config.LoadedPreset.Points.OrderBy(pt => pt.X).ToList();

            //Somehow generate image with those points

            // Create a new bitmap with specified width and height
            Bitmap bitmap = new Bitmap(Properties.Resources.Grid);

            // Create a Graphics object from the bitmap
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // Define the radius of the circle
                float circleRadius = 16f;
                float lineSize = 6f;

                Point[] dots = new Point[Config.LoadedPreset.Points.Count];
                // Draw circles on the bitmap based on the points
                int i = 0;
                foreach (PointF point in Config.LoadedPreset.Points)
                {
                    // Calculate the position of the circle's top-left corner
                    int x = (int)(point.X * bitmap.Width - circleRadius);
                    int y = (int)(bitmap.Height - point.Y * bitmap.Height - circleRadius);
                    dots[i] = new Point((int)(x + circleRadius), (int)(y + circleRadius));

                    // Draw a circle with specified center and radius
                    g.FillEllipse(Brushes.Cyan, x, y, circleRadius * 2, circleRadius * 2);
                    i++;
                }
                if (dots.Length > 1)
                {
                    g.DrawLines(new Pen(System.Drawing.Color.Red, lineSize), dots);

                    g.DrawLine(new Pen(System.Drawing.Color.Red, lineSize), dots[0], new Point(0, dots[0].Y));
                    g.DrawLine(new Pen(System.Drawing.Color.Red, lineSize), dots[dots.Length - 1], new Point(bitmap.Width, dots[dots.Length - 1].Y));
                }
                else if (dots.Length == 1)
                {
                    g.DrawLine(new Pen(System.Drawing.Color.Red, lineSize), new Point(0, dots[0].Y), new Point(bitmap.Width, dots[0].Y));
                }
                else
                {
                    g.DrawLine(new Pen(System.Drawing.Color.Red, lineSize), new Point(0, (int)lineSize), new Point(bitmap.Width, (int)lineSize));
                }

            }
            Texture2D texture = BitmapToTexture2D(bitmap);
            GraphImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        Vector2 FitVectorToGrid(Vector2 vector, int gridX, int gridY)
        {
            float roundedX = Mathf.Round((vector.x) * gridX) / gridX;
            float roundedY = Mathf.Round((vector.y) * gridY) / gridY;
            return new Vector2(roundedX, roundedY);
        }

        bool EqualsRound(float a, float b)
        {
            // Check if the absolute difference between each component is within the tolerance
            return Mathf.Abs(a - b) < 0.001;
        }

        Texture2D BitmapToTexture2D(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            Texture2D texture = new Texture2D(bitmap.Width, bitmap.Height);
            texture.LoadImage(ms.ToArray());
            ms.Close();
            return texture;
        }

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("Hit Score Rumbler", "HitScoreRumbler.UI.ConfigMenu.bsml", this);
        }

        public void Dispose()
        {
            if (BeatSaberMarkupLanguage.Settings.BSMLSettings.instance != null)
            {
                BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.RemoveSettingsMenu(this);
            }
        }
    }
}

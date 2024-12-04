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
using Libraries.HM.HMLib.VR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using Zenject;
using static AutoRecord;

namespace HitScoreRumbler.UI
{
    [HotReload(RelativePathToLayout = @"ConfigMenu.bsml")]
    [ViewDefinition("HitScoreRumbler.UI.ConfigMenu.bsml")]
    internal class ConfigMenuController : IInitializable, INotifyPropertyChanged, IDisposable
    {
        [Inject]
        private HapticFeedbackManager hapticFeedbackController;

        private static PluginConfig Config => PluginConfig.Instance;

        public event PropertyChangedEventHandler PropertyChanged;

        private List<PointF> selectedGraph = null;

        private string _selectedGraphName = "Strength";

        Pen linePen = new Pen(System.Drawing.Color.Red, 6f);
        Pen previewPen = new Pen(System.Drawing.Color.Blue, 6f);
        private int _previewLinePad;

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

        [UIValue("Frequency")]
        private float Frequency
        {
            get => Config.LoadedPreset.Frequency;
            set
            {
                Config.LoadedPreset.Frequency = value;
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
                NotifyPropertyChanged(nameof(Frequency));
                NotifyPropertyChanged(nameof(CanRemove));

                switch (displayedGraph)
                {
                    case "Strength":
                        selectedGraph = Config.LoadedPreset.Points;
                        break;
                    case "Duration":
                        selectedGraph = Config.LoadedPreset.PointsDuration;
                        break;
                    case "Frequency":
                        selectedGraph = Config.LoadedPreset.PointsFrequency;
                        break;
                }

                UpdateGrid();
            }
        }

        [UIComponent("Graph")]
        private ClickableImage GraphImage;

        [UIComponent("PreviewLine")]
        private LayoutGroup PreviewLine;

        [UIValue("list-graphs")]
        private List<object> graphs = new List<object>() { "Strength", "Duration" };

        [UIValue("displayed-graph")]
        private string displayedGraph
        {
            get
            {
                return _selectedGraphName;
            }
            set
            {
                Plugin.Log.Info("Selected Graph: " + value);
                if (value == _selectedGraphName) return;

                switch (value)
                {
                    case "Strength":
                        selectedGraph = Config.LoadedPreset.Points;
                        break;
                    case "Duration":
                        selectedGraph = Config.LoadedPreset.PointsDuration;
                        break;
                    case "Frequency":
                        selectedGraph = Config.LoadedPreset.PointsFrequency;
                        break;
                }

                UpdateGrid();

                _selectedGraphName = value;
            }
        }

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

        [UIValue("previewLinePad")]
        private int previewLinePad => _previewLinePad;

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
            selectedGraph = Config.LoadedPreset.Points.OrderBy(pt => pt.X).ToList();

            GraphImage.OnClickEvent += GraphClicked;

            //Le Spaghet
            Task.Run(() => { Thread.Sleep(500); }).ContinueWith((x) => UpdateGrid(), TaskScheduler.FromCurrentSynchronizationContext());
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

            foreach (PointF vector2 in selectedGraph)
            {
                if (EqualsRound(vector2.X, p.x))
                {
                    selectedGraph.Remove(vector2);
                    if (EqualsRound(vector2.Y, p.y))
                        add = false;
                    break;
                }
            }

            if (add)
            {
                selectedGraph.Add(new PointF(p.x, p.y));
                PreviewRumble(p);
            }

            selectedGraph.Sort((x, y) => x.X.CompareTo(y.X));

            UpdateGrid();

            PresetHelper.SavePreset(Config.LoadedPreset, Config.ChosenPreset);
        }

        private void PreviewRumble(Vector2 p)
        {
            HapticPresetSO so = Helper.GetHapticPreset(p.x * 0.28f + 0.01f);

            hapticFeedbackController.PlayHapticFeedback(XRNode.RightHand, so);
            hapticFeedbackController.PlayHapticFeedback(XRNode.LeftHand, so);
        }

        [UIAction("PreviewRumbleSlide")]
        private void PreviewRumbleSlide()
        {
            Task.Run(() =>
            {
                float dist = 0;
                while (dist <= 1)
                {
                    HapticPresetSO so = Helper.GetHapticPreset(dist * 0.28f + 0.01f);

                    hapticFeedbackController.PlayHapticFeedback(XRNode.RightHand, so);
                    hapticFeedbackController.PlayHapticFeedback(XRNode.LeftHand, so);

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        PreviewLine.padding.left = (int)(dist * 60f);
                        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)PreviewLine.transform);
                    });

                    dist += 1f/28f;

                    Thread.Sleep((int)(so._duration * 1000 + 200));
                }
            }).ContinueWith(x =>
            {
                UpdateGrid();
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateGrid()
        {
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

                Point[] dots = new Point[selectedGraph.Count];
                // Draw circles on the bitmap based on the points
                int i = 0;
                foreach (PointF point in selectedGraph)
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
                    g.DrawLines(linePen, dots);

                    g.DrawLine(linePen, dots[0], new Point(0, dots[0].Y));
                    g.DrawLine(linePen, dots[dots.Length - 1], new Point(bitmap.Width, dots[dots.Length - 1].Y));
                }
                else if (dots.Length == 1)
                {
                    g.DrawLine(linePen, new Point(0, dots[0].Y), new Point(bitmap.Width, dots[0].Y));
                }
                else
                {
                    g.DrawLine(linePen, new Point(0, (int)lineSize), new Point(bitmap.Width, (int)lineSize));
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
            GameplaySetup.Instance.AddTab("Hit Score Rumbler", "HitScoreRumbler.UI.ConfigMenu.bsml", this);
        }

        public void Dispose()
        {
            if (BeatSaberMarkupLanguage.Settings.BSMLSettings.Instance != null)
            {
                BeatSaberMarkupLanguage.Settings.BSMLSettings.Instance.RemoveSettingsMenu(this);
            }
        }
    }
}

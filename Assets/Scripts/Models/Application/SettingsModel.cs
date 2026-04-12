using Newtonsoft.Json;

namespace Models.Application
{
    public class SettingsModel
    {
        public GeneralSettings GeneralSettings { get; set; } = new();

        public ControlsSettings ControlsSettings { get; set; } = new();

        public GraphicsSettings GraphicsSettings { get; set; } = new();

        public AudioSettings AudioSettings { get; set; } = new();

        [JsonIgnore] public bool IsDirty { get; set; }

        public SettingsModel Clone()
        {
            return new SettingsModel
            {
                GeneralSettings = this.GeneralSettings?.Clone(),
                ControlsSettings = this.ControlsSettings?.Clone(),
                GraphicsSettings = this.GraphicsSettings?.Clone(),
                AudioSettings = this.AudioSettings?.Clone()
            };
        }
    }

    public class GeneralSettings
    {
        /// <summary>
        /// 0 = en, 1 = pl, 3 = de, 4 = es, 5 = it, 6 = ja, 7 = ch
        /// </summary>
        public int Language { get; set; } = 1;

        public GeneralSettings Clone()
        {
            return new GeneralSettings
            {
                Language = this.Language
            };
        }
    }

    public class ControlsSettings
    {
        public ControlsSettings Clone()
        {
            return new ControlsSettings
            {
            };
        }
    }

    public class GraphicsSettings
    {
        public int ResolutionWidth { get; set; }

        public int ResolutionHeight { get; set; }

        /// <summary>
        /// 0 = ExclusiveFullScreen, 1 = FullScreenWindow, 2 = MaximizedWindow, 3 = Windowed
        /// </summary>
        public int FullScreenMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint RefreshRate { get; set; }

        /// <summary>
        /// 0 = off, 1 = on
        /// </summary>
        public int VSync { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TargetFrameRate { get; set; }

        /// <summary>
        /// 0 = Low, 1 = Medium, 2 = High, 3 = UltraHigh
        /// </summary>
        public int GraphicQuality { get; set; }

        /// <summary>
        /// 0 (off), 2, 4, 8
        /// </summary>
        public int AntiAliasing { get; set; }

        /// <summary>
        /// 0 = Disable, 1 = Enable, 2 = ForceEnable
        /// </summary>
        public int AnisotropicFiltering { get; set; }

        /// <summary>
        /// 0 = Disable, 1 = HardOnly, 2 = All
        /// </summary>
        public int ShadowQuality { get; set; }

        public GraphicsSettings Clone()
        {
            return new GraphicsSettings()
            {
                ResolutionWidth = this.ResolutionWidth,
                ResolutionHeight = this.ResolutionHeight,
                FullScreenMode = this.FullScreenMode,
                RefreshRate = this.RefreshRate,
                VSync = this.VSync,
                TargetFrameRate = this.TargetFrameRate,
                GraphicQuality = this.GraphicQuality,
                AntiAliasing = this.AntiAliasing,
                AnisotropicFiltering = this.AnisotropicFiltering,
                ShadowQuality = this.ShadowQuality
            };
        }
    }

    public class AudioSettings
    {
        /// <summary>
        /// 0 = min, 1 = max
        /// </summary>
        public float MasterVolume { get; set; }

        /// <summary>
        /// 0 = min, 1 = max
        /// </summary>
        public float SpeechVolume { get; set; }

        /// <summary>
        /// 0 = min, 1 = max
        /// </summary>
        public float EffectsVolume { get; set; }

        /// <summary>
        /// 0 = off, 1 = on
        /// </summary>
        public int Subtitles { get; set; }

        /// <summary>
        /// 0 = small, 1 = medium, 2 = big
        /// </summary>
        public int SubtitlesSize { get; set; }

        public AudioSettings Clone()
        {
            return new AudioSettings()
            {
                MasterVolume = this.MasterVolume,
                SpeechVolume = this.SpeechVolume,
                EffectsVolume = this.EffectsVolume,
                Subtitles = this.Subtitles,
                SubtitlesSize = this.SubtitlesSize
            };
        }
    }
}
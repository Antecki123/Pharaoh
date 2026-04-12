using Controllers.SceneManagment;
using Zenject;

namespace App.Signals
{
    public class ApplicationSignals
    {
        public ApplicationSignals(DiContainer container)
        {
            container.DeclareSignal<GameInitialized>();
            container.DeclareSignal<LoadSceneRequest>();
            container.DeclareSignal<GameSceneLoaded>();

            // GraphicsOptions
            container.DeclareSignal<SetResolution>();
            container.DeclareSignal<SetFullScreenMode>();
            container.DeclareSignal<SetRefreshRate>();
            container.DeclareSignal<SetVSync>();
            container.DeclareSignal<SetTargetFrameRate>();
            container.DeclareSignal<SetQualityLevel>();
            container.DeclareSignal<SetAntiAliasing>();
            container.DeclareSignal<SetAnisotropicFiltering>();
            container.DeclareSignal<SetShadowsQuality>();

            // GeneralOptions
            container.DeclareSignal<SetLanguage>();

            // AudioOptions
            container.DeclareSignal<SetMasterVolume>();
            container.DeclareSignal<SetSpeechVolume>();
            container.DeclareSignal<SetEffectsVolume>();
            container.DeclareSignal<SetSubtitles>();
            container.DeclareSignal<SetSubtitlesSize>();
        }

        public struct GameInitialized { }

        public struct LoadSceneRequest
        {
            public SceneName TargetScene { get; private set; }

            public LoadSceneRequest(SceneName targetScene)
            {
                TargetScene = targetScene;
            }
        }

        public struct GameSceneLoaded
        {
            public SceneName SceneName { get; private set; }

            public GameSceneLoaded(SceneName sceneName)
            {
                SceneName = sceneName;
            }
        }

        public struct SetResolution
        {
            public int Width { get; private set; }
            public int Height { get; private set; }

            public SetResolution(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        public struct SetFullScreenMode
        {
            public int FullscreenMode { get; private set; }

            public SetFullScreenMode(int fullscreenMode)
            {
                FullscreenMode = fullscreenMode;
            }
        }

        public struct SetRefreshRate
        {
            public uint RefreshRate { get; private set; }

            public SetRefreshRate(uint refreshRate)
            {
                RefreshRate = refreshRate;
            }
        }

        public struct SetVSync
        {
            public int VSyncCount { get; private set; }

            public SetVSync(int vSyncCount)
            {
                VSyncCount = vSyncCount;
            }
        }

        public struct SetTargetFrameRate
        {
            public int TargetFrameRate { get; private set; }

            public SetTargetFrameRate(int targetFrameRate)
            {
                TargetFrameRate = targetFrameRate;
            }
        }

        public struct SetQualityLevel
        {
            public int QualityLevel { get; private set; }

            public SetQualityLevel(int qualityLevel)
            {
                QualityLevel = qualityLevel;
            }
        }

        public struct SetAntiAliasing
        {
            public int AntiAliasingLevel { get; private set; }

            public SetAntiAliasing(int antiAliasingLevel)
            {
                AntiAliasingLevel = antiAliasingLevel;
            }
        }

        public struct SetAnisotropicFiltering
        {
            public int Filter { get; private set; }

            public SetAnisotropicFiltering(int filter)
            {
                Filter = filter;
            }
        }

        public struct SetShadowsQuality
        {
            public int ShadowQuality { get; private set; }

            public SetShadowsQuality(int shadowQuality)
            {
                ShadowQuality = shadowQuality;
            }
        }

        public struct SetLanguage
        {
            public int Locale { get; private set; }

            public SetLanguage(int locale)
            {
                Locale = locale;
            }
        }

        public struct SetMasterVolume
        {
            public float Volume { get; private set; }

            public SetMasterVolume(float volume)
            {
                Volume = volume;
            }
        }

        public struct SetSpeechVolume
        {
            public float Volume { get; private set; }

            public SetSpeechVolume(float volume)
            {
                Volume = volume;
            }
        }

        public struct SetEffectsVolume
        {
            public float Volume { get; private set; }

            public SetEffectsVolume(float volume)
            {
                Volume = volume;
            }
        }

        public struct SetSubtitles
        {
            public int Subtitles { get; private set; }

            public SetSubtitles(int subtitles)
            {
                Subtitles = subtitles;
            }
        }

        public struct SetSubtitlesSize
        {
            public int SubtitlesSize { get; private set; }

            public SetSubtitlesSize(int subtitlesSize)
            {
                SubtitlesSize = subtitlesSize;
            }
        }
    }
}
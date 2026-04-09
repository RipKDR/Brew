using System.Collections;
using System.Collections.Generic;
using Brew.Data;
using UnityEngine;

namespace Brew.Presentation
{
    public class AudioManager : MonoBehaviour
    {
        public const int SfxChannelCount = 8;
        public const int MusicStemCount = 4;

        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioConfigSO _audioConfig;

        private AudioSource[] _sfxSources;
        private AudioSource[] _musicSources;
        private readonly int[] _sfxPlayingPriority = new int[SfxChannelCount];
        private readonly Dictionary<SfxId, int> _sfxRoundRobin = new();
        private readonly float[] _stemLevel = new float[MusicStemCount];

        private Coroutine _stopMusicRoutine;

        public AudioConfigSO Config => _audioConfig;

        /// <summary>Uses <see cref="SettingsManager"/> (PlayerPrefs keys Brew_SfxVolume / Brew_MusicVolume / Brew_IsMuted).</summary>
        public static float SfxVolume
        {
            get => SettingsManager.SfxVolume;
            set => SettingsManager.SfxVolume = value;
        }

        public static float MusicVolume
        {
            get => SettingsManager.MusicVolume;
            set => SettingsManager.MusicVolume = value;
        }

        public static bool IsMuted
        {
            get => SettingsManager.IsMuted;
            set => SettingsManager.IsMuted = value;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSources = new AudioSource[SfxChannelCount];
            for (int i = 0; i < SfxChannelCount; i++)
            {
                _sfxSources[i] = gameObject.AddComponent<AudioSource>();
                _sfxSources[i].playOnAwake = false;
                _sfxSources[i].spatialBlend = 0f;
                _sfxPlayingPriority[i] = -1;
            }

            _musicSources = new AudioSource[MusicStemCount];
            for (int i = 0; i < MusicStemCount; i++)
            {
                _musicSources[i] = gameObject.AddComponent<AudioSource>();
                _musicSources[i].playOnAwake = false;
                _musicSources[i].loop = true;
                _musicSources[i].spatialBlend = 0f;
            }
        }

        private void OnEnable() => SettingsManager.OnSettingsChanged += RefreshMusicVolumes;

        private void OnDisable() => SettingsManager.OnSettingsChanged -= RefreshMusicVolumes;

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void PlaySFX(SfxId id, float pitchOffset = 0f)
        {
            if (_audioConfig == null || !_audioConfig.TryGetSfxEntry(id, out AudioConfigSO.SfxEntry entry))
                return;

            if (entry.Clips == null || entry.Clips.Length == 0)
                return;

            if (!_sfxRoundRobin.TryGetValue(id, out int rr))
                rr = 0;
            int clipIdx = rr % entry.Clips.Length;
            _sfxRoundRobin[id] = clipIdx + 1;

            AudioClip clip = entry.Clips[clipIdx];
            if (clip == null)
                return;

            int requestPriority = entry.Priority;
            if (!TryGetSfxSource(requestPriority, out AudioSource source))
                return;

            float semi = pitchOffset;
            if (entry.PitchVariation > 0f)
                semi += Random.Range(-entry.PitchVariation, entry.PitchVariation);
            source.pitch = Mathf.Pow(2f, semi / 12f);

            float volMul = IsMuted ? 0f : SfxVolume;
            source.volume = entry.BaseVolume * volMul;
            source.clip = clip;
            source.priority = Mathf.Clamp(50 + requestPriority * 8, 0, 256);
            source.Play();

            int idx = System.Array.IndexOf(_sfxSources, source);
            if (idx >= 0)
                _sfxPlayingPriority[idx] = requestPriority;
        }

        public void PlayMusic()
        {
            if (_audioConfig == null)
                return;

            if (_stopMusicRoutine != null)
            {
                StopCoroutine(_stopMusicRoutine);
                _stopMusicRoutine = null;
            }

            AssignStem(0, _audioConfig.MelodyClip);
            AssignStem(1, _audioConfig.HarmonyClip);
            AssignStem(2, _audioConfig.PercussionClip);
            AssignStem(3, _audioConfig.FxChimesClip);

            for (int i = 0; i < MusicStemCount; i++)
                _stemLevel[i] = 1f;

            RefreshMusicVolumes();

            for (int i = 0; i < MusicStemCount; i++)
            {
                if (_musicSources[i].clip != null)
                    _musicSources[i].Play();
            }
        }

        public void StopMusic(float fadeTime = 1f)
        {
            if (_stopMusicRoutine != null)
                StopCoroutine(_stopMusicRoutine);
            _stopMusicRoutine = StartCoroutine(StopMusicRoutine(fadeTime));
        }

        public void SetStemVolume(int stemIndex, float volume)
        {
            if (stemIndex < 0 || stemIndex >= MusicStemCount)
                return;
            _stemLevel[stemIndex] = Mathf.Max(0f, volume);
            RefreshMusicVolumes();
        }

        public void SetStemClip(int stemIndex, AudioClip clip)
        {
            if (stemIndex < 0 || stemIndex >= MusicStemCount)
                return;

            AudioSource src = _musicSources[stemIndex];
            bool wasPlaying = src.isPlaying;
            float time = src.time;
            AudioClip previous = src.clip;

            src.clip = clip;

            if (wasPlaying && clip != null)
            {
                float len = clip.length;
                if (len > 0.001f)
                {
                    float prevLen = previous != null ? previous.length : len;
                    src.time = prevLen > 0.001f ? time % prevLen % len : time % len;
                }

                src.Play();
            }
        }

        public AudioSource GetMusicSource(int stemIndex)
        {
            if (stemIndex < 0 || stemIndex >= MusicStemCount)
                return null;
            return _musicSources[stemIndex];
        }

        public void DuckMusic(float targetDb, float duckDuration, float restoreDuration)
        {
            StartCoroutine(DuckMusicRoutine(targetDb, duckDuration, restoreDuration));
        }

        public void RefreshMusicVolumes()
        {
            if (_musicSources == null || _audioConfig == null)
                return;

            float global = IsMuted ? 0f : MusicVolume * _audioConfig.MusicBaseVolume;
            for (int i = 0; i < MusicStemCount; i++)
                _musicSources[i].volume = global * _stemLevel[i];
        }

        private IEnumerator StopMusicRoutine(float fadeTime)
        {
            fadeTime = Mathf.Max(0.01f, fadeTime);
            float[] start = new float[MusicStemCount];
            for (int i = 0; i < MusicStemCount; i++)
                start[i] = _musicSources[i].volume;

            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeTime);
                for (int i = 0; i < MusicStemCount; i++)
                    _musicSources[i].volume = Mathf.Lerp(start[i], 0f, t);
                yield return null;
            }

            for (int i = 0; i < MusicStemCount; i++)
            {
                _musicSources[i].Stop();
                _musicSources[i].volume = 0f;
            }

            _stopMusicRoutine = null;
        }

        private IEnumerator DuckMusicRoutine(float targetDb, float duckDuration, float restoreDuration)
        {
            float duckMultiplier = Mathf.Pow(10f, targetDb / 20f);
            float[] original = new float[MusicStemCount];
            for (int i = 0; i < MusicStemCount; i++)
                original[i] = _musicSources[i].volume;

            for (int i = 0; i < MusicStemCount; i++)
                _musicSources[i].volume *= duckMultiplier;

            yield return new WaitForSeconds(duckDuration);

            float elapsed = 0f;
            float[] ducked = new float[MusicStemCount];
            for (int i = 0; i < MusicStemCount; i++)
                ducked[i] = _musicSources[i].volume;

            restoreDuration = Mathf.Max(0.01f, restoreDuration);
            while (elapsed < restoreDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / restoreDuration);
                for (int i = 0; i < MusicStemCount; i++)
                    _musicSources[i].volume = Mathf.Lerp(ducked[i], original[i], t);
                yield return null;
            }

            for (int i = 0; i < MusicStemCount; i++)
                _musicSources[i].volume = original[i];
        }

        private bool TryGetSfxSource(int requestPriority, out AudioSource source)
        {
            source = null;
            for (int i = 0; i < SfxChannelCount; i++)
            {
                if (!_sfxSources[i].isPlaying)
                {
                    source = _sfxSources[i];
                    return true;
                }
            }

            int victimIdx = -1;
            int worstPriority = int.MinValue;
            for (int i = 0; i < SfxChannelCount; i++)
            {
                int p = _sfxPlayingPriority[i];
                if (p < 0)
                    p = 999;
                if (p > worstPriority)
                {
                    worstPriority = p;
                    victimIdx = i;
                }
            }

            if (victimIdx < 0)
                return false;

            if (requestPriority > worstPriority)
                return false;

            _sfxSources[victimIdx].Stop();
            source = _sfxSources[victimIdx];
            _sfxPlayingPriority[victimIdx] = -1;
            return true;
        }

        private void AssignStem(int index, AudioClip clip)
        {
            _musicSources[index].clip = clip;
        }

        private void LateUpdate()
        {
            for (int i = 0; i < SfxChannelCount; i++)
            {
                if (!_sfxSources[i].isPlaying)
                    _sfxPlayingPriority[i] = -1;
            }
        }
    }
}

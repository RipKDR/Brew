using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Brew.Data
{
    public interface IAudioConfigSO
    {
        AudioClip MelodyClip { get; }
        AudioClip HarmonyClip { get; }
        AudioClip PercussionClip { get; }
        AudioClip FxChimesClip { get; }
        AudioClip PercussionTenseClip { get; }
        AudioClip HeartbeatClip { get; }
        global::System.Single MusicBaseVolume { get; }

        global::System.Boolean TryGetSfxEntry(SfxId id, out AudioConfigSO.SfxEntry entry);
    }


    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Brew/Config/Audio Config")]
    public class AudioConfigSO : ScriptableObject, IAudioConfigSO

    {
        public interface ISfxEntry
        {
            System.Single BaseVolume { get; set; }
        }

        public interface ISfxEntry1
        {
            System.Single BaseVolume { get; set; }
        }

        [Serializable]
        public struct SfxEntry : ISfxEntry, ISfxEntry1

        {
            public SfxId Id;
            public AudioClip[] Clips;
            [Range(0f, 1f)] private float baseVolume = 1f;
            [Tooltip("Random pitch offset range in semitones (±).")]
            public float PitchVariation;
            [Tooltip("Lower value = higher priority for voice stealing.")]
            public int Priority;

            public global::System.Single BaseVolume { get => baseVolume; set => baseVolume = value; }

        }

        [SerializeField] private SfxEntry[] _sfxEntries;

        [Header("Music Stems")]
        [SerializeField, FormerlySerializedAs("MelodyStem")] private AudioClip _melodyClip;
        [SerializeField, FormerlySerializedAs("HarmonyPadStem")] private AudioClip _harmonyClip;
        [SerializeField, FormerlySerializedAs("PercussionStem")] private AudioClip _percussionClip;
        [SerializeField, FormerlySerializedAs("FxChimesStem")] private AudioClip _fxChimesClip;

        [Header("Music Stems - Tension")]
        [SerializeField, FormerlySerializedAs("PercussionTenseStem")] private AudioClip _percussionTenseClip;
        [SerializeField, FormerlySerializedAs("HeartbeatBassStem")] private AudioClip _heartbeatClip;

        [SerializeField, FormerlySerializedAs("MusicBaseVolume")]
        [Range(0f, 1f)]
        private float _musicBaseVolume = 0.6f;

        public AudioClip MelodyClip => _melodyClip;
        public AudioClip HarmonyClip => _harmonyClip;
        public AudioClip PercussionClip => _percussionClip;
        public AudioClip FxChimesClip => _fxChimesClip;
        public AudioClip PercussionTenseClip => _percussionTenseClip;
        public AudioClip HeartbeatClip => _heartbeatClip;
        public float MusicBaseVolume => _musicBaseVolume;

        public bool TryGetSfxEntry(SfxId id, out SfxEntry entry)
        {
            entry = default;
            if (_sfxEntries == null)
                return false;

            for (int i = 0; i < _sfxEntries.Length; i++)
            {
                if (_sfxEntries[i].Id == id)
                {
                    entry = _sfxEntries[i];
                    return true;
                }
            }

            return false;
        }
    }
}

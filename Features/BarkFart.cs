using BarkFart.Core;
using BarkFart.Libraries.RadiumWrapper;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BarkFart.Features
{
    [Instance.AddOnAwake]
    internal class BarkFart : MonoWrap
    {
        internal static BarkFart Instance;

        internal static AudioClip Bark;
        internal static AudioClip Fart;

        public void Awake()
        {
            Instance = this;

            Bark = LoadSoundFromResource("BarkFart.Resources.bark.wav");
            Fart = LoadSoundFromResource("BarkFart.Resources.fart.wav");
        }

        internal class ThrownObject : MonoWrap
        {
            internal float speedVar;

            internal AudioSource SoundSource()
            {
                var ssObject = transform.Find("BarkFartSource")?.gameObject;
                if (ssObject == null)
                {
                    ssObject = new GameObject("BarkFartSource");
                    ssObject.transform.SetParent(transform, false);
                }

                var audioSrc = ssObject.GetComponent<AudioSource>();
                if (audioSrc == null)
                    audioSrc = ssObject.AddComponent<AudioSource>();

                audioSrc.volume = 1f;
                audioSrc.spatialBlend = 1f;
                return audioSrc;
            }

            internal void Awake()
            {
                speedVar = UnityEngine.Random.Range(-0.3f, 0.3f);
                SoundSource().pitch = 1f + speedVar;
                SoundSource().PlayOneShot(Bark);
            }

            internal bool collided;
            internal void OnCollisionEnter()
            {
                if (collided)
                    return;

                SoundSource().pitch = 1f - speedVar;
                SoundSource().PlayOneShot(Fart);

                Destroy(this);
                collided = true;
            }
        }

        private Tool previousLeft;
        private Tool previousRight;

        public void Update()
        {
            var leftTool = PlayerRoot.LeftHand.OIDGJKOGAIJ;
            var rightTool = PlayerRoot.RightHand.OIDGJKOGAIJ;

            if (leftTool == null && previousLeft != null)
            {
                if (previousLeft.gameObject.name.Contains("[Basketball]"))
                    previousLeft.gameObject.AddComponent<ThrownObject>();
            }

            if (rightTool == null && previousRight != null)
            {
                if (previousRight.gameObject.name.Contains("[Basketball]"))
                    previousRight.gameObject.AddComponent<ThrownObject>();
            }

            previousLeft = leftTool;
            previousRight = rightTool;
        }

        // Thank the god ChatGPT for the WAV formula
        public static AudioClip LoadWav(byte[] wavFile)
        {
            int channels = BitConverter.ToInt16(wavFile, 22);
            int sampleRate = BitConverter.ToInt32(wavFile, 24);

            // Find the "data" chunk
            int dataChunkOffset = 12;
            int dataSize = 0;
            while (dataChunkOffset < wavFile.Length)
            {
                string chunkID = System.Text.Encoding.ASCII.GetString(wavFile, dataChunkOffset, 4);
                int chunkSize = BitConverter.ToInt32(wavFile, dataChunkOffset + 4);

                if (chunkID == "data")
                {
                    dataSize = chunkSize;
                    dataChunkOffset += 8;
                    break;
                }
                dataChunkOffset += 8 + chunkSize;
            }

            int sampleCount = dataSize / 2; // 16-bit PCM = 2 bytes per sample
            float[] audioData = new float[sampleCount];

            int offset = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(wavFile, dataChunkOffset + i * 2);
                audioData[offset++] = sample / 32768f;
            }

            AudioClip clip = AudioClip.Create("EmbeddedClip", sampleCount / channels, channels, sampleRate, false);
            clip.SetData(audioData, 0);
            return clip;
        }


        public static Dictionary<string, AudioClip> audioPool = new Dictionary<string, AudioClip> { };
        public static AudioClip LoadSoundFromResource(string resourceName)
        {
            AudioClip sound = null;

            if (!audioPool.ContainsKey(resourceName))
            {
                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debug.LogError($"Failed to load resource stream for '{resourceName}'.");
                        return null;
                    }

                    byte[] wavData = new byte[stream.Length];
                    stream.Read(wavData, 0, wavData.Length);
                    sound = LoadWav(wavData);

                    sound.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    UnityEngine.Object.DontDestroyOnLoad(sound);
                    audioPool.Add(resourceName, sound);
                }
            }
            else
            {
                sound = audioPool[resourceName];
            }

            return sound;
        }

        private static GameObject audiomgr = null;
        public static void Play2DAudio(AudioClip sound, float volume)
        {
            if (audiomgr == null)
            {
                audiomgr = new GameObject("2DAudioMgr");
                AudioSource temp = audiomgr.AddComponent<AudioSource>();
                temp.spatialBlend = 0f;
            }
            AudioSource ausrc = audiomgr.GetComponent<AudioSource>();
            ausrc.volume = volume;
            ausrc.PlayOneShot(sound);
        }
    }
}

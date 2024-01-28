using System.Linq;
using UnityEngine;

public class SoundInterface : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        public float volume;
        [Range(0, 1)] public float pitchVariation;
    }

    [SerializeField] bool thwapOnHit;
    public Sound[] sounds;

    public void Play(string soundName)
    {
        Sound sound = sounds.First(sound => sound.name == soundName);
        AudioManager.PlaySFXAtTransform(sound.clip, transform, sound.volume, 1 + Random.Range(-sound.pitchVariation, sound.pitchVariation));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (thwapOnHit) Play("Thwap");
    }
}

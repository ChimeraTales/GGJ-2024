using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static void PlaySFX(AudioClip clip, GameObject sourceObject, bool loop = false)
    {
        AudioSource newSource = sourceObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.loop = loop;
        newSource.Play();
        Destroy(newSource, clip.length);
    }

    public static void PlaySFXAtTransform(AudioClip clip, Transform transform, float volume = 1, float pitch = 1)
    {
        GameObject newObject = new("AudioSource");
        newObject.transform.position = transform.position;
        AudioSource newSource = newObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.volume = volume;
        newSource.pitch = pitch;
        newSource.Play();
        Destroy(newObject, clip.length);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void PlaySFX(AudioClip clip, GameObject sourceObject, bool loop = false)
    {
        AudioSource newSource = sourceObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.loop = loop;
        newSource.Play();
        Destroy(newSource, clip.length);
    }
}

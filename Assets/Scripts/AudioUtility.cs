using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


//命名给我命   _Audio  听见没你这家伙
public class AudioUtility : MonoBehaviour
{
    public AudioSource source;

    public void PlayClipOneShot(AudioClip clip, float volume = 1.0f)
    {
        if (source == null) return;
        source.PlayOneShot(clip, volume);
    }
    public void PlayClip(AudioClip clip, float volume = 1.0f)
    {
        if(source == null) { return; }
        if(source.clip == clip)
        {
            source.volume = volume;
            source.Play();
            return;
        }
        else if(source.isPlaying)
        {
            source.Stop();
        }

        source.clip = clip;
        source.volume = volume;
        source.Play();
    }
    public void PlayLoopClip(AudioClip clip, float volume = 1.0f)
    {
        if (source == null) { return; }
        if (source.clip == clip && source.isPlaying)
        {
            source.volume = volume;
            return;
        }
        else if (source.clip == clip)
        {
            source.volume = volume;
            source.Play();
            return;
        }
        else if (source.isPlaying)
        {
            source.Stop();
        }

        source.clip = clip;
        source.volume = volume;
        source.Play();
    }

    public void StopPlay()
    {
        if(source != null && source.isPlaying)
        {
            source.Stop();
        }
        else
        {
            return;
        }
    }
}

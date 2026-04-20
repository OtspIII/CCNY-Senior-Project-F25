using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioLibrary : MonoBehaviour
{
    // Thanks, Misha!!
    public static AudioLibrary Instance;
    public List<SoundType> Sounds;
    public AudioSource Audio;
    public Dictionary<Sfx, AudioClip> SoundDict = new Dictionary<Sfx, AudioClip>();

    void Awake()
    {
        Instance = this;
        foreach (SoundType s in Sounds)
        {
            SoundDict.Add(s.Type, s.Clip);
        }
    }

    public AudioClip GetAudio(Sfx a)
    {
        if (SoundDict.ContainsKey(a)) return SoundDict[a];
        return null;
    }

    public void PlaySound(Sfx a)
    {
        if (!Audio.isPlaying) Audio.PlayOneShot(GetAudio(a));
    }

}

[System.Serializable]
public class SoundType
{
    public Sfx Type;
    public AudioClip Clip;
}

public enum Sfx
{
    None = 0,
    Walk = 1,
    Dash = 2
}
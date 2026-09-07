using System;
using UnityEngine;

// Class that stores a list of audio clips and provide public API
[Serializable]
public class AudioClipBank
{
    [SerializeField] AudioClip[] clips;

    public bool HasClips
    {
        get
        {
            if (clips == null)
                return false;

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                    return true;
            }

            return false;
        }
    }

    public AudioClip GetRandom(AudioClip previousClip = null)
    {
        if (clips == null || clips.Length == 0)
            return null;

        int validCount = 0;

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i] != previousClip)
                validCount++;
        }

        if (validCount > 0)
            return GetRandomValidClip(previousClip);

        return GetRandomValidClip(null);
    }

    private AudioClip GetRandomValidClip(AudioClip excludedClip)
    {
        int count = 0;

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i] != excludedClip)
                count++;
        }

        if (count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, count);

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] == null || clips[i] == excludedClip)
                continue;

            if (randomIndex == 0)
                return clips[i];

            randomIndex--;
        }

        return null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    private Dictionary<string, AudioSource> SFX = new();

    // Start is called before the first frame update
    void Start()
    {
        // Load all audio clips from the specified folder
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/SFX");

        // Create an AudioSource for each clip
        foreach (AudioClip clip in clips)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = clip;
            newSource.loop = false;
            newSource.playOnAwake = false;
            SFX.Add(clip.name, newSource);

            // Configure default settings
            newSource.playOnAwake = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play(string name)
    {
        if (SFX.TryGetValue(name, out AudioSource source))
        {
            source.Play();
        }
    }
}

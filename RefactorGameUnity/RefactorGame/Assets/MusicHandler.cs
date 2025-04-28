using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicHandler : MonoBehaviour
{
    private Dictionary<string, AudioSource> Music = new();

    // Start is called before the first frame update
    void Start()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/Music");

        foreach (AudioClip clip in clips)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = clip;
            newSource.playOnAwake = false;
            Music.Add(clip.name, newSource);

            newSource.playOnAwake = false;
        }

        switch (SceneManager.GetActiveScene().name)
        {
            case "MainMenu":
                PlayMusic("AutumnDay");
                break;
            case "LevelSelect":
                PlayMusic("Hustle");
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMusic(string name)
    {
        if (Music.TryGetValue(name, out AudioSource source))
        {
            source.Play();
        }
    }
}

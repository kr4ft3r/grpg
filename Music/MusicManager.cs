using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip SongRaindropGentle;

    public AudioSource AudioSource;
    private float _switchTimePassed = 0;
    private float _switchLengthSlow = 4f;

    public AudioClip CurrentSong { get; private set; }
    public AudioClip NextSong { get { return _nextSong; } set
        {
            _nextSong = value;
            _switchTimePassed = 0;
        } }
    private AudioClip _nextSong;

    public void StartPlaying()
    {
        if (CurrentSong == null) return;

        AudioSource.clip = CurrentSong;
        AudioSource.Play();
    }

    private void Awake()
    {
        CurrentSong = null;
        NextSong = null;
        AudioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("MusicManagerUpdate");
        if (NextSong != null)
        {
            float volume;
            if (_switchTimePassed < _switchLengthSlow)
            {
                volume = Mathf.Lerp(1.0f, 0.0f, _switchTimePassed / _switchLengthSlow);
                _switchTimePassed += Time.deltaTime;
            } else
            {
                volume = 1.0f;
                CurrentSong = NextSong;
                NextSong = null;
                _switchTimePassed = 0;
                StartPlaying();
            }
            AudioSource.volume = volume;
            
        }
    }
}

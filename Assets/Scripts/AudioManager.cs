using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    FMOD.Studio.EventInstance SFXVolumeTestEvent;
    
    FMOD.Studio.Bus Music;
    FMOD.Studio.Bus SFX;
    FMOD.Studio.Bus Master;

    float MusicVolume = 0.4f;
    float SFXVolume = 0.7f;
    float MasterVolume = 1f;

    private void Awake()
    {

        Music = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        SFX = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");
        Master = FMODUnity.RuntimeManager.GetBus("bus:/Master");
        SFXVolumeTestEvent = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/SFXVolumeTest");
    }

    public void SetMasterVolumeLevel(float volume)
    {
        MasterVolume = volume;
        Master.setVolume(MasterVolume);
    }

    public void SetMusicVolumeLevel(float volume)
    {
        MusicVolume = volume;
        Music.setVolume(MusicVolume);
    }

    public void SetSFXVolumeLevel(float volume)
    {
        SFXVolume = volume;
        SFX.setVolume(SFXVolume);
        FMOD.Studio.PLAYBACK_STATE pbState;
        SFXVolumeTestEvent.getPlaybackState(out pbState);
        if (pbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            SFXVolumeTestEvent.start();
        }
    }

}

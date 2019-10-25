using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    static AudioManager current;

    [Header("Ambient Audio")]
    public AudioClip ambientClip;		//The background ambient sound
    public AudioClip musicClip;			//The background music 
    public AudioClip ambientShdowWorldClip;

    [Header("Player Audio")]
    public AudioClip[] walkStepClips;

    [Header("SFX Audio")]
    public AudioClip droppedCrash;
    public AudioClip wallSlide;
    public AudioClip doubleJump;
    public AudioClip evade;

    [Header("Attack Audio")]
    public AudioClip[] swordAttack;
    public AudioClip swordImpact;

    [Header("Mixer Groups")]
    public AudioMixerGroup ambientGroup;
    public AudioMixerGroup ambientShadowWorldGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup playerGroup;
    public AudioMixerGroup particlesGroup;
    public AudioMixerGroup SFXGroup;

    [Header("Audio Snapshots")]
    public AudioMixerSnapshot physicWorld;
    public AudioMixerSnapshot shadowWorld;

    AudioSource playerSource;
    public static AudioSource ambientSource;
    public static AudioSource ambientShadowWorldSource;
    AudioSource musicSource;
    public static AudioSource particlesSource;
    AudioSource sfxSource;

    void Awake()
    {
        if (current != null && current != this)
        {
            //...destroy this. There can be only one AudioManager
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);

        ambientSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        ambientShadowWorldSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        musicSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        playerSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        particlesSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        sfxSource = gameObject.AddComponent<AudioSource>() as AudioSource;

        ambientSource.outputAudioMixerGroup = ambientGroup;
        ambientShadowWorldSource.outputAudioMixerGroup = ambientShadowWorldGroup;
        musicSource.outputAudioMixerGroup = musicGroup;
        playerSource.outputAudioMixerGroup = playerGroup;
        particlesSource.outputAudioMixerGroup = particlesGroup;
        sfxSource.outputAudioMixerGroup = SFXGroup;

        StartLevelAudio();
    }

    private void Update()
    {
        if (!current.sfxSource.isPlaying)
            current.sfxSource.volume = 1f;
    }

    void StartLevelAudio()
    {
        //Set the clip for ambient audio, tell it to loop, and then tell it to play
        ambientSource.clip = current.ambientClip;
        ambientSource.loop = true;
        ambientSource.Play();

        //Set the clip for music audio, tell it to loop, and then tell it to play
        current.musicSource.clip = current.musicClip;
        current.musicSource.loop = true;
        current.musicSource.Play();
    }

    public static IEnumerator PlayAmbientShadowWorld()
    {
        ambientSource.loop = true;

        //Switch to shadow world
        if (GameManager.inTheShadowWorld)
        {
            ambientShadowWorldSource.clip = current.ambientShdowWorldClip;

            if (!ambientShadowWorldSource.isPlaying)
            {
                ambientShadowWorldSource.volume = .01f;
                ambientShadowWorldSource.Play();
            }

            current.shadowWorld.TransitionTo(0.1f);                             //Switch snapshot in audio mixer
        }
        //Switch to physic world
        else
        {
            ambientSource.Play();
            current.physicWorld.TransitionTo(0.1f);

            while (ambientSource.volume < 1)
            {
                yield return new WaitForEndOfFrame();

                ambientSource.volume += .01f;
                ambientShadowWorldSource.volume -= .01f;

                if(ambientShadowWorldSource.volume <= 0)
                    ambientShadowWorldSource.Stop();
            }
            
        }
    }

    public static void PlaySlideWallAudio()
    {
        if (current == null || particlesSource.isPlaying)
            return;

        particlesSource.clip = current.wallSlide;
        particlesSource.Play();
    }

    public static void PlayDroppedCrashAudio()
    {
        if (current == null)
            return;

        //particlesSource.clip = current.droppedCrash;
        particlesSource.PlayOneShot(current.droppedCrash);
    }

    public static void PlayFootstepAudio()
    {
        //If there is no current AudioManager or the player source is already playing
        //a clip, exit 
        if (current == null || current.playerSource.isPlaying)
            return;

        //Pick a random footstep sound
        int index = Random.Range(0, current.walkStepClips.Length);

        //Set the footstep clip and tell the source to play
        current.playerSource.clip = current.walkStepClips[index];
        current.playerSource.Play();
    }

    public static void PlaySwordAttackAudio()
    {
        if (current == null)
            return;

        //Pick a random footstep sound
        int index = Random.Range(0, current.swordAttack.Length);

        //Set the footstep clip and tell the source to play
        current.sfxSource.PlayOneShot(current.swordAttack[index]);
    }

    public static void PlayEvadeAudio()
    {
        if (current == null)
            return;

        //Set the footstep clip and tell the source to play
        //current.sfxSource.clip = current.evade;
        //current.sfxSource.volume = .25f;
        current.sfxSource.PlayOneShot(current.evade, .25f);
    }

    public static void PlayDoubleJumpAudio()
    {
        if (current == null)
            return;

        //Set the footstep clip and tell the source to play
        //current.sfxSource.clip = current.doubleJump;
        //current.sfxSource.volume = .25f;
        current.sfxSource.PlayOneShot(current.doubleJump, .25f);
    }

    public static void PlaySwordImpactAudio()
    {
        if (current == null)
            return;

        current.sfxSource.PlayOneShot(current.swordImpact);
    }
}

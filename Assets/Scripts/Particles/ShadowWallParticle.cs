using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowWallParticle : MonoBehaviour
{
    ParticleSystem particle;

    bool shadowWorld;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        shadowWorld = GameManager.inTheShadowWorld;
    }

    void Update()
    {
        if (GameManager.inTheShadowWorld)
        {
            if (particle.isPlaying)
                particle.Stop();
        }
        else
        {
            if (particle.isStopped)
                particle.Play();
        }
    }
}

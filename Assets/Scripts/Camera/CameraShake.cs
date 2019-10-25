using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = .1f;
    public float shakeAmplitude = 1.2f;
    public float shakeFrequency = 2f;
    public bool shakeThis;

    float _shakeElapsedTime;

    //Cinemachine shake
    public CinemachineVirtualCamera virtualCamera;
    CinemachineBasicMultiChannelPerlin virtualCameraNoise;

    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        virtualCameraNoise = virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
    }
    
    void Update()
    {
        //TODO: Replace with your trigger
        if(shakeThis)
        {
            _shakeElapsedTime = shakeDuration;
            shakeThis = false;
        }

        if(virtualCamera != null || virtualCameraNoise != null)
        {
            if(_shakeElapsedTime > 0)
            {
                virtualCameraNoise.m_AmplitudeGain = shakeAmplitude;
                virtualCameraNoise.m_FrequencyGain = shakeFrequency;

                _shakeElapsedTime -= Time.deltaTime;
            }
            else
            {
                virtualCameraNoise.m_AmplitudeGain = 0f;
                _shakeElapsedTime = 0f;
            }
        }
    }

    public void Shake(float duration, float amplitude, float frequency)
    {
        _shakeElapsedTime = duration;

        if(virtualCamera != null || virtualCameraNoise != null)
        {
            if(_shakeElapsedTime > 0)
            {
                virtualCameraNoise.m_AmplitudeGain = amplitude;
                virtualCameraNoise.m_FrequencyGain = frequency;

                _shakeElapsedTime -= Time.deltaTime;
            }
            else
            {
                virtualCameraNoise.m_AmplitudeGain = 0f;
                _shakeElapsedTime = 0f;
            }
        }
    }
}

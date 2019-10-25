using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAtributes : MonoBehaviour
{
    public int health = 10;
    public int attack = 2;

    public float blinkingDuration = 0.4f;
    public float blinkingDelay = .1f;

    SpriteRenderer sprite;
    PlayerMovement movement;
    CameraShake camShake;

    bool _takeDamageDelay;
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        movement = GetComponent<PlayerMovement>();

        camShake = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>().GetComponent<CameraShake>();
    }
    
    void Update()
    {
        if (health <= 0)
        {
            Physics2D.IgnoreLayerCollision(10, 12);
            movement.isDead = true;
        }
    }

    public void TakeDamage(int damage)
    {
        if (movement.isEvading)
            return;

        health -= damage;
        camShake.Shake(.4f, 5f, 8f);
        AudioManager.PlaySwordImpactAudio();
        sprite.color = Color.red;
        StartCoroutine(SwitchSpriteColor(blinkingDuration));
    }

    IEnumerator SwitchSpriteColor(float duration)
    {
        _takeDamageDelay = true;

        for (int i = 0; duration > 0f; duration -= blinkingDelay, i++)
        {
            yield return new WaitForSeconds(blinkingDelay);

            sprite.color = i % 2 == 0 ?  Color.white : Color.red;
        }
        _takeDamageDelay = false;
    }
}

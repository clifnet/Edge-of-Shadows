using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkFlip : MonoBehaviour
{
    PlayerMovement movement;
    PlayerInput input;
    ParticleSystem particle;
    int direction;

    // Start is called before the first frame update
    void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
        input = GetComponentInParent<PlayerInput>();
        particle = GetComponent<ParticleSystem>();

        transform.localPosition = new Vector3(transform.localPosition.x * movement.direction, transform.localPosition.y, transform.localPosition.z);
    }

    //private void Start()
    //{
    //    if (movement.direction != direction)
    //    {
    //        transform.localPosition = new Vector3(transform.localPosition.x * movement.direction, transform.localPosition.y, transform.localPosition.z);
    //        direction = movement.direction;
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        if (!movement.isWallRun && movement.isGrabing && input.crouchHeld)
        {
            particle.Play();
        }
        else
            particle.Stop();


    }
}

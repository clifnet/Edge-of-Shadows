using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimations : MonoBehaviour
{
    Rigidbody2D rigidBody;      //Reference to the Rigidbody2D component
    Animator anim;              //Reference to the Animator component
    Enemy enemy;
    SpriteRenderer sprite;

    int _shadowPatrolParamID;
    int _attackParamID;
    int _searchParamID;
    int _deadParamID;
    int _speedParamID;

    bool _isDead;

    void Start()
    {
        //Get the integer hashes of the parameters. This is much more efficient
        //than passing strings into the animator
        _shadowPatrolParamID = Animator.StringToHash("isShadowPatrol");
        _attackParamID = Animator.StringToHash("isAttacking");
        _searchParamID = Animator.StringToHash("isSearching");
        _deadParamID = Animator.StringToHash("isDead");

        _speedParamID = Animator.StringToHash("speed");

        //Get references to the needed components
        rigidBody = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        //If any of the needed components don't exist...
        if (rigidBody == null || anim == null)
        {
            //...log an error and then remove this component
            Debug.LogError("A needed component is missing from the enemy");
            Destroy(this);
        }
    }

    void Update()
    {
        //Update the Animator with the appropriate values
        anim.SetBool(_attackParamID, enemy.isAttacking);
        anim.SetBool(_searchParamID, enemy.isSearching);
        anim.SetBool(_deadParamID, enemy.isDead);
        anim.SetBool(_shadowPatrolParamID, enemy._shadowTag != GameManager.inTheShadowWorld);

        anim.SetFloat(_speedParamID, enemy.speed);

        BlurPhysicEnemies();
    }

    ///<summary>
    ///Switch color and alpha for physic enemies
    ///</summary>
    void BlurPhysicEnemies()
    {
        if (!enemy._shadowTag)
            if (enemy._shadowTag != GameManager.inTheShadowWorld)
            {
                if (sprite.color == Color.white)
                {
                    float alpha = .3f;
                    sprite.color = Color.black * alpha;
                }
            }
            else
            {
                if (sprite.color != Color.white)
                    sprite.color = Color.white;
            }
    }

    //This method is called from events in the animation itself. This keeps the footstep
    //sounds in sync with the visuals
    public void StepAudio()
    {
        //Tell the Audio Manager to play a footstep sound
        AudioManager.PlayFootstepAudio();
    }

    //This method is called from events in the animation itself. This keeps the footstep
    //sounds in sync with the visuals
    public void CrouchStepAudio()
    {
        //Tell the Audio Manager to play a crouching footstep sound
        //AudioManager.PlayCrouchFootstepAudio();
    }
}

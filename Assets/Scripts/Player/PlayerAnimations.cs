// This script controls the animations of the Robbie player character. Normally, most
// of this functionality would be added to the PlayerMovement script instead of having
// its own script since that would be more efficient. For the purposes of learning,
// however, this functionality was separated out.

using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    PlayerAttack attacking;
    PlayerMovement movement;    //Reference to the PlayerMovement script component
    Rigidbody2D rigidBody;      //Reference to the Rigidbody2D component
    PlayerInput input;          //Reference to the PlayerInput script component
    Animator anim;              //Reference to the Animator component
    
    int _grabingParamID;         //ID of the isHanging parameter
    int _wallRunParamID;
    int _groundParamID;          //ID of the isOnGround parameter
    int _crouchParamID;          //ID of the isCrouching parameter
    int _climbingParamID;        //ID of the isClimbing parameter
    int _attackingParamID;
    int _hurtParamID;
    int _doubleJumpParamID;           //ID of the isDoubleJump parameter
    int _deathParamID;
    int _speedParamID;           //ID of the speed parameter
    int _fallParamID;            //ID of the verticalVelocity parameter
    int _comboParamID;
    int _airComboParamID;
    int _evadingParamID;



    void Start()
    {
        //Get the integer hashes of the parameters. This is much more efficient
        //than passing strings into the animator
        _grabingParamID         = Animator.StringToHash("isGrabing");
        _wallRunParamID         = Animator.StringToHash("isWallRun");
        _groundParamID          = Animator.StringToHash("isOnGround");
        _crouchParamID          = Animator.StringToHash("isCrouching");
        _climbingParamID        = Animator.StringToHash("isClimbing");
        _attackingParamID       = Animator.StringToHash("isAttacking");
        _doubleJumpParamID      = Animator.StringToHash("isDoubleJump");
        _hurtParamID            = Animator.StringToHash("isHurt");
        _evadingParamID         = Animator.StringToHash("isEvading");
        _deathParamID           = Animator.StringToHash("isDead");
        _speedParamID           = Animator.StringToHash("speed");
        _fallParamID            = Animator.StringToHash("verticalVelocity");
        _comboParamID           = Animator.StringToHash("combo");
        _airComboParamID        = Animator.StringToHash("airCombo");


        //Get references to the needed components
        attacking               = GetComponentInChildren<PlayerAttack>();
        movement                = GetComponent<PlayerMovement>();
        rigidBody               = GetComponent<Rigidbody2D>();
        input                   = GetComponent<PlayerInput>();
        anim                    = GetComponent<Animator>();

        //If any of the needed components don't exist...
        if (movement == null || rigidBody == null || input == null || anim == null)
        {
            //...log an error and then remove this component
            Debug.LogError("A needed component is missing from the player");
            Destroy(this);
        }
    }

    void Update()
    {
        //Update the Animator with the appropriate values
        anim.SetBool(_grabingParamID, movement.isGrabing);
        anim.SetBool(_groundParamID, movement.isOnGround);
        anim.SetBool(_crouchParamID, movement.isCrouching);
        anim.SetBool(_climbingParamID, movement.isClimbing);
        anim.SetBool(_doubleJumpParamID, movement.isDoubleJump);
        anim.SetBool(_hurtParamID, movement.isHurt);
        anim.SetBool(_wallRunParamID, movement.isWallRun);
        anim.SetBool(_attackingParamID, movement.isAttacking);
        anim.SetBool(_evadingParamID, movement.isEvading);
        anim.SetBool(_deathParamID, movement.isDead);

        anim.SetInteger(_comboParamID, attacking.combo);
        anim.SetInteger(_airComboParamID, attacking.airCombo);

        anim.SetFloat(_fallParamID, rigidBody.velocity.y);
        
        //Use the absolute value of speed so that we only pass in positive numbers
        anim.SetFloat(_speedParamID, Mathf.Abs(input.horizontal));
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

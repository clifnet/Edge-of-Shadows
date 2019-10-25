using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool drawDebugRaycast = true;

    [Header("Movement Properties")]
    public float speed              = 8f;
    public float crouchSpeedDivisor = 3f;
    public float coyoteDuration     = .05f; //Задержка перед падением с платформы
    public float maxFallSpeed       = -25f;
    public float wallSlideMaxSpeed  = 1f;
    public float climbingSpeed      = 4f;
    public float wallRunSpeed       = 6f;

    [Header("Jump Properties")]
    public float jumpForce          = 6.3f;
    public float jumpHoldForce      = 2.2f;
    public float jumpHoldDuration   = .1f;
    public float wallJumpDelay      = .2f;
    public int extraJumps = 1;

    [Header("Environment Check Properties")]
    public float footOffset         = .3f;
    public float headClearence      = 1f;
    public float groundDistance     = .2f;
    public float grabDistance       = .2f;
    public float grabingOffDuration = .3f;
    public float wallRunDuration    = .4f;
    public float wallRunDelay       = .2f;
    public float evadingDistance    = 14f;
    public float evadingDuration    = .35f;
    public float evadingCooldown    = 1f;
    public LayerMask groundLayer    = 1 << 9; //9 - Platform layer

    [Header("Status Flags")]
    public bool isOnGround;
    public bool isJumping;
    public bool isDoubleJump;
    public bool isCrouching;
    public bool isHeadBlocked;
    public bool isGrabing;
    public bool isClimbing;
    public bool isHurt;
    public bool isWallRun;
    public bool isDropping;
    public bool isAttacking;
    public bool isEvading;
    public bool isDead;

    public bool giveDamage;

    [Header("Particles Prefabs")]
    public GameObject droppedPrefab;
    public GameObject evadePrefab;
    public GameObject doubleJumpPrefab;
    public GameObject wallSlidePrefab;

    GameObject _cloneEvadePrefab;
    GameObject _cloneDroppedPrefab;
    GameObject _cloneDoubleJumpPrefab;
    GameObject _cloneWallSlidePrefab;

    PlayerInput input;
    BoxCollider2D bodyCollider;
    Rigidbody2D rigidBody;
    SpriteRenderer sprite;
    PlayerAttack attack;

    ParticleSystem droppedParticle;
    ParticleSystem evadeParticle;
    ParticleSystem jumpParticle;

    float _jumpTime;
    float _jumpForceX;
    float _jumpForceY;
    float _coyoteTime;
    float _playerHeight;
    float _wallSlideSpeed;
    float _grabingOffDelay;
    float _wallRunDur;
    float _wallJumpDelay;
    float _evadingDuration;
    float _evadingCooldown;

    bool _wallRunEnabled;

    internal int direction = 1;
    int _extraJumpsCount;
    int _wallJumpDirection;
    int _wallJumpPrevDirection;     //Check to disable multi double jumps at same wall

    RaycastHit2D droppedCheck;

    Vector2 _colliderStandSize;
    Vector2 _colliderStandOffset;
    Vector2 _colliderCrouchSize;
    Vector2 _colliderCrouchOffset;

    const float smallAmount = .05f;

    void Start()
    {
        input = GetComponent<PlayerInput>();
        rigidBody = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();

        attack = GetComponentInChildren<PlayerAttack>();

        _playerHeight = bodyCollider.size.y;

        _colliderStandSize = bodyCollider.size;
        _colliderStandOffset = bodyCollider.offset;

        _colliderCrouchSize = new Vector2(bodyCollider.size.x, .85f);
        _colliderCrouchOffset = new Vector2(bodyCollider.offset.x, -.17f);
    }

    private void OnGUI()
    {
        GUI.TextField(new Rect(10, 10, 100, 200), rigidBody.velocity.ToString() + "\n" + transform.position + "\nSwitch" + PlayerInput.switchWorld);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            Time.timeScale = 0.1f;
        if (Input.GetMouseButtonDown(2))
            Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.T))
            Time.timeScale = 0.01f;

        if (!isGrabing)
        {
            isWallRun = false;
            if(!isEvading)
                input.horizontalAccess = true;
        }

        if (isOnGround)
        {
            if (isDropping)
            {
                Instantiate(droppedPrefab, transform.position + droppedPrefab.transform.position, droppedPrefab.transform.rotation);
                //droppedParticle.Play();
                AudioManager.PlayDroppedCrashAudio();
                isDropping = false;
            }

            _coyoteTime = Time.time + coyoteDuration;
            _wallRunEnabled = true;
            _wallJumpDirection = 0;
            _wallJumpPrevDirection = 0;
            _extraJumpsCount = extraJumps;
        }
    }

    void FixedUpdate()
    {
        if (!isDead)
        {
            //Check the environment to determine status
            PhysicsCheck();

            //Process ground and air movements
            GroundMovement();
            MidAirMovement();
        }
        else
            rigidBody.velocity = Vector2.zero;
    }

    void PhysicsCheck()
    {
        //Start by assuming the player isn't on the ground and the head isn't blocked
        isOnGround = false;
        isHeadBlocked = false;

        //Cast ray for check to enabled dropped
        droppedCheck = Raycast(new Vector2(0f, -_playerHeight / 2f), Vector2.down, 2f);

        //Cast rays for the left and right foot
        RaycastHit2D leftCheck = Raycast(new Vector2(-footOffset, -_playerHeight / 2f), Vector2.down, groundDistance);
        RaycastHit2D rightCheck = Raycast(new Vector2(footOffset, -_playerHeight / 2f), Vector2.down, groundDistance);

        if ((leftCheck || rightCheck) && !isJumping && !isWallRun)
            isOnGround = true;

        //Check free space above the character (for crouch)
        RaycastHit2D headCheck = Raycast(Vector2.zero, Vector2.up, headClearence);

        if (headCheck)
            isHeadBlocked = true;

        Vector2 grabDir = new Vector2(direction, 0f);

        // Cast a ray to look for a wall grab
        RaycastHit2D wallCheckTop = Raycast(new Vector2(footOffset * direction, _playerHeight / 2f), grabDir, grabDistance);
        RaycastHit2D wallCheckDown = Raycast(new Vector2(footOffset * direction, -_playerHeight / 2f), grabDir, grabDistance);

        //Grabbing
        if (!isOnGround && !isGrabing && wallCheckTop && wallCheckDown && input.horizontal != 0 && direction != _wallJumpDirection && _wallJumpDelay <= Time.time)
        {
            isJumping = false;
            isDoubleJump = false;
            Vector3 pos = transform.position;
            //if (wallCheckTop.distance != 0)
            //    pos.x += (wallCheckTop.distance - smallAmount) * direction;
            //else
            pos.x += (wallCheckDown.distance - smallAmount) * direction;    //The code above can be used if character will be grabbing wall with incorrect distance

            _wallJumpDirection = direction;                                 //For impossible grabbing same wall

            if (_wallJumpPrevDirection != _wallJumpDirection)
            {
                _extraJumpsCount = extraJumps;                              //Extra jumps refresh
                _wallJumpPrevDirection = _wallJumpDirection;
            }
            else
                _wallJumpPrevDirection = 0;

            transform.position = pos;
            rigidBody.velocity = Vector2.zero;

            isGrabing = true;
        }
        else if ((!wallCheckTop && !wallCheckDown) || (input.horizontalAccess && Mathf.Sign(input.horizontal) != _wallJumpDirection && input.horizontal != 0))
            isGrabing = false;
        
        //Evading
        if (input.evade && !attack.endOfAttack && !isEvading && _evadingCooldown <= Time.time)
        {
            if (isGrabing)
                FlipCharacterDirection();

            isAttacking = false;
            isDropping = false;
            isGrabing = false;
            isEvading = true;
            _evadingDuration = Time.time + evadingDuration;
            input.horizontalAccess = false; 
            Crouch();

            sprite.enabled = false;
            //evadeParticle.Play();
            _cloneEvadePrefab = Instantiate(evadePrefab, transform);
            AudioManager.PlayEvadeAudio();

            rigidBody.velocity = Vector2.zero;

            if(input.horizontal != 0)
                rigidBody.AddForce(new Vector2(evadingDistance * Mathf.Sign(input.horizontal), 0f), ForceMode2D.Impulse); //input horizontal instead direction for evade after attack
            else
                rigidBody.AddForce(new Vector2(evadingDistance * direction, 0f), ForceMode2D.Impulse);

        }
        else if(isEvading && (_evadingDuration <= Time.time || isGrabing || isClimbing || isDropping || (input.jumpPressed && _extraJumpsCount > 0)))
        {
            isEvading = false;
            input.horizontalAccess = true;
            sprite.enabled = true;
            //evadeParticle.Stop();
            _cloneEvadePrefab.GetComponent<ParticleSystem>().Stop();

            _evadingCooldown = Time.time + evadingCooldown;
        }

        //Climbing
        if (!wallCheckTop && wallCheckDown && input.horizontal != 0)
        {
            int numColliders = 10;
            Collider2D[] colliders = new Collider2D[numColliders];
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(1 << 9); //9 - platform layer
            int colliderCount = bodyCollider.OverlapCollider(contactFilter, colliders);

            if (colliderCount > 0)
            {
                Vector3 pos = transform.position;
                isJumping = false;
                isDoubleJump = false;
                isClimbing = true;
                rigidBody.velocity = Vector2.zero;
            }
        }
        if (isClimbing)
        {
            rigidBody.AddForce(Vector2.up * climbingSpeed, ForceMode2D.Impulse);

            if (!wallCheckDown)
            {
                rigidBody.velocity = Vector2.zero;
                isClimbing = false;
            }
        }
    }

    void GroundMovement()
    {
        if (isGrabing)
            return;
            
        if (input.crouchHeld && !isCrouching && isOnGround && !isAttacking)
            Crouch();
        else if (!input.crouchHeld && isCrouching && !isEvading)
            StandUp();
        else if (!isOnGround && isCrouching)
            StandUp();

        float xVelocity;

        if (input.horizontalAccess && !isCrouching && !isAttacking && !isDropping)
        {
            xVelocity = speed * input.horizontal;
            //Флипать чара, если он смотрит в протиповоложную от движения сторону
            if (xVelocity * direction < 0f)
                FlipCharacterDirection();
        }
        else
        {
            xVelocity = 0f;
            if (direction != Mathf.Sign(input.horizontal) && input.horizontal != 0 && (isCrouching || isDropping))
                FlipCharacterDirection();
        }

        //Уменьшение мувспида при приседании
        if (isCrouching)
            xVelocity /= crouchSpeedDivisor;

        if(!isEvading)
            rigidBody.velocity = new Vector2(xVelocity, rigidBody.velocity.y);
    }
    
    void MidAirMovement()
    {
        if (input.jumpPressed && !isEvading && !isJumping && !isAttacking && !isDropping && (isOnGround || isGrabing || _coyoteTime > Time.time))
        {
            if (isCrouching && !isHeadBlocked)
                StandUp();

            if (isGrabing)
            {
                _jumpForceX = 25f * -direction;

                FlipCharacterDirection();
            }
            else
                _jumpForceX = 0f;

            isOnGround = false;
            isGrabing = false;
            isJumping = true;
            _jumpTime = Time.time + jumpHoldDuration;

            //Ставит допустимую задержку для бега по стене после прыжка
            if (_wallRunEnabled)
            {
                _wallRunDur = Time.time + wallRunDelay;
                _wallRunEnabled = false;
            }

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            rigidBody.AddForce(new Vector2(_jumpForceX, jumpForce), ForceMode2D.Impulse);


            //AudioManager.PlayJumpAuduio();
        }
        //Higher jump
        else if (isJumping)
        {
            if (input.jumpHeld)
                rigidBody.AddForce(new Vector2(0f, jumpHoldForce), ForceMode2D.Impulse);

            if (_jumpTime <= Time.time)
            {
                isJumping = false;
                isDoubleJump = false;
            }
        }
        //Double jumps
        else if (!isOnGround && !isGrabing && !isDropping && input.jumpPressed && _extraJumpsCount > 0 && !input.crouchHeld)
        {
            isJumping = true;
            isDoubleJump = true;
            _jumpTime = Time.time + jumpHoldDuration;
            //jumpParticle.Play();
            Instantiate(doubleJumpPrefab, transform.position + doubleJumpPrefab.transform.position, doubleJumpPrefab.transform.rotation);
            AudioManager.PlayDoubleJumpAudio();

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            rigidBody.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

            //Delay for impossible grabbing to same wall
            _wallJumpDelay = Time.time + wallJumpDelay;
            _wallJumpDirection = 0;
            _extraJumpsCount--;
        }
        else if (isGrabing)
        {
            //Grabbing off delay
            if (Mathf.Sign(input.horizontal) == _wallJumpDirection || input.horizontal == 0)
                _grabingOffDelay = Time.time + grabingOffDuration;

            if (_grabingOffDelay > Time.time)
                input.horizontalAccess = false;
            else
                input.horizontalAccess = true;

            //Wall run enabled
            if (_wallRunDur > Time.time)
            {
                if (!isWallRun)
                {
                    _wallRunDur = Time.time + wallRunDuration;
                    isWallRun = true;
                }

                rigidBody.velocity = new Vector2(0f, wallRunSpeed);
            }
            //Wall run disabled or his time is over
            else
            {
                isWallRun = false;

                //Фиксирование максимальной скорости скольжения по стене
                if (rigidBody.velocity.y < -_wallSlideSpeed)
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, -_wallSlideSpeed);

                if (input.crouchHeld)
                {
                    AudioManager.PlaySlideWallAudio();
                    _wallSlideSpeed = wallSlideMaxSpeed * 7f;

                    if (_cloneWallSlidePrefab == null)
                        _cloneWallSlidePrefab = Instantiate(wallSlidePrefab, transform);
                }
                else
                {
                    AudioManager.particlesSource.Stop();        //Need to change this way to stop audio on the something other
                    //_cloneWallSlidePrefab.GetComponent<ParticleSystem>().Stop();
                    _wallSlideSpeed = wallSlideMaxSpeed;
                }

                if (isOnGround && rigidBody.velocity.y <= 0)
                {
                    isGrabing = false;
                    FlipCharacterDirection();
                }
            }
        }
        //Dropping
        else if (input.crouchHeld && input.jumpPressed && !isDropping && !droppedCheck)
        {
            isDropping = true;
            rigidBody.AddForce(new Vector2(0f, maxFallSpeed), ForceMode2D.Impulse);
        }

        //Фиксирование максимальной скорости падения
        if (rigidBody.velocity.y < maxFallSpeed)
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxFallSpeed);
    }

    internal void FlipCharacterDirection()
    {
        direction *= -1;
        sprite.flipX = !sprite.flipX;
    }

    internal void Crouch()
    {
        isCrouching = true;

        bodyCollider.size = _colliderCrouchSize;
        bodyCollider.offset = _colliderCrouchOffset;
    }

    internal void StandUp()
    {
        if (isHeadBlocked)
            return;

        isCrouching = false;
        bodyCollider.size = _colliderStandSize;
        bodyCollider.offset = _colliderStandOffset;
    }

    void GiveDamage()
    {
        if (isEvading)
            return;

        giveDamage = true;
    }

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
    {
        //Если луч без указания слоя
        return Raycast(offset, rayDirection, length, groundLayer);
    }
    
    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
    {
        Vector2 pos = transform.position;
        

        //Рассчёт луча от чара+отступ до ближайшего объекта на указанном слое (если не указан см. выше)
        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, mask);

        if(drawDebugRaycast)
        {
            Color color = hit ? Color.red : Color.cyan;

            Debug.DrawRay(pos + offset, rayDirection * length, color);
        }

        //Вернуть луч
        return hit;
    }
}

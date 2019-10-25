using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Properties")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float startDazedTime = .6f;

    [Header("Ranges for actions")]
    public float chasingRange = 5f;
    public float attackDistance = 1f;
    public float distanceView = 7f;
    public float angleOfView = 40f;

    [Header("Timings")]
    public float searchDuration = 3f;
    public float searchDelay = 1f;
    public float timeBtwAttack = 2f;
    public float attackDelay = .1f;

    [Header("Atributes")]
    public int health = 2;
    public int damage = 1;
    public enum attackType { melee, range, shadow, shadowDistance, light, lightDistance };
    public enum movementType { grounding, flying };

    [Header("Status Flags")]
    public bool isPatrolling = true;
    public bool isChasing;
    public bool isSearching;
    public bool isAttacking;
    public bool isDead;
    public bool isHurt;

    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public bool _shadowTag;

    internal float speed;
    float _dazedTime;
    float _spriteHeight;
    float _offsetX;
    float _searchDuration;
    float _searchDelay;
    float _timeBtwAttack;
    float _distanceToPlayer;

    int direction = 1;
    int viewCount;

    EnemyAttack enemyAttack;
    PlayerAtributes playerAtributes;
    SpriteRenderer sprite;
    Animator anim;
    Rigidbody2D rigidBody;
    BoxCollider2D bodyCollider;
    BoxCollider2D playerCollider;
    Transform player;
    PolygonCollider2D fieldOfView;
    public ContactFilter2D viewFilter;

    Vector2 playerDir;

    RaycastHit2D playerCheck;
    RaycastHit2D wallCheck;

    void Start()
    {
        bodyCollider = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        enemyAttack = GetComponentInChildren<EnemyAttack>();
        
        player = GameObject.Find("Player").GetComponent<Transform>();
        playerAtributes = GameObject.Find("Player").GetComponent<PlayerAtributes>();
        playerCollider = GameObject.Find("Player").GetComponent<BoxCollider2D>();

        fieldOfView = GetComponentInChildren<PolygonCollider2D>();

        _spriteHeight = bodyCollider.size.y;
        _offsetX = bodyCollider.size.x / 2f;

        if (tag == "Shadow Enemy")
            _shadowTag = true;
        else
            _shadowTag = false;
    }

    void Update()
    {/*
        if (_dazedTime <= 0)
            speed = 5f;
        else
        {
            speed = 0f;
            _dazedTime -= Time.deltaTime;
        }*/

        //Distance of view refresh
        if (fieldOfView.points[0].x != distanceView || fieldOfView.points[3].x != distanceView)
        {
            fieldOfView.pathCount = 1;
            fieldOfView.SetPath( 0, new Vector2[] { new Vector2(distanceView, 2f), new Vector2(0f, .1f), new Vector2(0f, -.1f), new Vector2(distanceView, -1f) } );
        }

        if (health <= 0)
            isDead = true;
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            isAttacking = false;
            rigidBody.velocity = Vector2.zero;
            //Physics2D.IgnoreCollision(playerCollider, bodyCollider);
        }
        else
        {
            Movement();
            Chase();
            Search();
            Patrol();
        }
    }

    void Movement()
    {
        RaycastHit2D hit;
        LayerMask mask = groundLayer | playerLayer;

        //Raycast to player from enemy
        if (hit = Physics2D.Raycast(transform.position, player.position - transform.position, distanceView, mask))
        {
            //If enemy see the player
            if (hit.transform == player)
            {
                //Check the area for the presence of the player in sight
                Collider2D[] collCount = new Collider2D[1];
                viewFilter = new ContactFilter2D();
                viewFilter.SetLayerMask(1 << 10); //10 - player layer
                viewCount = fieldOfView.OverlapCollider(viewFilter, collCount);
            }
            else
                viewCount = 0;
        }
        //Check collision with the wall for switch direction of move
        wallCheck = Raycast(new Vector2(_offsetX * direction, 0f), Vector2.right * direction, .5f, groundLayer);
        _distanceToPlayer = Vector2.Distance(transform.position, player.position);
        playerDir = player.position - transform.position;

        //If enemy from shadow world and character in the shadow world or vice versa
        if (_shadowTag == GameManager.inTheShadowWorld && playerAtributes.health > 0)
        {
            if (viewCount > 0) //If the player in the field of view
            {
                if (_distanceToPlayer > attackDistance)
                    StartChase();
                else
                    StartSearch();
            }
            else if (isChasing)
                StartSearch();
        }
        else if ( isChasing || isSearching )
            StartSearch();
        else if ( !isAttacking )
            StartPatrol();

        if (isPatrolling || isChasing)
            rigidBody.velocity = new Vector2(speed * direction, rigidBody.velocity.y);
        else
            rigidBody.velocity = Vector2.zero;
    }

    void StartPatrol()
    {
        if (isPatrolling)
            return;

        isPatrolling = true;
        isChasing = false;
        isSearching = false;
        fieldOfView.transform.rotation = new Quaternion();
    }

    void Patrol()
    {
        if (!isPatrolling)
            return;

        if (wallCheck)
        {
            rigidBody.velocity = Vector2.zero;
            FlipCharacterDirection();
        }

        speed = patrolSpeed;
    }

    void StartChase()
    {
        if (isAttacking)
            return;

        isChasing = true;
        isPatrolling = false;
        isSearching = false;
    }

    void Chase()
    {
        if (!isChasing)
            return;
        
        if (Mathf.Abs(2f * Mathf.Rad2Deg * Mathf.Abs(fieldOfView.transform.rotation.z)) < angleOfView)
        {
            float angle = Vector2.Angle(Vector2.right * direction, player.position - transform.position);
            angle = Mathf.Clamp(angle, 0, angleOfView);
            fieldOfView.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, angle * direction);
        }

        speed = chaseSpeed;
        direction = (int)Mathf.Sign(playerDir.x);
    }

    void StartSearch()
    {
        if (isAttacking || isSearching)
            return;

        isChasing = false;
        isSearching = true;
        rigidBody.velocity = Vector2.zero;
        _searchDuration = Time.time + searchDuration;
        _searchDelay = Time.time + searchDelay;

        if (playerAtributes.health > 0)
        {
            if (viewCount > 0)
                StartCoroutine(AttackTimings(attackDelay)); //Delay before attack
        }
        else
            StartPatrol();
    }

    void Search()
    {
        if (isAttacking || !isSearching)
            return;

        if (Mathf.Abs(2f * Mathf.Rad2Deg * Mathf.Abs(fieldOfView.transform.rotation.z)) < angleOfView && _searchDelay <= Time.time + searchDelay / 2f)
            fieldOfView.transform.Rotate(0f, 0f, direction * 2f); //look up

        if (_searchDelay <= Time.time && _searchDuration > Time.time)
        {
            FlipCharacterDirection();
            _searchDelay = Time.time + searchDelay;
        }
        else if (_searchDuration <= Time.time)
            StartPatrol();
    }

    void Attack()
    {
        enemyAttack.MeleeAttack(damage);
        StartCoroutine(AttackTimings(timeBtwAttack));
    }

    IEnumerator AttackTimings(float delay)
    {
        if (!isAttacking)
        {
            if (_timeBtwAttack > Time.time)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(delay);
                isSearching = false;
                isChasing = false;
                isPatrolling = false;
                isAttacking = true;
            }
        }
        else
        {
            _timeBtwAttack = Time.time + delay;
            yield return new WaitForSeconds(delay / 4f);

            isAttacking = false;
            fieldOfView.transform.rotation = new Quaternion();

            if (playerAtributes.health > 0)
            {
                if (viewCount > 0)
                {
                    if (_distanceToPlayer > attackDistance)
                        StartChase();
                    else
                        StartSearch();
                }
                else
                    StartSearch();
            }
            else
                StartPatrol();
        }
    }
    
    void FlipCharacterDirection()
    {
        direction = -direction;
        sprite.flipX = !sprite.flipX;

        fieldOfView.transform.localScale = new Vector3(direction, fieldOfView.transform.localScale.y, fieldOfView.transform.localScale.z);
        fieldOfView.transform.rotation = new Quaternion();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        //_dazedTime = startDazedTime;
        AudioManager.PlaySwordImpactAudio();
        health -= damage;
    }

    void Dead()
    {
        Destroy(gameObject);
    }

    //Raycast
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

        Color color = hit ? Color.red : Color.cyan;

        Debug.DrawRay(pos + offset, rayDirection * length, color);

        return hit;
    }
}

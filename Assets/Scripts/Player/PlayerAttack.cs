using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float startTimeBtwAttack = .4f;
    public float attackRangeX = 1f;
    public float attackRangeY = 1.2f;
    public float attackDuration = .6f;
    public float delayResetCombo = 1f;
    public float attackForceDistance = 2000f;

    public int damage;
    public int comboCount = 3;      //count of combo
    public int combo;           //current number of combo
    public int airComboCount = 3;
    public int airCombo;

    [HideInInspector]
    public bool endOfAttack;

    public LayerMask[] whatIsEnemies;
    public Transform attackPos;

    Animator _anim;
    CameraShake _camShake;
    SpriteRenderer _sprite;
    PlayerMovement _movement;
    PlayerInput _input;
    Transform _parent;
    Rigidbody2D _rigidBody;

    float _timeBtwAttack;
    float _attackDuration;
    float _delayResetCombo;
    float _posX;

    bool _canFlip = true;

    void Start()
    {
        _posX = transform.localPosition.x;
        _parent = transform.parent;
        _anim = _parent.GetComponent<Animator>();
        _movement = _parent.GetComponent<PlayerMovement>();
        _rigidBody = _parent.GetComponent<Rigidbody2D>();
        _sprite = _parent.GetComponent<SpriteRenderer>();
        _input = _parent.GetComponent<PlayerInput>();
        _camShake = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>().GetComponent<CameraShake>(); //Получить компонент из камеры. Скорее всего позже нужно будет как-то указать нужную камеру.
    }
    
    void Update()
    {
        //Flip children object "Attack"
        if (_sprite.flipX)
            transform.localPosition = new Vector2(-_posX, transform.localPosition.y);
        else
            transform.localPosition = new Vector2(_posX, transform.localPosition.y);

        //Flip character only before he's give damage
        if (_movement.isAttacking && _canFlip && _movement.direction != Mathf.Sign(_input.horizontal) && _input.horizontal != 0)
            _movement.FlipCharacterDirection();
    }

    private void FixedUpdate()
    {
        if (!_movement.isDead)
        {
            EndOfAttack();
            Attack();
            GiveDamage();
        }
    }

    void Attack()
    {
        if (_input.attack && _timeBtwAttack + Time.fixedDeltaTime <= Time.time && !_movement.isEvading)
        {
            if (_movement.isCrouching)
                _movement.StandUp();

            _movement.isAttacking = true;
            _input.attack = false;
            _timeBtwAttack = Time.time + startTimeBtwAttack;
            _attackDuration = Time.time + attackDuration;

            if (_movement.isOnGround)
            {
                airCombo = 0;

                if (combo < comboCount)
                    combo++;
                else
                    combo = 1;
            }
            else
            {
                combo = 0;

                if (airCombo < comboCount)
                    airCombo++;
                else
                    airCombo = 1;
            }
        }

        if (!_canFlip)
            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, 0f);

        //End of attack (after this code next attack available) Must to be here, after attack code
        if (_attackDuration <= Time.time)
        {
            //_input.attack = false;
            _movement.isAttacking = false;
        }
    }

    void EndOfAttack()
    {
        //End of attack anim
        if (_timeBtwAttack <= Time.time)
        {
            _canFlip = true;
            endOfAttack = false;

            //Switch animation from air attack to grounded
            if (airCombo > 0 && _movement.isOnGround)
                _movement.isAttacking = false;

            if (_delayResetCombo <= Time.time)
            {
                combo = 0;
                airCombo = 0;
            }
        }
    }

    void GiveDamage()
    {
        if (!_movement.giveDamage)
            return;

        AudioManager.PlaySwordAttackAudio();
        endOfAttack = true;
        _canFlip = false;

        _rigidBody.AddForce(Vector2.right * _movement.direction * attackForceDistance);

        Collider2D[] enemiesToDamage = Physics2D.OverlapBoxAll(attackPos.position, new Vector2(attackRangeX, attackRangeY), 0, whatIsEnemies[0] | whatIsEnemies[1]);
        LayerMask mask = 1 << 9 | 1 << 12;
        RaycastHit2D hit;

        //Raycast to player from enemy
        hit = Physics2D.Raycast(_movement.transform.position, Vector2.right * _movement.direction, 1.2f, mask);
        Debug.Log(hit + "\n" + hit.transform);
        Debug.DrawRay(_movement.transform.position, Vector2.right * _movement.direction * 1.2f, Color.cyan, 2f);

        for (int i = 0; i < enemiesToDamage.Length; i++)
        {
            _camShake.Shake(.2f, 1f, 2f);
            enemiesToDamage[i].GetComponent<Enemy>().TakeDamage(damage);
        }

        _delayResetCombo = Time.time + delayResetCombo;

        _movement.giveDamage = false;
    }
   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPos.position, new Vector3(attackRangeX, attackRangeY, 0f));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public LayerMask whatIsPlayer;
    public Transform attackPos;

    public float attackRangeX;
    public float attackRangeY;

    float _posX;

    Transform _parent;
    CameraShake _camShake;
    SpriteRenderer _sprite;

    void Start()
    {
        _parent = transform.parent;
        _posX = transform.localPosition.x;
        _camShake = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>().GetComponent<CameraShake>();
        _sprite = _parent.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //Flip enemy children object "Attack"
        if (_sprite.flipX)
            transform.localPosition = new Vector2(-_posX, transform.localPosition.y);
        else
            transform.localPosition = new Vector2(_posX, transform.localPosition.y);
    }

    public void MeleeAttack(int damage)
    {
        AudioManager.PlaySwordAttackAudio();
        Collider2D[] hittedObjects = Physics2D.OverlapBoxAll(attackPos.position, new Vector2(attackRangeX, attackRangeY), 0, whatIsPlayer);
        for (int i = 0; i < hittedObjects.Length; i++)
        {
            hittedObjects[i].GetComponent<PlayerAtributes>().TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPos.position, new Vector3(attackRangeX, attackRangeY, 0f));
    }
}

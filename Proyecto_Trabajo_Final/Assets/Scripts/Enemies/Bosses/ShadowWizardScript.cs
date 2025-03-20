using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowWizardScript : EnemyScript
{
    private CapsuleCollider2D m_Collider;
    private Rigidbody2D m_Rigidbody2D;
    public Transform m_InitialSummonPoint;
    private Animator m_Animator;
    private SpriteRenderer m_SpriteRenderer;
    public GameObject m_ProyectilePrefab;
    public GameObject m_ProyectilePrefab2;
    

    public enum WIZARD_BEHAVIOUR
    {
        IDLE,
        BATTLE
    }
    public WIZARD_BEHAVIOUR m_WizardBehaviour = WIZARD_BEHAVIOUR.IDLE;

    // Variables
    public float m_StunnedTime = 0.5f;
    private bool m_CanMove;
    public int m_TimesUntilKnockback = 2;
    public int m_TimesItGotHit = 0;
    public float m_JumpForce;
    public float m_KnockbackForce;
    private bool m_TriggerPhase2 = false; 

    public Transform m_GroundCheck;
    public LayerMask m_GroundLayer;
    private bool m_IsGrounded;

    // Start is called before the first frame update
    void Start()
    {
        m_CurrentLifePoints = m_MaxLifePoints;
        m_GoingRight = false;
        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_Collider = GetComponent<CapsuleCollider2D>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        m_CanMove = true;
        m_TriggerPhase2 = false;
        m_ActivateHealthBar = false;
        m_healthBar.UpdateHealthBar(m_MaxLifePoints, m_CurrentLifePoints);
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        if (m_Player.GetComponent<PlayerController>().m_ActivateBossFight)
        {
            m_WizardBehaviour = WIZARD_BEHAVIOUR.BATTLE;
            m_Player.GetComponent<PlayerController>().m_ActivateBossFight = false;
            m_ActivateHealthBar = true;
        }

        if (m_CurrentLifePoints <= 0)
        {
            if (m_CanMove)
            {
                DestroyBoss();
                m_Animator.SetTrigger("Die");
            }
            m_CanMove = false;
            m_Collider.enabled = false;
            m_Rigidbody2D.bodyType = RigidbodyType2D.Static;
        }

        m_IsGrounded = Physics2D.OverlapBox(
            m_GroundCheck.position, 
            new Vector2(2, 0.35f), 
            0, 
            m_GroundLayer); // The same as the player's ground check

        if (m_IsGrounded)
        {
            m_Animator.SetBool("Jumping", false);
        }
    }

    private void CheckIfFlipNeeded()
    {
        if (m_Player.transform.position.x < transform.position.x && m_GoingRight)
        {
            GoingRight = false;
            if (m_healthBar != null) m_healthBar.transform.localScale *= new Vector2(-1, 1);
        }
        else if (m_Player.transform.position.x > transform.position.x && !m_GoingRight)
        {
            GoingRight = true;
            if (m_healthBar != null) m_healthBar.transform.localScale *= new Vector2(-1, 1);
        }
    }

    public override void GetDamage(int howMuchDamage)
    {
        base.GetDamage(howMuchDamage);
        m_CanMove = false;
        CheckIfFlipNeeded();

        m_Animator.SetTrigger("Damaged");
        m_healthBar.UpdateHealthBar(m_MaxLifePoints, m_CurrentLifePoints);
        Attack();
        StartCoroutine(StunnedAfterHit());

        m_TimesItGotHit++;
        if (m_TimesItGotHit >= m_TimesUntilKnockback)
        {
            GetKnockback();
            m_TimesItGotHit = 0;
        }

        if (m_CurrentLifePoints <= m_MaxLifePoints * 0.625f && !m_TriggerPhase2)
        {
            m_TimesUntilKnockback = 1;
            m_TriggerPhase2 = true;
            InvokeRepeating("Jump", 0, 12);
            InvokeRepeating("AttackWithProyectile", 5, 7);
            InvokeRepeating("GetKnockback", 2, 3);
        }
    }

    private void GetKnockback()
    {
        CheckIfFlipNeeded();
        if (m_Rigidbody2D.gravityScale > 0)
        {
            if (m_Player.transform.position.x < transform.position.x)
            {
                m_Rigidbody2D.AddForce((Vector2.up + Vector2.left) * m_KnockbackForce);
            }
            else
            {
                m_Rigidbody2D.AddForce((Vector2.up + Vector2.right) * m_KnockbackForce);
            }
        }
        else
        {
            if (m_Player.transform.position.x < transform.position.x)
            {
                m_Rigidbody2D.AddForce((Vector2.down + Vector2.left) * m_KnockbackForce);
            }
            else
            {
                m_Rigidbody2D.AddForce((Vector2.down + Vector2.right) * m_KnockbackForce);
            }
        }
    }

    private void Attack()
    {
        m_Animator.SetTrigger("Attack2");
        m_CanMove = false;
        StartCoroutine(EnableMovement());
    }
    private void AttackWithProyectile()
    {
        m_Animator.SetTrigger("Attack1");
    }
    public void SummonProyectile()
    {
        if (m_CurrentLifePoints <= m_MaxLifePoints * 0.625f)
        {
            Instantiate(m_ProyectilePrefab2, m_InitialSummonPoint.position, Quaternion.identity);
        }
        else
        {
            Instantiate(m_ProyectilePrefab, m_InitialSummonPoint.position, Quaternion.identity);
        }
    }

    private void Jump()
    {
        m_Animator.SetBool("Jumping", true);
        m_Rigidbody2D.AddForce(Vector2.up * m_Rigidbody2D.gravityScale * m_JumpForce);
        Invoke("InvertGravity", 0.3f);
    }
    private void InvertGravity()
    {
        m_Rigidbody2D.gravityScale *= -1;
        transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y * -1);
    }

    public void DestroyBoss()
    {
        Invoke("EnableNextLevel", 1.8f);
        Destroy(gameObject, 2);
    }

    private IEnumerator EnableMovement()
    {
        yield return new WaitForSeconds(0.8f);
        m_CanMove = true;
    }

    private IEnumerator StunnedAfterHit()
    {
        yield return new WaitForSeconds(m_StunnedTime);
        m_CanMove = true;
        m_SpriteRenderer.color = Color.white;
        m_Animator.ResetTrigger("Damaged");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Attack();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AttackWithProyectile();
        }
    }

    public void EnableNextLevel()
    {
        GameObject.FindGameObjectWithTag("EndLevel").GetComponent<BoxCollider2D>().enabled = true;
    }
}

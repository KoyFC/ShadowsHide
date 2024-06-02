using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalWizardScript : EnemyScript
{
    private CapsuleCollider2D m_Collider;
    private Rigidbody2D m_Rigidbody2D;
    public Transform m_InitialSummonPoint;
    public Transform[] m_TeleportPoints;
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
    public int m_TimesUntilTeleport = 4;
    public int m_TimesItGotHit = 0;
    private int m_CurrentTeleportPointIndex = 0;
    public float m_JumpForce;
    public float m_KnockbackForce;
    private bool m_TriggerPhase2 = false; 
    private int m_CurrentProyectileIndex;

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
            InvokeRepeating("AttackWithProyectile", 2, 4);
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
        CheckIfFlipNeeded();
    }

    private void CheckIfFlipNeeded()
    {
        if (m_Player.transform.position.x < transform.position.x && m_GoingRight)
        {
            GoingRight = false;
            m_healthBar.transform.localScale *= new Vector2(-1, 1);
        }
        else if (m_Player.transform.position.x > transform.position.x && !m_GoingRight)
        {
            GoingRight = true;
            m_healthBar.transform.localScale *= new Vector2(-1, 1);
        }
    }

    public override void GetDamage(int howMuchDamage)
    {
        base.GetDamage(howMuchDamage);
        m_CanMove = false;

        m_Animator.SetTrigger("Damaged");
        m_healthBar.UpdateHealthBar(m_MaxLifePoints, m_CurrentLifePoints);
        StartCoroutine(StunnedAfterHit());

        m_TimesItGotHit++;
        if (m_TimesItGotHit >= m_TimesUntilTeleport)
        {
            m_Animator.SetTrigger("Teleport");
            m_TimesItGotHit = 0;
        }

        if (m_CurrentLifePoints <= m_MaxLifePoints * 0.5f && !m_TriggerPhase2)
        {
            m_TimesUntilTeleport = 3;
            m_TriggerPhase2 = true;
            InvokeRepeating("RepeatTeleport", 0, 5);
            InvokeRepeating("AttackWithProyectile", 2, 2);
            InvokeRepeating("GetKnockback", 2, 3);
        }
    }

    public void Teleport()
    {
        transform.position = m_TeleportPoints[m_CurrentTeleportPointIndex].position;
        m_CurrentTeleportPointIndex++;
        if (m_CurrentTeleportPointIndex >= m_TeleportPoints.Length)
        {
            m_CurrentTeleportPointIndex = 0;
        }
    }

    private void RepeatTeleport()
    {
        m_Animator.SetTrigger("Teleport");
    }

    private void AttackWithProyectile()
    {
        m_Animator.SetTrigger("Attack");
    }
    public void SummonProyectile()
    {
        if (m_CurrentProyectileIndex == 0)
        {
            Instantiate(m_ProyectilePrefab, m_InitialSummonPoint.position, Quaternion.identity);
        }
        else
        {
            Instantiate(m_ProyectilePrefab2, m_InitialSummonPoint.position, Quaternion.identity);
        }
        m_CurrentProyectileIndex++;
        if (m_CurrentProyectileIndex >= 2)
        {
            m_CurrentProyectileIndex = 0;
        }
    }

    public void DestroyBoss()
    {
        Destroy(gameObject, 4);
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
            AttackWithProyectile();
        }
    }

    public void EnableNextLevel()
    {
        GameObject.FindGameObjectWithTag("EndLevel").GetComponent<BoxCollider2D>().enabled = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGhostScript : EnemyScript
{
    private CapsuleCollider2D m_Collider;
    private Rigidbody2D m_Rigidbody2D;
    public Transform[] m_PatrolPoints;
    private Animator m_Animator;
    private SpriteRenderer m_SpriteRenderer;
    public GameObject m_DeathParticlesPrefab;

    public enum BOSS_GHOST_BEHAVIOUR
    {
        IDLE,
        PATROL_POINT, 
        FOLLOW_PLAYER
    }
    public BOSS_GHOST_BEHAVIOUR m_BossGhostBehaviour = BOSS_GHOST_BEHAVIOUR.IDLE;

    // Variables
    public float m_BossGhostMaxSpeed = 2;
    public float m_BossGhostCurrentSpeed;
    public float m_StunnedTime = 0.5f;

    private int m_CurrentPatrolPointIndex = 0;
    private bool m_CanMove;

    // Start is called before the first frame update
    void Start()
    {
        m_CurrentLifePoints = m_MaxLifePoints;
        m_BossGhostCurrentSpeed = m_BossGhostMaxSpeed;
        m_GoingRight = false;
        m_healthBar.UpdateHealthBar(m_MaxLifePoints, m_CurrentLifePoints);
        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_Collider = GetComponentInChildren<CapsuleCollider2D>();
        m_Rigidbody2D = GetComponentInChildren<Rigidbody2D>();
        m_Animator = GetComponentInChildren<Animator>();
        m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        m_CanMove = true;
        m_ActivateHealthBar = false;
        m_ActivateMusic = false;

        if (m_BossGhostBehaviour == BOSS_GHOST_BEHAVIOUR.PATROL_POINT)
        {
            CheckIfFlipNeeded();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        if (m_Player.GetComponent<PlayerController>().m_ActivateBossFight)
        {
            InvokeRepeating("ScarePlayer", 3, 5);
            m_BossGhostBehaviour = BOSS_GHOST_BEHAVIOUR.PATROL_POINT;
            m_Player.GetComponent<PlayerController>().m_ActivateBossFight = false;
            m_ActivateHealthBar = true;
            m_ActivateMusic = true;
        }

        switch (m_BossGhostBehaviour)
        {
            case BOSS_GHOST_BEHAVIOUR.PATROL_POINT:
                PatrolPoints(dt);
                break;
            
            case BOSS_GHOST_BEHAVIOUR.FOLLOW_PLAYER:
                FollowPlayer(dt);
                break;
        }

        if (m_CurrentLifePoints <= 0)
        {
            if (m_CanMove)
            {
                Invoke("DestroyBoss", 2);
            }
            m_CanMove = false;
            m_Collider.enabled = false;
            m_Animator.SetTrigger("Dead");
        }
    }

    private void PatrolPoints(float dt)
    {
        MoveToPoint(dt);
    }

    private void MoveToPoint(float dt)
    {
        if (!m_CanMove) return;

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = m_PatrolPoints[m_CurrentPatrolPointIndex].position;

        transform.position = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            m_BossGhostCurrentSpeed * dt);
        float stopDistance = 0.1f;

        stopDistance = Mathf.Pow(stopDistance, 2);
        float xElement = Mathf.Pow(targetPosition.x - currentPosition.x, 2);
        float yElement = Mathf.Pow(targetPosition.y - currentPosition.y, 2);

        if (xElement + yElement < stopDistance)
        {
            SelectNextPoint();
        }

        CheckIfFlipNeeded();
    }

    void FollowPlayer(float dt)
    {
        if (!m_CanMove) return;

        Vector2 direction = m_Player.transform.position - transform.position;

        //Debug.Log(direction.x);
        transform.position = Vector2.MoveTowards(
            this.transform.position,
            m_Player.transform.position,
            (m_BossGhostCurrentSpeed * 0.4f) * dt);

        if (direction.x < 0)
            {
                if (GoingRight)
                {
                    GoingRight = !GoingRight;
                    if (m_healthBar != null) m_healthBar.transform.localScale *= new Vector2(-1, 1);
                }
            }
            else
            {
                if (!GoingRight)
                {
                    GoingRight = !GoingRight;
                    if (m_healthBar != null) m_healthBar.transform.localScale *= new Vector2(-1, 1);
                }
            }
    }


    private void CheckIfFlipNeeded()
    {
        Vector2 curentPoint = m_PatrolPoints[m_CurrentPatrolPointIndex].position;
        if (curentPoint.x < transform.position.x)
        {
            if (GoingRight)
            {
                GoingRight = !GoingRight;
                if (m_healthBar != null)
                {
                    m_healthBar.transform.localScale *= new Vector2(-1, 1);
                }
            }
        }
        else
        {
            if (!GoingRight)
            {
                GoingRight = !GoingRight;

                if (m_healthBar != null)
                {
                    m_healthBar.transform.localScale *= new Vector2(-1, 1);
                }
            }
        }
    }

    private void SelectNextPoint()
    {
        m_CurrentPatrolPointIndex++;
        if (m_CurrentPatrolPointIndex >= m_PatrolPoints.Length)
        {
            m_CurrentPatrolPointIndex = 0;
        }
    }

    public override void GetDamage(int howMuchDamage)
    {
        base.GetDamage(howMuchDamage);
        m_CanMove = false;
        m_Animator.SetTrigger("Damaged");
        m_healthBar.UpdateHealthBar(m_MaxLifePoints, m_CurrentLifePoints);
        StartCoroutine(StunnedAfterHit());
    }


    private void ScarePlayer()
    {
        if (m_BossGhostBehaviour == BOSS_GHOST_BEHAVIOUR.IDLE) return;

        m_Animator.SetTrigger("Scare");
        m_CanMove = false;

        // Change behaviour from point to follow and viceversa each time it scares the player
        if (m_BossGhostBehaviour == BOSS_GHOST_BEHAVIOUR.PATROL_POINT)
        {
            m_BossGhostBehaviour = BOSS_GHOST_BEHAVIOUR.FOLLOW_PLAYER;
        }
        else
        {
            m_BossGhostBehaviour = BOSS_GHOST_BEHAVIOUR.PATROL_POINT;
        }
        StartCoroutine(EnableMovement());
    }

    private IEnumerator EnableMovement()
    {
        yield return new WaitForSeconds(0.8f);
        m_CanMove = true;
    }

    public void DestroyBoss()
    {
        Invoke("EnableNextLevel", 1);
        Instantiate(m_DeathParticlesPrefab, new Vector3(transform.position.x - 0.85f, transform.position.y + 0.85f, transform.position.z) , Quaternion.Euler(0, 0, 90));  
    }

    private void EnableNextLevel()
    {
        GameObject.FindGameObjectWithTag("EndLevel").GetComponent<BoxCollider2D>().enabled = true;
        Destroy(gameObject, 2);
    }

    private IEnumerator StunnedAfterHit()
    {
        yield return new WaitForSeconds(m_StunnedTime);
        m_CanMove = true;
        m_SpriteRenderer.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Lantern"))
        {
            Debug.Log("Boss hit player");
            ScarePlayer();

            CancelInvoke("ScarePlayer");
            InvokeRepeating("ScarePlayer", 5, 8);

            m_BossGhostBehaviour = BOSS_GHOST_BEHAVIOUR.PATROL_POINT;
        }
    }
}

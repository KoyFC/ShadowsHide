using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonScript : EnemyScript
{
    private Animator m_Animator;
    private CapsuleCollider2D m_Collider;
    private BoxCollider2D m_BoxCollider;
    private Rigidbody2D m_Rigidbody;
    

    public enum SKELETON_BEHAVIOUR
    {
        IDLE, // equals 0
        PATROL_COLLISION, // equals 1
        PATROL_POINT, // equals 2
        FOLLOW_PLAYER, // Will try to follow the player until the player is out of range
    }

    public SKELETON_BEHAVIOUR m_SkeletonBehaviour = SKELETON_BEHAVIOUR.IDLE;

    public float m_SkeletonSpeed = 1;
    private bool m_CanMove = true;
    private bool m_IsMoving = true;
    private float m_Distance;
    public float m_FollowDistance = 1;
    private bool m_ShouldFollow = false;
    public Transform[] m_PatrolPoints;
    private int m_CurrentPatrolPointIndex = 0;



    // Start is called before the first frame update
    void Start()
    {
        m_CurrentLifePoints = m_MaxLifePoints;
        m_SpawnPoint = transform.position;
        m_GoingRight = false;
        m_healthBar.UpdateHealthBar(m_MaxLifePoints, m_CurrentLifePoints);

        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_Animator = GetComponent<Animator>();
        m_Collider = GetComponent<CapsuleCollider2D>();
        m_BoxCollider = GetComponent<BoxCollider2D>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
        if (m_SkeletonBehaviour == SKELETON_BEHAVIOUR.PATROL_POINT)
        {
            CheckIfFlipNeeded();
        }
        m_ShouldFollow = false;

    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        if (m_IsMoving)
        {
            m_Animator.SetBool("IsWalking", true);
        }
        else
        {
            m_Animator.SetBool("IsWalking", false);
        }

        switch (m_SkeletonBehaviour)
        {
            case SKELETON_BEHAVIOUR.PATROL_COLLISION:
                PatrolCollision(dt);
                break;

            case SKELETON_BEHAVIOUR.PATROL_POINT:
                PatrolPoints(dt);
                break;
            
            case SKELETON_BEHAVIOUR.FOLLOW_PLAYER:
                FollowPlayer(dt);
                break;
        }

        if (m_CurrentLifePoints <= 0)
        {
            DestroySkeleton();
        }
    }

    private void PatrolCollision(float dt)
    {
        if (!m_CanMove) return;
        //---------x---------/-y-/
        Vector2 direction = new Vector2(GoingRight ? 1 : -1, 0);
        m_IsMoving = true;
        transform.Translate(direction * m_SkeletonSpeed * dt);
    }

    private void PatrolPoints(float dt)
    {
        m_IsMoving = true;
        MoveToPoint(dt);
    }

    private void MoveToPoint(float dt)
    {
        if (!m_CanMove) return;
        m_IsMoving = true;

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = m_PatrolPoints[m_CurrentPatrolPointIndex].position;

        targetPosition.y = transform.position.y;

        transform.position = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            m_SkeletonSpeed * dt);
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

        m_Distance = Vector2.Distance(transform.position, m_Player.transform.position);
        Vector2 direction = m_Player.transform.position - transform.position;

        // If the m_Distance is less than a public variable, then the skeleton moves towards the player, otherwise, it patrols the area.
        if (direction.x < m_FollowDistance && direction.x > -m_FollowDistance)
        {
            m_ShouldFollow = true;
        }
        else // This will check to see if the skeleton should follow the player once it has left the range
        {
            m_ShouldFollow = false;
        }

        if (m_ShouldFollow)    
        {
            //Debug.Log(direction.x);
            transform.position = Vector2.MoveTowards(
                this.transform.position,
                m_Player.transform.position,
                (m_SkeletonSpeed + 2) * dt);

            // Flip the skeleton if needed relative to the player
            
            if (direction.x < 0)
            {
                if (GoingRight)
                {
                    GoingRight = !GoingRight;
                }
            }

            else
            {
                if (!GoingRight)
                {
                    GoingRight = !GoingRight;
                }
            }
        }
        
        // If the skeleton should not follow the player, it will patrol the area.
        else if (m_SkeletonBehaviour == SKELETON_BEHAVIOUR.FOLLOW_PLAYER) 
        {
            PatrolCollision(dt);
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
            }
        }
        else
        {
            if (!GoingRight)
            {
                GoingRight = !GoingRight;
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
        m_Animator.SetTrigger("Damaged");
        m_healthBar.UpdateHealthBar(m_MaxLifePoints, m_CurrentLifePoints);
        // If helath is less than or equal to 0, destroy the skeleton
        if (m_CurrentLifePoints <= 0)
        {
            m_CanMove = false;
            DestroySkeleton();
        }

        else
        {
            if (m_SkeletonBehaviour == SKELETON_BEHAVIOUR.PATROL_COLLISION)
            {
                GoingRight = !GoingRight;
            }
        }
    }

    private void DestroySkeleton()
    {
        m_CanMove = false;
        m_Collider.enabled = false;
        m_BoxCollider.enabled = false;
        m_Animator.SetTrigger("Die");
        m_Rigidbody.bodyType = RigidbodyType2D.Static;
        Destroy(gameObject, 1);
    }

    public void ToggleCanMove()
    {
        m_CanMove = false;
        Invoke("CanMoveAgain", 0.4f);
    }
    private void CanMoveAgain()
    {
        m_CanMove = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_SkeletonBehaviour == SKELETON_BEHAVIOUR.PATROL_COLLISION || m_SkeletonBehaviour == SKELETON_BEHAVIOUR.FOLLOW_PLAYER)
        {
            GoingRight = !GoingRight;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeathBox"))
        {
            m_CurrentLifePoints = 0;
            if (m_healthBar != null) m_healthBar.UpdateHealthBar(m_MaxLifePoints, m_CurrentLifePoints);
        }
    }
}

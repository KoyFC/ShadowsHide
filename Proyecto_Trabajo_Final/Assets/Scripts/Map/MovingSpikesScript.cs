using System.Collections;
using UnityEngine;

public class MovingSpikesScript : MonoBehaviour
{
    [SerializeField] private Vector2 m_OriginalPosition = new Vector2(0, 0);
    private Vector2 m_TargetPosition = new Vector2(0, 0);

    void Start()
    {
        m_TargetPosition = transform.position;

        transform.position = m_OriginalPosition;

        StartCoroutine(MoveSpikes());
    }

    private IEnumerator MoveSpikes()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            yield return MoveToMiddlePosition();

            yield return new WaitForSeconds(0.75f);
            yield return MoveToPosition(m_TargetPosition, 0.1f); // Moving quickly to target

            yield return new WaitForSeconds(0.5f);
            yield return MoveToPosition(m_OriginalPosition, 0.5f); // Moving slowly to original            
        }
    }

    private IEnumerator MoveToMiddlePosition()
    {
        float duration = 1.0f;
        Vector2 startPosition = transform.position;
        Vector2 midPosition = (m_OriginalPosition + m_TargetPosition) / 2;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(startPosition, midPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = midPosition;
    }

    private IEnumerator MoveToPosition(Vector2 target, float duration)
    {
        Vector2 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(startPosition, target, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnableTutorialControls : MonoBehaviour
{
    private Collider2D m_Collider = null;
    private List<Renderer> m_Renderers = new List<Renderer>(); 

    [SerializeField] private GameObject[] m_ObjectsToEnable;
    [SerializeField] private float m_FadeDuration = 1.0f;

    void Awake()
    {
        m_Collider = GetComponent<Collider2D>();

        if (!m_Collider.isTrigger) m_Collider.isTrigger = true;

        m_Renderers = new List<Renderer>();
        foreach (GameObject targetObject in m_ObjectsToEnable)
        {
            Renderer renderer = targetObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                m_Renderers.Add(renderer);
                SetRendererOpacity(renderer, 0f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeRenderers(1f));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeRenderers(0f));
        }
    }

    private IEnumerator FadeRenderers(float targetOpacity)
    {
        float startOpacity = m_Renderers.Count > 0 ? m_Renderers[0].material.color.a : 0f; // Usar Count en lugar de Length
        float elapsedTime = 0f;

        while (elapsedTime < m_FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newOpacity = Mathf.MoveTowards(startOpacity, targetOpacity, elapsedTime / m_FadeDuration);
            
            foreach (Renderer renderer in m_Renderers)
            {
                SetRendererOpacity(renderer, newOpacity);
            }

            yield return null;
        }

        foreach (Renderer renderer in m_Renderers)
        {
            SetRendererOpacity(renderer, targetOpacity);
        }
    }

    private void SetRendererOpacity(Renderer renderer, float opacity)
    {
        Color color = renderer.material.color;
        color.a = opacity;
        renderer.material.color = color;
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MultiIconManager : MonoBehaviour
{
    private bool m_IsGamepad;

    [System.Serializable]
    private struct KeyboardIcons
    {
        [Header("Movement")]
        public Sprite MoveLeft;
        public Sprite MoveRight;
        public Sprite Jump;
        public Sprite Run;

        [Header("Lamp")]
        public Sprite Summon;
        public Sprite MoveLamp;
        public Sprite BasicAttack;
        public Sprite SpecialAbility;

        [Header("Colors")]
        public Sprite ToggleColor;
        public Sprite NextColor;
        public Sprite PreviousColor;     
    }

    [System.Serializable]
    private struct ControllerIcons
    {
        public Sprite BaseSprite;

        [Header("Movement")]
        public Sprite Move;
        public Sprite Jump;
        public Sprite Run;

        [Header("Lamp")]
        public Sprite Summon;
        public Sprite MoveLamp;
        public Sprite BasicAttack;
        public Sprite SpecialAbility;

        [Header("Colors")]
        public Sprite ToggleColor;
        public Sprite NextColor;
        public Sprite PreviousColor;  
    }

    [System.Serializable]
    private struct IconReferences
    {
        [Header("Movement")]
        public SpriteRenderer[] MoveLeftImages;
        public SpriteRenderer[] MoveRightImages;
        public SpriteRenderer[] MoveImages;
        public SpriteRenderer[] JumpImages;
        public SpriteRenderer[] RunImages;

        [Header("Lamp")]
        public SpriteRenderer[] SummonImages;
        public SpriteRenderer[] MoveLampImages;
        public SpriteRenderer[] BasicAttackImages;
        public SpriteRenderer[] SpecialAbilityImages;

        [Header("Colors")]
        public SpriteRenderer[] ToggleColorImages;
        public SpriteRenderer[] NextColorImages;
        public SpriteRenderer[] PreviousColorImages;
    }

    [Header("Icons")]
    [SerializeField] private KeyboardIcons m_KeyboardIcons;
    [SerializeField] private ControllerIcons m_ControllerIcons;

    [Header("Icon References")]
    [SerializeField] private IconReferences m_IconReferences;

    void Awake()
    {
        UpdateIconsBasedOnCurrentDevice();
    }

    void Start()
    {
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        OnDeviceChange(playerInput);
    }

    public void OnDeviceChange(PlayerInput playerInput)
    {
        m_IsGamepad = playerInput.currentControlScheme.Equals("Controller");

        UpdateIconsBasedOnCurrentDevice();
    }

    private void UpdateIconsBasedOnCurrentDevice()
    {
        if (!m_IsGamepad)
        {
            SetIcons(m_KeyboardIcons.MoveLeft, m_IconReferences.MoveLeftImages);
            SetIcons(m_KeyboardIcons.MoveRight, m_IconReferences.MoveRightImages);
            SetIcons(m_KeyboardIcons.Jump, m_IconReferences.JumpImages);
            SetIcons(m_KeyboardIcons.Run, m_IconReferences.RunImages);
            
            SetIcons(m_KeyboardIcons.Summon, m_IconReferences.SummonImages);
            SetIcons(m_KeyboardIcons.MoveLamp, m_IconReferences.MoveLampImages);
            SetIcons(m_KeyboardIcons.BasicAttack, m_IconReferences.BasicAttackImages);
            SetIcons(m_KeyboardIcons.SpecialAbility, m_IconReferences.SpecialAbilityImages);

            SetIcons(m_KeyboardIcons.ToggleColor, m_IconReferences.ToggleColorImages);
            SetIcons(m_KeyboardIcons.NextColor, m_IconReferences.NextColorImages);
            SetIcons(m_KeyboardIcons.PreviousColor, m_IconReferences.PreviousColorImages);

            SetIcons(null, m_IconReferences.MoveImages); // Disable MoveImages for keyboard
        }
        else
        {
            SetIcons(m_ControllerIcons.Move, m_IconReferences.MoveImages);
            SetIcons(m_ControllerIcons.Jump, m_IconReferences.JumpImages);
            SetIcons(m_ControllerIcons.Run, m_IconReferences.RunImages);

            SetIcons(m_ControllerIcons.Summon, m_IconReferences.SummonImages);
            SetIcons(m_ControllerIcons.MoveLamp, m_IconReferences.MoveLampImages);
            SetIcons(m_ControllerIcons.BasicAttack, m_IconReferences.BasicAttackImages);
            SetIcons(m_ControllerIcons.SpecialAbility, m_IconReferences.SpecialAbilityImages);

            SetIcons(m_ControllerIcons.ToggleColor, m_IconReferences.ToggleColorImages);
            SetIcons(m_ControllerIcons.NextColor, m_IconReferences.NextColorImages);
            SetIcons(m_ControllerIcons.PreviousColor, m_IconReferences.PreviousColorImages);
            
            SetIcons(null, m_IconReferences.MoveLeftImages); // Disable MoveLeftImages for controller
            SetIcons(null, m_IconReferences.MoveRightImages); // Disable MoveRightImages for controller
        }
    }

    private void SetIcons(Sprite icon, SpriteRenderer[] renderers)
    {
        if (renderers != null)
        {
            foreach (var renderer in renderers)
            {
                if (renderer != null) renderer.sprite = icon;
            }
        }
    }
}

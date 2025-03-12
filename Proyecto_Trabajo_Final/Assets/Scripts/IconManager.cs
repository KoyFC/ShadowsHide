using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
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
        public SpriteRenderer MoveLeftImage;
        public SpriteRenderer MoveRightImage;
        public SpriteRenderer MoveImage;
        public SpriteRenderer JumpImage;
        public SpriteRenderer RunImage;

        [Header("Lamp")]
        public SpriteRenderer SummonImage;
        public SpriteRenderer MoveLampImage;
        public SpriteRenderer BasicAttackImage;
        public SpriteRenderer SpecialAbilityImage;

        [Header("Colors")]
        public SpriteRenderer ToggleColorImage;
        public SpriteRenderer NextColorImage;
        public SpriteRenderer PreviousColorImage;
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
            SetIcon(m_KeyboardIcons.MoveLeft, m_IconReferences.MoveLeftImage);
            SetIcon(m_KeyboardIcons.MoveRight, m_IconReferences.MoveRightImage);
            SetIcon(m_KeyboardIcons.Jump, m_IconReferences.JumpImage);
            SetIcon(m_KeyboardIcons.Run, m_IconReferences.RunImage);
            
            SetIcon(m_KeyboardIcons.Summon, m_IconReferences.SummonImage);
            SetIcon(m_KeyboardIcons.MoveLamp, m_IconReferences.MoveLampImage);
            SetIcon(m_KeyboardIcons.BasicAttack, m_IconReferences.BasicAttackImage);
            SetIcon(m_KeyboardIcons.SpecialAbility, m_IconReferences.SpecialAbilityImage);

            SetIcon(m_KeyboardIcons.ToggleColor, m_IconReferences.ToggleColorImage);
            SetIcon(m_KeyboardIcons.NextColor, m_IconReferences.NextColorImage);
            SetIcon(m_KeyboardIcons.PreviousColor, m_IconReferences.PreviousColorImage);

            SetIcon(null, m_IconReferences.MoveImage); // Disable MoveImage for keyboard
        }
        else
        {
            SetIcon(m_ControllerIcons.Move, m_IconReferences.MoveImage);
            SetIcon(m_ControllerIcons.Jump, m_IconReferences.JumpImage);
            SetIcon(m_ControllerIcons.Run, m_IconReferences.RunImage);

            SetIcon(m_ControllerIcons.Summon, m_IconReferences.SummonImage);
            SetIcon(m_ControllerIcons.MoveLamp, m_IconReferences.MoveLampImage);
            SetIcon(m_ControllerIcons.BasicAttack, m_IconReferences.BasicAttackImage);
            SetIcon(m_ControllerIcons.SpecialAbility, m_IconReferences.SpecialAbilityImage);

            SetIcon(m_ControllerIcons.ToggleColor, m_IconReferences.ToggleColorImage);
            SetIcon(m_ControllerIcons.NextColor, m_IconReferences.NextColorImage);
            SetIcon(m_ControllerIcons.PreviousColor, m_IconReferences.PreviousColorImage);
            
            SetIcon(null, m_IconReferences.MoveLeftImage); // Disable MoveLeftImage for controller
            SetIcon(null, m_IconReferences.MoveRightImage); // Disable MoveRightImage for controller
        }
    }

    private void SetIcon(Sprite icon, SpriteRenderer renderer)
    {
        if (renderer != null) renderer.sprite = icon;
    }
}

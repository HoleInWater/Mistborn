using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.World
{
    public class WorldInteraction : MonoBehaviour
    {
        public enum InteractionType
        {
            Door,
            Lever,
            Button,
            PressurePlate,
            Chest,
            Ladder,
            ClimbingWall
        }

        [Header("Interaction")]
        [SerializeField] protected InteractionType m_interactionType = InteractionType.Door;
        [SerializeField] protected string m_interactionPrompt = "Press [E] to interact";
        [SerializeField] protected bool m_requireKey = false;
        [SerializeField] protected string m_requiredKeyId;

        [Header("Animation")]
        [SerializeField] protected bool m_useAnimation = true;
        [SerializeField] protected string m_animTrigger = "Interact";
        [SerializeField] protected float m_animationDuration = 1f;

        [Header("Audio")]
        [SerializeField] protected AudioClip m_interactSound;
        [SerializeField] protected AudioClip[] m_denySounds;

        protected bool m_isActivated;
        protected Animator m_animator;
        protected AudioSource m_audioSource;
        protected Renderer m_renderer;
        protected Material m_originalMaterial;

        public event System.Action<WorldInteraction> OnInteract;
        public event System.Action<WorldInteraction> OnActivate;
        public event System.Action<WorldInteraction> OnDeny;

        public bool IsActivated => m_isActivated;
        public string Prompt => m_interactionPrompt;

        protected virtual void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_audioSource = GetComponent<AudioSource>();
            m_renderer = GetComponent<Renderer>();

            if (m_audioSource == null && GetComponent<AudioSource>() == null)
            {
                m_audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public virtual bool CanInteract(GameObject player)
        {
            return !m_requireKey || HasKey(player);
        }

        protected virtual bool HasKey(GameObject player)
        {
            if (string.IsNullOrEmpty(m_requiredKeyId)) return true;

            PlayerInventory inventory = player?.GetComponent<PlayerInventory>();
            return inventory?.HasItem(m_requiredKeyId) ?? false;
        }

        public virtual void Interact(GameObject player)
        {
            if (m_isActivated && !CanRepeatActivate()) return;

            if (!CanInteract(player))
            {
                OnDenyInteraction(player);
                return;
            }

            OnInteract?.Invoke(this);
            PlayInteractEffects();

            if (m_useAnimation && m_animator != null)
            {
                m_animator.SetTrigger(m_animTrigger);
            }

            ExecuteInteraction();

            if (!CanRepeatActivate())
            {
                m_isActivated = true;
            }

            OnActivate?.Invoke(this);
        }

        protected virtual bool CanRepeatActivate()
        {
            return m_interactionType == InteractionType.Lever || m_interactionType == InteractionType.Button;
        }

        protected virtual void ExecuteInteraction()
        {
        }

        protected virtual void OnDenyInteraction(GameObject player)
        {
            OnDeny?.Invoke(this);
            PlayDenySound();
        }

        protected void PlayInteractEffects()
        {
            if (m_interactSound != null && m_audioSource != null)
            {
                m_audioSource.PlayOneShot(m_interactSound);
            }
        }

        protected void PlayDenySound()
        {
            if (m_denySounds != null && m_denySounds.Length > 0 && m_audioSource != null)
            {
                AudioClip clip = m_denySounds[UnityEngine.Random.Range(0, m_denySounds.Length)];
                m_audioSource.PlayOneShot(clip);
            }
        }

        public void Reset()
        {
            m_isActivated = false;
        }
    }

    public class InteractableDoor : WorldInteraction
    {
        [Header("Door Settings")]
        [SerializeField] private float m_openAngle = 90f;
        [SerializeField] private float m_openSpeed = 3f;
        [SerializeField] private bool m_openOutward = true;
        [SerializeField] private bool m_locked = false;
        [SerializeField] private AudioClip m_openSound;
        [SerializeField] private AudioClip m_closeSound;

        private Quaternion m_closedRotation;
        private Quaternion m_openRotation;
        private bool m_isMoving;

        protected override void Awake()
        {
            base.Awake();
            m_closedRotation = transform.rotation;
            m_openRotation = m_closedRotation * Quaternion.Euler(0, m_openOutward ? m_openAngle : -m_openAngle, 0);
        }

        protected override void ExecuteInteraction()
        {
            if (m_locked) return;

            m_isMoving = true;

            if (m_audioSource != null)
            {
                m_audioSource.PlayOneShot(m_isActivated ? m_closeSound : m_openSound);
            }
        }

        private void Update()
        {
            if (!m_isMoving) return;

            Quaternion target = m_isActivated ? m_closedRotation : m_openRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, target, m_openSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, target) < 1f)
            {
                transform.rotation = target;
                m_isMoving = false;
                m_isActivated = !m_isActivated;
            }
        }
    }

    public class InteractableLever : WorldInteraction
    {
        [Header("Lever Settings")]
        [SerializeField] private float m_leverAngle = 45f;
        [SerializeField] private bool m_startsOn = false;

        [Header("Targets")]
        [SerializeField] private WorldInteraction[] m_linkedInteractions;
        [SerializeField] private GameObject[] m_linkedObjects;

        private Quaternion m_offRotation;
        private Quaternion m_onRotation;
        private float m_currentAngle;

        protected override void Awake()
        {
            base.Awake();
            m_interactionType = InteractionType.Lever;

            Vector3 forward = transform.forward;
            forward.y = 0;
            transform.forward = forward.normalized;

            m_offRotation = transform.localRotation * Quaternion.Euler(m_leverAngle, 0, 0);
            m_onRotation = transform.localRotation * Quaternion.Euler(-m_leverAngle, 0, 0);
            m_currentAngle = m_startsOn ? -m_leverAngle : m_leverAngle;

            if (m_startsOn)
            {
                m_isActivated = true;
            }
        }

        protected override void ExecuteInteraction()
        {
            m_isActivated = !m_isActivated;
            m_currentAngle = m_isActivated ? -m_leverAngle : m_leverAngle;

            foreach (WorldInteraction interact in m_linkedInteractions)
            {
                interact.Interact(gameObject);
            }

            foreach (GameObject obj in m_linkedObjects)
            {
                obj?.SetActive(!obj.activeSelf);
            }
        }

        private void Update()
        {
            float targetAngle = m_currentAngle;
            float currentPitch = transform.localEulerAngles.x;
            if (currentPitch > 180) currentPitch -= 360;

            float newPitch = Mathf.Lerp(currentPitch, targetAngle, 5f * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(newPitch, 0, 0);
        }
    }

    public class InteractableChest : WorldInteraction
    {
        [Header("Chest Settings")]
        [SerializeField] private string[] m_itemIds;
        [SerializeField] private int[] m_itemQuantities;
        [SerializeField] private bool m_emptyAfterOpen = true;
        [SerializeField] private GameObject m_lootEffect;

        [Header("Audio")]
        [SerializeField] private AudioClip m_openSound;
        [SerializeField] private AudioClip m_emptySound;

        protected override void Awake()
        {
            base.Awake();
            m_interactionType = InteractionType.Chest;
        }

        protected override bool HasKey(GameObject player)
        {
            return true;
        }

        protected override void ExecuteInteraction()
        {
            if (m_itemIds == null || m_itemIds.Length == 0)
            {
                PlayDenySound();
                return;
            }

            if (m_audioSource != null && m_openSound != null)
            {
                m_audioSource.PlayOneShot(m_openSound);
            }

            if (m_lootEffect != null)
            {
                Instantiate(m_lootEffect, transform.position + Vector3.up, Quaternion.identity);
            }

            for (int i = 0; i < m_itemIds.Length; i++)
            {
                GiveItem(m_itemIds[i], i < m_itemQuantities.Length ? m_itemQuantities[i] : 1);
            }

            if (m_emptyAfterOpen)
            {
                ClearChest();
            }
        }

        private void GiveItem(string itemId, int quantity)
        {
            PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
            inventory?.AddItem(itemId, quantity);
        }

        private void ClearChest()
        {
            m_itemIds = new string[0];
            m_itemQuantities = new int[0];
            m_requireKey = true;
            m_requiredKeyId = "EMPTY";
        }
    }

    public class InteractableLadder : WorldInteraction
    {
        [Header("Ladder Settings")]
        [SerializeField] private float m_climbSpeed = 5f;
        [SerializeField] private float m_topY = 10f;
        [SerializeField] private float m_bottomY = 0f;
        [SerializeField] private Transform m_topExit;
        [SerializeField] private Transform m_bottomExit;

        private bool m_isClimbing;
        private GameObject m_player;
        private CharacterController m_charController;
        private Vector3 m_playerVelocity;

        protected override void Awake()
        {
            base.Awake();
            m_interactionType = InteractionType.Ladder;
            m_interactionPrompt = "Press [E] to climb";
        }

        protected override bool CanInteract(GameObject player)
        {
            return !m_isClimbing;
        }

        protected override void ExecuteInteraction()
        {
            StartClimbing();
        }

        private void StartClimbing()
        {
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            float vertical = Input.GetAxis("Vertical");
            if (Mathf.Abs(vertical) > 0.1f)
            {
                m_isClimbing = true;
                m_player = other.gameObject;
                m_charController = m_player.GetComponent<CharacterController>();

                Vector3 position = m_player.transform.position;
                position += Vector3.up * vertical * m_climbSpeed * Time.deltaTime;
                position.y = Mathf.Clamp(position.y, m_bottomY, m_topY);
                m_player.transform.position = position;
            }
            else if (m_isClimbing)
            {
                StopClimbing();
            }
        }

        private void StopClimbing()
        {
            m_isClimbing = false;
        }
    }
}

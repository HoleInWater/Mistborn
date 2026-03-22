using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Environment
{
    public class LurcherPuzzle : MonoBehaviour
    {
        [Header("Puzzle Configuration")]
        [SerializeField] private int requiredMetalPieces = 4;
        [SerializeField] private float completionRadius = 2f;
        [SerializeField] private bool isPuzzleActive = true;
        
        [Header("Metal Pieces")]
        [SerializeField] private GameObject[] metalPiecePrefabs;
        [SerializeField] private Transform[] pieceSpawnPoints;
        [SerializeField] private float pieceWeight = 5f;
        
        [Header("Target Position")]
        [SerializeField] private Transform targetPosition;
        [SerializeField] private GameObject targetIndicator;
        [SerializeField] private float targetGlowIntensity = 2f;
        
        [Header("Completion")]
        [SerializeField] private GameObject completionEffect;
        [SerializeField] private AudioClip completionSound;
        [SerializeField] private AudioClip piecePlaceSound;
        [SerializeField] private bool unlockDoorOnComplete = false;
        [SerializeField] private GameObject doorToUnlock;
        
        [Header("Hints")]
        [SerializeField] private bool showHints = true;
        [SerializeField] private float hintRevealRadius = 15f;
        [SerializeField] private GameObject hintVfx;
        
        private List<PuzzlePiece> activePieces = new List<PuzzlePiece>();
        private int placedPieces = 0;
        private bool isCompleted = false;
        private bool[] piecePlacementStatus;
        
        private struct PuzzlePiece
        {
            public GameObject piece;
            public Vector3 targetPosition;
            public bool isPlaced;
            public Vector3 originalPosition;
        }
        
        private void Start()
        {
            piecePlacementStatus = new bool[requiredMetalPieces];
            
            if (targetIndicator != null)
            {
                targetIndicator.SetActive(false);
            }
            
            if (isPuzzleActive)
            {
                InitializePuzzle();
            }
        }
        
        private void InitializePuzzle()
        {
            for (int i = 0; i < requiredMetalPieces && i < pieceSpawnPoints.Length; i++)
            {
                if (metalPiecePrefabs == null || metalPiecePrefabs.Length <= i)
                    continue;
                
                GameObject piece = Instantiate(metalPiecePrefabs[i], pieceSpawnPoints[i].position, Quaternion.identity);
                
                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.mass = pieceWeight;
                rb.drag = 2f;
                rb.angularDrag = 2f;
                
                LurcherPullable pullable = piece.AddComponent<LurcherPullable>();
                pullable.Initialize(this);
                
                PuzzlePiece puzzlePiece = new PuzzlePiece
                {
                    piece = piece,
                    targetPosition = targetPosition != null ? targetPosition.position : transform.position,
                    isPlaced = false,
                    originalPosition = pieceSpawnPoints[i].position
                };
                
                activePieces.Add(puzzlePiece);
                
                piecePlacementStatus[i] = false;
            }
            
            if (showHints)
            {
                RevealHints();
            }
        }
        
        private void Update()
        {
            if (!isPuzzleActive || isCompleted)
                return;
            
            CheckPiecePlacement();
            UpdateTargetIndicator();
        }
        
        private void CheckPiecePlacement()
        {
            for (int i = 0; i < activePieces.Count; i++)
            {
                if (activePieces[i].isPlaced)
                    continue;
                
                PuzzlePiece piece = activePieces[i];
                
                if (piece.piece == null)
                    continue;
                
                float distanceToTarget = Vector3.Distance(piece.piece.transform.position, piece.targetPosition);
                
                if (distanceToTarget <= completionRadius)
                {
                    PlacePiece(i);
                }
            }
        }
        
        private void PlacePiece(int pieceIndex)
        {
            PuzzlePiece piece = activePieces[pieceIndex];
            piece.isPlaced = true;
            activePieces[pieceIndex] = piece;
            
            Rigidbody rb = piece.piece.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            piece.piece.transform.position = piece.targetPosition;
            
            AudioSource.PlayClipAtPoint(piecePlaceSound, transform.position);
            
            placedPieces++;
            piecePlacementStatus[pieceIndex] = true;
            
            if (placedPieces >= requiredMetalPieces)
            {
                CompletePuzzle();
            }
        }
        
        private void CompletePuzzle()
        {
            isCompleted = true;
            
            AudioSource.PlayClipAtPoint(completionSound, transform.position);
            
            if (completionEffect != null)
            {
                GameObject effect = Instantiate(completionEffect, targetPosition.position, Quaternion.identity);
                Destroy(effect, 5f);
            }
            
            if (targetIndicator != null)
            {
                targetIndicator.SetActive(false);
            }
            
            if (unlockDoorOnComplete && doorToUnlock != null)
            {
                MetalDoor door = doorToUnlock.GetComponent<MetalDoor>();
                if (door != null)
                {
                    door.Unlock();
                    door.Open();
                }
            }
            
            OnPuzzleComplete();
        }
        
        private void OnPuzzleComplete()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 20f);
            foreach (var col in colliders)
            {
                col.SendMessage("OnPuzzleSolved", SendMessageOptions.DontRequireReceiver);
            }
        }
        
        private void UpdateTargetIndicator()
        {
            if (targetIndicator == null)
                return;
            
            targetIndicator.SetActive(true);
            
            float glow = Mathf.PingPong(Time.time, 1f) * targetGlowIntensity;
            Light targetLight = targetIndicator.GetComponent<Light>();
            if (targetLight != null)
            {
                targetLight.intensity = glow;
            }
        }
        
        private void RevealHints()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, hintRevealRadius);
            foreach (var col in colliders)
            {
                if (col.GetComponent<LurcherPullable>() != null)
                {
                    if (hintVfx != null)
                    {
                        GameObject hint = Instantiate(hintVfx, col.transform.position, Quaternion.identity);
                        Destroy(hint, 3f);
                    }
                }
            }
        }
        
        public void ResetPuzzle()
        {
            isCompleted = false;
            placedPieces = 0;
            
            for (int i = 0; i < activePieces.Count; i++)
            {
                if (activePieces[i].piece != null)
                {
                    Destroy(activePieces[i].piece);
                }
            }
            activePieces.Clear();
            
            for (int i = 0; i < piecePlacementStatus.Length; i++)
            {
                piecePlacementStatus[i] = false;
            }
            
            InitializePuzzle();
        }
        
        public bool IsCompleted()
        {
            return isCompleted;
        }
        
        public int GetPlacedPieces()
        {
            return placedPieces;
        }
        
        public int GetRequiredPieces()
        {
            return requiredMetalPieces;
        }
    }
    
    public class LurcherPullable : MonoBehaviour
    {
        [SerializeField] private float pullForce = 20f;
        [SerializeField] private float maxPullDistance = 50f;
        [SerializeField] private bool canBePushed = true;
        [SerializeField] private float pushForce = 15f;
        
        private LurcherPuzzle parentPuzzle;
        private Rigidbody rb;
        private bool isBeingPulled = false;
        
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }
        
        public void Initialize(LurcherPuzzle puzzle)
        {
            parentPuzzle = puzzle;
        }
        
        private void Update()
        {
            if (isBeingPulled && rb != null)
            {
                rb.AddForce(transform.forward * pullForce * Time.deltaTime, ForceMode.Acceleration);
            }
        }
        
        public void StartPulling(Transform source)
        {
            Vector3 direction = (source.position - transform.position).normalized;
            transform.forward = direction;
            isBeingPulled = true;
        }
        
        public void StopPulling()
        {
            isBeingPulled = false;
        }
        
        public void ApplyPushForce(Vector3 direction)
        {
            if (!canBePushed || rb == null)
                return;
            
            direction.y = 0.3f;
            rb.AddForce(direction * pushForce * 100f, ForceMode.Impulse);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class AllomancerTraining : MonoBehaviour
    {
        [Header("Training Settings")]
        [SerializeField] private TrainingType trainingType = TrainingType.BasicCombat;
        [SerializeField] private int requiredReputation = 0;
        [SerializeField] private float trainingCost = 100f;
        [SerializeField] private float trainingDuration = 60f;
        
        [Header("Skill Gains")]
        [SerializeField] private int strengthGain = 5;
        [SerializeField] private int speedGain = 3;
        [SerializeField] private int allomanticPowerGain = 10;
        [SerializeField] private string[] teachablePowers;
        
        [Header("Training Benefits")]
        [SerializeField] private bool unlockNewAbilities = true;
        [SerializeField] private float statBoostDuration = 300f;
        [SerializeField] private float damageBoost = 1.2f;
        
        [Header("Visual")]
        [SerializeField] private GameObject trainingArea;
        [SerializeField] private ParticleSystem trainingEffect;
        [SerializeField] private AudioClip trainingSound;
        
        [Header("NPC Trainer")]
        [SerializeField] private string trainerName = "Master Allomancer";
        [SerializeField] private Sprite trainerPortrait;
        
        private bool isTraining = false;
        private float trainingStartTime;
        private GameObject currentTrainee;
        private PlayerStats traineeStats;
        private Allomancer traineeAllomancer;
        
        public enum TrainingType
        {
            BasicCombat,
            AdvancedCombat,
            Stealth,
            AllomanticPowers,
            FeruchemyBasics,
            MetalControl,
            MovementMastery
        }
        
        private void Start()
        {
            if (trainingArea != null)
            {
                trainingArea.SetActive(false);
            }
            
            if (trainingEffect != null)
            {
                trainingEffect.Stop();
            }
        }
        
        public bool CanTrain(GameObject trainee)
        {
            PlayerStats stats = trainee.GetComponent<PlayerStats>();
            if (stats == null)
            {
                return false;
            }
            
            if (stats.GetGold() < trainingCost)
            {
                return false;
            }
            
            ReputationSystem repSystem = trainee.GetComponent<ReputationSystem>();
            if (repSystem != null && repSystem.GetReputation() < requiredReputation)
            {
                return false;
            }
            
            return true;
        }
        
        public void StartTraining(GameObject trainee)
        {
            if (isTraining)
            {
                return;
            }
            
            if (!CanTrain(trainee))
            {
                return;
            }
            
            currentTrainee = trainee;
            traineeStats = trainee.GetComponent<PlayerStats>();
            traineeAllomancer = trainee.GetComponent<Allomancer>();
            
            if (traineeStats != null)
            {
                traineeStats.SpendGold((int)trainingCost);
            }
            
            isTraining = true;
            trainingStartTime = Time.time;
            
            if (trainingArea != null)
            {
                trainingArea.SetActive(true);
            }
            
            if (trainingEffect != null)
            {
                trainingEffect.Play();
            }
            
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null && trainingSound != null)
            {
                audioSource.clip = trainingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            
            if (OnTrainingStarted != null)
            {
                OnTrainingStarted(currentTrainee, trainingType);
            }
            
            Invoke(nameof(CompleteTraining), trainingDuration);
        }
        
        public void CancelTraining()
        {
            if (!isTraining)
            {
                return;
            }
            
            isTraining = false;
            float elapsedTime = Time.time - trainingStartTime;
            float refundPercentage = 1f - (elapsedTime / trainingDuration);
            
            if (traineeStats != null)
            {
                int refund = Mathf.RoundToInt(trainingCost * refundPercentage);
                traineeStats.AddGold(refund);
            }
            
            EndTrainingEffects();
            
            if (OnTrainingCancelled != null)
            {
                OnTrainingCancelled(currentTrainee);
            }
            
            currentTrainee = null;
            traineeStats = null;
            traineeAllomancer = null;
        }
        
        private void CompleteTraining()
        {
            if (currentTrainee == null)
            {
                return;
            }
            
            ApplyTrainingBenefits();
            
            EndTrainingEffects();
            
            if (OnTrainingCompleted != null)
            {
                OnTrainingCompleted(currentTrainee, trainingType);
            }
            
            currentTrainee = null;
            traineeStats = null;
            traineeAllomancer = null;
        }
        
        private void ApplyTrainingBenefits()
        {
            if (traineeStats != null)
            {
                traineeStats.ModifyStat(StatType.Strength, strengthGain);
                traineeStats.ModifyStat(StatType.Speed, speedGain);
                
                if (unlockNewAbilities)
                {
                    traineeStats.ModifyStat(StatType.AttackPower, damageBoost - 1f);
                }
            }
            
            if (traineeAllomancer != null && teachablePowers != null && teachablePowers.Length > 0)
            {
                foreach (string power in teachablePowers)
                {
                    traineeAllomancer.UnlockPower(power);
                }
            }
            
            if (unlockNewAbilities)
            {
                Invoke(nameof(RemoveTemporaryBoost), statBoostDuration);
            }
        }
        
        private void RemoveTemporaryBoost()
        {
            if (traineeStats != null)
            {
                traineeStats.ModifyStat(StatType.AttackPower, 1f - damageBoost);
            }
        }
        
        private void EndTrainingEffects()
        {
            isTraining = false;
            
            if (trainingArea != null)
            {
                trainingArea.SetActive(false);
            }
            
            if (trainingEffect != null)
            {
                trainingEffect.Stop();
            }
            
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
        
        public bool IsTraining()
        {
            return isTraining;
        }
        
        public float GetTrainingProgress()
        {
            if (!isTraining)
            {
                return 0f;
            }
            
            float elapsed = Time.time - trainingStartTime;
            return Mathf.Clamp01(elapsed / trainingDuration);
        }
        
        public float GetRemainingTime()
        {
            if (!isTraining)
            {
                return 0f;
            }
            
            float elapsed = Time.time - trainingStartTime;
            return Mathf.Max(0f, trainingDuration - elapsed);
        }
        
        public TrainingType GetTrainingType()
        {
            return trainingType;
        }
        
        public void SetTrainingType(TrainingType type)
        {
            trainingType = type;
        }
        
        public void SetTrainingCost(float cost)
        {
            trainingCost = cost;
        }
        
        public float GetTrainingCost()
        {
            return trainingCost;
        }
        
        public void SetRequiredReputation(int rep)
        {
            requiredReputation = rep;
        }
        
        public int GetRequiredReputation()
        {
            return requiredReputation;
        }
        
        public string GetTrainerName()
        {
            return trainerName;
        }
        
        public Sprite GetTrainerPortrait()
        {
            return trainerPortrait;
        }
        
        public event System.Action<GameObject, TrainingType> OnTrainingStarted;
        public event System.Action<GameObject, TrainingType> OnTrainingCompleted;
        public event System.Action<GameObject> OnTrainingCancelled;
    }
}

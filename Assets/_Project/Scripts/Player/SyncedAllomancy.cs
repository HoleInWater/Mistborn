using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Player
{
    public class SyncedAllomancy : MonoBehaviour
    {
        [Header("Sync Configuration")]
        [SerializeField] private bool enableSync = true;
        [SerializeField] private float syncRadius = 50f;
        [SerializeField] private float metalReservePercent = 0.3f;
        
        [Header("Metal Sharing")]
        [SerializeField] private float metalSharingRate = 1f;
        [SerializeField] private float minMetalToShare = 10f;
        [SerializeField] private float syncEfficiency = 0.8f;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem syncVfx;
        [SerializeField] private AudioClip syncSound;
        [SerializeField] private float syncInterval = 0.5f;
        
        [Header("Combat")]
        [SerializeField] private bool shareCombatBuffs = false;
        [SerializeField] private float combatBuffMultiplier = 1.1f;
        
        private Allomancy.Allomancer allomancer;
        private List<Allomancy.Allomancer> syncedAllomancers = new List<Allomancy.Allomancer>();
        private Dictionary<Allomancy.Allomancer.MetalType, float> sharedMetalReserve = new Dictionary<Allomancy.Allomancer.MetalType, float>();
        private bool isSynced = false;
        private float lastSyncTime = -100f;
        
        private void Start()
        {
            allomancer = GetComponent<Allomancy.Allomancer>();
            
            if (allomancer == null)
                enabled = false;
        }
        
        private void Update()
        {
            if (!enableSync)
                return;
            
            if (Time.time - lastSyncTime > syncInterval)
            {
                UpdateSyncedAllomancers();
                ShareMetalReserves();
                lastSyncTime = Time.time;
            }
        }
        
        private void UpdateSyncedAllomancers()
        {
            syncedAllomancers.RemoveAll(a => a == null);
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, syncRadius);
            
            foreach (var col in colliders)
            {
                Allomancy.Allomancer otherAllomancer = col.GetComponent<Allomancy.Allomancer>();
                
                if (otherAllomancer != null && otherAllomancer != allomancer)
                {
                    if (!syncedAllomancers.Contains(otherAllomancer))
                    {
                        if (ShouldSync(otherAllomancer))
                        {
                            syncedAllomancers.Add(otherAllomancer);
                            OnSyncStarted(otherAllomancer);
                        }
                    }
                }
            }
        }
        
        private bool ShouldSync(Allomancy.Allomancer other)
        {
            if (other == null || other == allomancer)
                return false;
            
            float distance = Vector3.Distance(transform.position, other.transform.position);
            
            if (distance > syncRadius)
                return false;
            
            bool otherIsBurning = other.IsBurningMetal;
            bool selfIsBurning = allomancer.IsBurningMetal;
            
            if (!otherIsBurning && !selfIsBurning)
                return false;
            
            return true;
        }
        
        private void ShareMetalReserves()
        {
            if (!isSynced || syncedAllomancers.Count == 0)
                return;
            
            UpdateSharedMetalReserve();
            
            foreach (var other in syncedAllomancers)
            {
                if (other == null)
                    continue;
                
                if (!other.IsBurningMetal)
                    continue;
                
                Allomancy.Allomancer.MetalType[] activeMetals = other.GetActiveMetals();
                
                foreach (var metal in activeMetals)
                {
                    if (sharedMetalReserve.ContainsKey(metal))
                    {
                        float sharedAmount = sharedMetalReserve[metal] / (syncedAllomancers.Count + 1);
                        
                        if (sharedAmount >= minMetalToShare)
                        {
                            other.AddMetalReserve(metal, sharedAmount * syncEfficiency);
                        }
                    }
                }
            }
        }
        
        private void UpdateSharedMetalReserve()
        {
            sharedMetalReserve.Clear();
            
            foreach (var metal in System.Enum.GetValues(typeof(Allomancy.Allomancer.MetalType)))
            {
                float totalReserve = allomancer.GetMetalReserve((Allomancy.Allomancer.MetalType)metal);
                
                totalReserve += allomancer.GetMetalReserve((Allomancy.Allomancer.MetalType)metal) * metalReservePercent;
                
                foreach (var other in syncedAllomancers)
                {
                    if (other != null)
                    {
                        totalReserve += other.GetMetalReserve((Allomancy.Allomancer.MetalType)metal) * metalReservePercent;
                    }
                }
                
                sharedMetalReserve[(Allomancy.Allomancer.MetalType)metal] = totalReserve;
            }
        }
        
        private void OnSyncStarted(Allomancy.Allomancer other)
        {
            isSynced = true;
            
            AudioSource.PlayClipAtPoint(syncSound, transform.position);
            
            if (syncVfx != null)
            {
                ParticleSystem vfx = Instantiate(syncVfx, transform.position, Quaternion.identity);
                vfx.Play();
                Destroy(vfx.gameObject, 3f);
            }
            
            other.OnSyncJoined();
        }
        
        public void StartSync()
        {
            if (isSynced)
                return;
            
            isSynced = true;
            
            if (syncVfx != null)
            {
                syncVfx.Play();
            }
        }
        
        public void EndSync()
        {
            if (!isSynced)
                return;
            
            isSynced = false;
            
            foreach (var other in syncedAllomancers)
            {
                if (other != null)
                {
                    other.OnSyncEnded();
                }
            }
            
            syncedAllomancers.Clear();
            
            if (syncVfx != null)
            {
                syncVfx.Stop();
            }
        }
        
        public float GetSyncedMetalAmount(Allomancy.Allomancer.MetalType metal)
        {
            if (sharedMetalReserve.ContainsKey(metal))
            {
                return sharedMetalReserve[metal];
            }
            return 0f;
        }
        
        public int GetSyncedAllomancerCount()
        {
            return syncedAllomancers.Count;
        }
        
        public bool IsSynced => isSynced;
    }
}

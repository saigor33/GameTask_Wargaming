using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityNightPool
{
    [CreateAssetMenu(fileName = "PoolConfig")]
    public class PoolConfig : ScriptableObject
    {
        [SerializeField] private List<PoolSetup> _pools = new List<PoolSetup>();
        private static PoolConfig _instance;

        public static List<PoolSetup> Pools
        {
            get { return Instance._pools; }
        }

        public static PoolConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (PoolConfig)Resources.Load("PoolConfig");
                    if (_instance == null)
                        _instance = PoolConfig.CreateInstance<PoolConfig>();
                }
                return _instance;
            }
        }

    }
}
using System.Collections.Generic;
using UnityEngine;

namespace UnityNightPool 
{
	public static class PoolManager
    {

		private static List<Pool> _pools = new List<Pool>();
		private static bool init = false;

		public static Transform Parent { get; private set; }

        public static PoolObject Get(int key) 
        {
			Init ();
			var p = _pools.Find (x => x.GetId == key);
            return (p != null) ? p.Get() : null;
		}

        public static bool CheckHaveIndexPrefab(int key)
        {
            Init();
            var p = _pools.Find(x => x.GetId == key);
            return (p != null) ? true : false;
        }

        private static void Init() 
        {
            if (!init) 
            {
                Parent = (new GameObject("Pools")).transform;
                Object.DontDestroyOnLoad(Parent.gameObject);

                for (int i = 0; i < PoolConfig.Pools.Count; i++) 
                {
                    if (PoolConfig.Pools[i].prefab != null) 
                    {
                        Pool p = new Pool(PoolConfig.Pools[i]);
                        _pools.Add(p);
                    }
                }
				init = true;
			}
		}
	}
}
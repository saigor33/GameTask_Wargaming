using System.Collections.Generic;
using UnityEngine;

namespace UnityNightPool {
	public class Pool
    {
        private List<PoolObject> _spawned = new List<PoolObject>();

	    public PoolSetup Setup { get; private set; }

        public int GetId 
        {
            get { return Setup.id; }
        }


		public void ReturnAll() 
        {
			for (int i = 0; i < _spawned.Count; i++) 
            {
				if (_spawned[i]!=null && !_spawned [i].Free)
					_spawned [i].Return ();
			}
		}

		public Pool (PoolSetup setup) 
        {
			this.Setup = setup;
		}


	    private PoolObject Create(int index = -1)
	    {
	        GameObject obj = Object.Instantiate(Setup.prefab.gameObject) as GameObject;
	        obj.name = Setup.prefab.name + "(Pool)";
	        var p = obj.GetComponent<PoolObject>();
	        if (p != null)
	        {
	            p.Init(this);
	            if (index == -1)
	                _spawned.Add(p);
	            else
	                _spawned[index] = p;
	        }
	        else
	            Object.Destroy(obj);

	        return p;
	    }

	    public PoolObject Get()
	    {
	        for (int i = 0; i < _spawned.Count; i++)
	        {
	            if (_spawned[i] == null)
	            {
	                Create(i);
	                _spawned[i].Push();
	                return _spawned[i];
	            }
	            if (_spawned[i].Free)
	            {
	                _spawned[i].Push();
	                return _spawned[i];
	            }
	        }
	        var p = Create();
	        p.Push();

	        return p;
	    }
    }
}
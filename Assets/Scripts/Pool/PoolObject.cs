using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityNightPool 
{
	public class PoolObject : MonoBehaviour
    {
	    private bool _init;
	    private bool _switchGameObject;

        public bool Free { get; private set; } = true;

        public void Init(Pool pool)
	    {
	        if (!_init)
	        {
	            _init = true;
	            _switchGameObject = pool.Setup.switchGameObject;

	            if (_switchGameObject) 
                    gameObject.SetActive(false);
	        }
	    }

        private void OnLevelWasLoaded(int level)
        {
            Return();
        }

        public void Push() 
        {
			Free = false;
            if (_switchGameObject) 
                gameObject.SetActive(true);
        }
			
		public void Return() 
        {
		    if (_init)
		    {
		        Free = true;
		        transform.SetParent(PoolManager.Parent);

                if (_switchGameObject) 
                    gameObject.SetActive(false);
		    }
		    else
		    {
		        Destroy(gameObject);
		    }
        }
	}
}
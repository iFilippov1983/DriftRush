using UnityEngine;

namespace RaceManager
{
    public class LoaderCallback : MonoBehaviour
    {
        private bool _firstUpdate = true;
        
        void Update()
        {
            if (_firstUpdate)
            { 
                Loader.InvokeLoadUsing(this);
                _firstUpdate = false;
            }
        }
    }
}

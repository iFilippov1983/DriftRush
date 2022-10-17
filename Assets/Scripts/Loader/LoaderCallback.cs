using UnityEngine;

namespace RaceManager
{
    public class LoaderCallback : MonoBehaviour
    {
        private bool firstUpdate = true;
        
        void Update()
        {
            if (firstUpdate)
            { 
                Loader.InvokeLoadUsing(this);
                firstUpdate = false;
            }
        }
    }
}

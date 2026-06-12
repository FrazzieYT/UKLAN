using UnityEngine;

namespace UltraLANCoop.UI
{
    public abstract class UiWindow : MonoBehaviour
    {
        protected Rect windowRect;
        public bool isVisible = false;
        
        protected virtual void Awake()
        {
            windowRect = new Rect(100, 100, 400, 300);
        }
        
        public void Toggle()
        {
            isVisible = !isVisible;
        }
        
        protected virtual void OnGUI()
        {
            if (!isVisible) return;
            
            try
            {
                windowRect = GUI.Window(GetWindowId(), windowRect, DrawWindow, "UltraLANCoop");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[" + GetType().Name + "] OnGUI error: " + e.Message);
            }
        }
        
        protected abstract int GetWindowId();
        protected abstract void DrawWindow(int id);
    }
}
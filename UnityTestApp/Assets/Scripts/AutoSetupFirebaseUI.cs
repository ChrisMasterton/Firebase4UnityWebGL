using UnityEngine;

[System.Serializable]
public class AutoSetupFirebaseUI : MonoBehaviour
{
    [Header("Auto Setup Configuration")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool destroyAfterSetup = true;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupUI();
        }
    }
    
    public void SetupUI()
    {
        // Find or create the FirebaseUISetup component
        FirebaseUISetup uiSetup = FindFirstObjectByType<FirebaseUISetup>();
        
        if (uiSetup == null)
        {
            GameObject setupGO = new GameObject("FirebaseUISetup");
            uiSetup = setupGO.AddComponent<FirebaseUISetup>();
        }
        
        // Setup the UI
        uiSetup.SetupFirebaseUI();
        
        // Optionally destroy this component after setup
        if (destroyAfterSetup)
        {
            Destroy(this);
        }
    }
}
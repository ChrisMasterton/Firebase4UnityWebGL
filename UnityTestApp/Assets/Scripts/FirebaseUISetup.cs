using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirebaseUISetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    public Canvas canvas;
    
    [Header("UI References (Auto-populated)")]
    public FirebaseManager firebaseManager;
    public GameObject authPanel;
    public GameObject firestorePanel;

    void Start()
    {
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        SetupFirebaseUI();
    }

    [ContextMenu("Setup Firebase UI")]
    public void SetupFirebaseUI()
    {
        if (canvas == null)
        {
            Debug.LogError("Canvas not found! Please assign a Canvas to setup the UI.");
            return;
        }

        // Create or find Firebase Manager
        firebaseManager = FindFirstObjectByType<FirebaseManager>();
        if (firebaseManager == null)
        {
            GameObject managerGO = new GameObject("FirebaseManager");
            firebaseManager = managerGO.AddComponent<FirebaseManager>();
        }

        // Setup Authentication Panel
        SetupAuthPanel();
        
        // Setup Firestore Panel
        SetupFirestorePanel();

        Debug.Log("Firebase UI Setup Complete!");
    }

    void SetupAuthPanel()
    {
        // Create Auth Panel
        authPanel = CreatePanel("AuthPanel", new Vector2(0, 100), new Vector2(400, 200));
        
        // Title
        CreateText(authPanel, "Auth Title", "Firebase Authentication", new Vector2(0, 70), new Vector2(300, 30), 16);
        
        // Username field
        GameObject usernameFieldGO = CreateInputField(authPanel, "UsernameField", "Enter username...", new Vector2(0, 30), new Vector2(300, 30));
        firebaseManager.usernameField = usernameFieldGO.GetComponent<TMP_InputField>();
        
        // Password field
        GameObject passwordFieldGO = CreateInputField(authPanel, "PasswordField", "Enter password...", new Vector2(0, -10), new Vector2(300, 30));
        firebaseManager.passwordField = passwordFieldGO.GetComponent<TMP_InputField>();
        firebaseManager.passwordField.contentType = TMP_InputField.ContentType.Password;
        
        // Login Button
        GameObject loginBtnGO = CreateButton(authPanel, "LoginButton", "Login", new Vector2(-80, -50), new Vector2(100, 30));
        firebaseManager.loginButton = loginBtnGO.GetComponent<Button>();
        
        // Logout Button
        GameObject logoutBtnGO = CreateButton(authPanel, "LogoutButton", "Logout", new Vector2(80, -50), new Vector2(100, 30));
        firebaseManager.logoutButton = logoutBtnGO.GetComponent<Button>();
        
        // Auth Status Text
        GameObject authStatusGO = CreateText(authPanel, "AuthStatus", "Not authenticated", new Vector2(0, -90), new Vector2(350, 30), 12);
        firebaseManager.authStatusText = authStatusGO.GetComponent<TextMeshProUGUI>();
        firebaseManager.authStatusText.color = Color.yellow;
    }

    void SetupFirestorePanel()
    {
        // Create Firestore Panel
        firestorePanel = CreatePanel("FirestorePanel", new Vector2(0, -150), new Vector2(400, 250));
        
        // Title
        CreateText(firestorePanel, "Firestore Title", "Firestore Database", new Vector2(0, 100), new Vector2(300, 30), 16);
        
        // Data Key field
        GameObject keyFieldGO = CreateInputField(firestorePanel, "DataKeyField", "Enter data key...", new Vector2(0, 60), new Vector2(300, 30));
        firebaseManager.dataKeyField = keyFieldGO.GetComponent<TMP_InputField>();
        
        // Data Value field
        GameObject valueFieldGO = CreateInputField(firestorePanel, "DataValueField", "Enter data value...", new Vector2(0, 20), new Vector2(300, 30));
        firebaseManager.dataValueField = valueFieldGO.GetComponent<TMP_InputField>();
        
        // Buttons Row 1
        GameObject saveBtnGO = CreateButton(firestorePanel, "SaveButton", "Save", new Vector2(-100, -20), new Vector2(80, 30));
        firebaseManager.saveDataButton = saveBtnGO.GetComponent<Button>();
        
        GameObject loadBtnGO = CreateButton(firestorePanel, "LoadButton", "Load", new Vector2(0, -20), new Vector2(80, 30));
        firebaseManager.loadDataButton = loadBtnGO.GetComponent<Button>();
        
        GameObject deleteBtnGO = CreateButton(firestorePanel, "DeleteButton", "Delete", new Vector2(100, -20), new Vector2(80, 30));
        firebaseManager.deleteDataButton = deleteBtnGO.GetComponent<Button>();
        
        // Firestore Status Text
        GameObject firestoreStatusGO = CreateText(firestorePanel, "FirestoreStatus", "Ready to use", new Vector2(0, -60), new Vector2(350, 30), 12);
        firebaseManager.firestoreStatusText = firestoreStatusGO.GetComponent<TextMeshProUGUI>();
        firebaseManager.firestoreStatusText.color = Color.cyan;
        
        // Loaded Data Display
        GameObject loadedDataGO = CreateText(firestorePanel, "LoadedData", "No data loaded", new Vector2(0, -100), new Vector2(350, 60), 10);
        firebaseManager.loadedDataText = loadedDataGO.GetComponent<TextMeshProUGUI>();
        firebaseManager.loadedDataText.color = Color.white;
        firebaseManager.loadedDataText.alignment = TextAlignmentOptions.TopLeft;
    }

    GameObject CreatePanel(string name, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        return panel;
    }

    GameObject CreateText(GameObject parent, string name, string text, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent.transform, false);
        
        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        
        return textGO;
    }

    GameObject CreateInputField(GameObject parent, string name, string placeholderString, Vector2 position, Vector2 size)
    {
        GameObject inputFieldGO = new GameObject(name);
        inputFieldGO.transform.SetParent(parent.transform, false);
        
        RectTransform rect = inputFieldGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = inputFieldGO.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.1f);
        
        TMP_InputField inputField = inputFieldGO.AddComponent<TMP_InputField>();
        
        // Create Text Area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputFieldGO.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.sizeDelta = Vector2.zero;
        textAreaRect.offsetMin = new Vector2(10, 6);
        textAreaRect.offsetMax = new Vector2(-10, -7);
        
        // Create Placeholder
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(textArea.transform, false);
        RectTransform placeholderRect = placeholder.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholderString;
        placeholderText.fontSize = 12;
        placeholderText.color = new Color(1f, 1f, 1f, 0.5f);
        placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
        
        // Create Text
        GameObject text = new GameObject("Text");
        text.transform.SetParent(textArea.transform, false);
        RectTransform textRect = text.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI textComponent = text.AddComponent<TextMeshProUGUI>();
        textComponent.fontSize = 12;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.MidlineLeft;
        
        // Setup InputField references
        inputField.textViewport = textAreaRect;
        inputField.textComponent = textComponent;
        inputField.placeholder = placeholderText;
        
        return inputFieldGO;
    }

    GameObject CreateButton(GameObject parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent.transform, false);
        
        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.4f, 0.6f, 1f, 1f);
        
        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;
        
        // Create button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 12;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        
        return buttonGO;
    }
}
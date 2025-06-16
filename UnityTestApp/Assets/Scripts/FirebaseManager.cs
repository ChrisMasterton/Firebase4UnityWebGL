using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Core;
using Firebase.Firestore;

public class FirebaseManager : MonoBehaviour
{
    [SerializeField]
    public FirebaseConfig firebaseConfig;

    public FirebaseAuth firebaseAuth;
    public FirebaseFirestore firebaseFirestore;
    
    [Header("Authentication UI")]
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public Button loginButton;
    public Button logoutButton;
    public TextMeshProUGUI authStatusText;

    [Header("Firestore UI")]
    public TMP_InputField dataKeyField;
    public TMP_InputField dataValueField;
    public Button saveDataButton;
    public Button loadDataButton;
    public Button deleteDataButton;
    public TextMeshProUGUI firestoreStatusText;
    public TextMeshProUGUI loadedDataText;

    private bool isAuthenticated = false;
    private Dictionary<string, string> localData = new Dictionary<string, string>();

    async void Start()
    {
        InitializeUI();
        UpdateAuthStatus("Initializing Firebase...");
        UpdateFirestoreStatus("Waiting for Firebase...");

        firebaseConfig = FindFirstObjectByType<FirebaseConfigSetup>().firebaseConfig;
        
        if (firebaseConfig.IsValid())
        {
            Debug.Log("Firebase configuration is valid!");
        
            UpdateAuthStatus("FirebaseConfig not found!");
            UpdateFirestoreStatus("Add FirebaseConfig component to scene");

            firebaseAuth = new FirebaseAuth(firebaseConfig);
            Debug.Log("Firebase Auth initialized successfully!");
        }
        else
        {
            Debug.LogError("Firebase configuration is invalid!");
        }
    }

    void InitializeUI()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(Login);
        
        if (logoutButton != null)
            logoutButton.onClick.AddListener(Logout);
        
        if (saveDataButton != null)
            saveDataButton.onClick.AddListener(SaveData);
        
        if (loadDataButton != null)
            loadDataButton.onClick.AddListener(LoadData);
        
        if (deleteDataButton != null)
            deleteDataButton.onClick.AddListener(DeleteData);

        UpdateButtonStates();
    }

    public async void Login()
    {
        if (firebaseConfig == null)
        {
            UpdateAuthStatus("Firebase not initialized");
            return;
        }

        string email = usernameField?.text ?? "";
        string password = passwordField?.text ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateAuthStatus("Please enter email and password");
            return;
        }

        UpdateAuthStatus("Authenticating...");
        
        try
        {
            var authResult = await firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
            var user = authResult;
            
            if (user != null)
            {
                isAuthenticated = true;
                UpdateAuthStatus($"Authenticated as: {user.Email}");
                UpdateButtonStates();
            }
        }
        catch (System.Exception ex)
        {
            UpdateAuthStatus($"Login failed: {ex.Message}");
            
            // If user doesn't exist, try to create account
            if (ex.Message.Contains("no user record"))
            {
                await CreateAccount(email, password);
            }
        }
    }

    async Task CreateAccount(string email, string password)
    {
        try
        {
            UpdateAuthStatus("Creating new account...");
            var authResult = await firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);
            var user = authResult;
            
            if (user != null)
            {
                isAuthenticated = true;
                UpdateAuthStatus($"Account created and signed in: {user.Email}");
                UpdateButtonStates();
            }
            
            firebaseFirestore = new FirebaseFirestore(firebaseConfig, firebaseAuth);
            if( firebaseFirestore != null)
            {
                UpdateFirestoreStatus("Firestore initialized successfully");
            }
            else
            {
                UpdateFirestoreStatus("Failed to initialize Firestore");
            }
        }
        catch (System.Exception ex)
        {
            UpdateAuthStatus($"Account creation failed: {ex.Message}");
        }
    }

    public void Logout()
    {
        if (firebaseAuth != null)
        {
            firebaseAuth.SignOut();
        }
        
        isAuthenticated = false;
        usernameField.text = "";
        passwordField.text = "";
        UpdateAuthStatus("Logged out successfully");
        UpdateButtonStates();
    }

    public async void SaveData()
    {
        if (!isAuthenticated || firebaseFirestore == null)
        {
            UpdateFirestoreStatus("Please login first and ensure Firebase is initialized");
            return;
        }

        string key = dataKeyField?.text ?? "";
        string value = dataValueField?.text ?? "";

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        {
            UpdateFirestoreStatus("Please enter both key and value");
            return;
        }

        UpdateFirestoreStatus("Saving data...");

        try
        {
            string userId = firebaseAuth.CurrentUser.Uid;
            var docRef = firebaseFirestore.Collection("users").Document(userId).Collection("data").Document(key);
            
            await docRef.SetAsync(new Dictionary<string, object>
            {
                {"value", value},
                {"timestamp", DateTime.UtcNow.ToString()}
            });

            UpdateFirestoreStatus($"Data saved: {key} = {value}");
            
            // Clear input fields
            dataKeyField.text = "";
            dataValueField.text = "";
        }
        catch (System.Exception ex)
        {
            UpdateFirestoreStatus($"Save failed: {ex.Message}");
        }
    }

    public async void LoadData()
    {
        if (!isAuthenticated || firebaseFirestore == null)
        {
            UpdateFirestoreStatus("Please login first and ensure Firebase is initialized");
            return;
        }

        string key = dataKeyField?.text ?? "";

        if (string.IsNullOrEmpty(key))
        {
            UpdateFirestoreStatus("Please enter a key to load");
            return;
        }

        UpdateFirestoreStatus("Loading data...");

        try
        {
            string userId = firebaseAuth.CurrentUser.Uid;
            var docRef = firebaseFirestore.Collection("users").Document(userId).Collection("data").Document(key);
            var snapshot = await docRef.GetAsync();

            if (snapshot != null )
            {
                var data = snapshot.fields;
                if (data.ContainsKey("value"))
                {
                    string value = data["value"].ToString();
                    string timestamp = data.ContainsKey("timestamp") ? data["timestamp"].ToString() : "Unknown";
                    
                    UpdateFirestoreStatus("Data loaded successfully");
                    UpdateLoadedData($"Key: {key}\nValue: {value}\nSaved: {timestamp}");
                }
                else
                {
                    UpdateFirestoreStatus("Data structure invalid");
                    UpdateLoadedData("Invalid data format");
                }
            }
            else
            {
                UpdateFirestoreStatus($"No data found for key: {key}");
                UpdateLoadedData("No data found");
            }
        }
        catch (System.Exception ex)
        {
            UpdateFirestoreStatus($"Load failed: {ex.Message}");
            UpdateLoadedData("Load error");
        }
    }

    public void DeleteData()
    {
        if (!isAuthenticated)
        {
            UpdateFirestoreStatus("Please login first");
            return;
        }

        string key = dataKeyField?.text ?? "";

        if (string.IsNullOrEmpty(key))
        {
            UpdateFirestoreStatus("Please enter a key to delete");
            return;
        }

        UpdateFirestoreStatus("Deleting data...");

        // In a real implementation, you would use Firestore here:
        // Firebase.Firestore.FirebaseFirestore.DefaultInstance
        //     .Collection("userdata").Document(key).DeleteAsync();

        // For demo purposes, remove from local dictionary
        if (localData.ContainsKey(key))
        {
            localData.Remove(key);
            UpdateFirestoreStatus($"Data deleted: {key}");
            UpdateLoadedData("Data deleted");
        }
        else
        {
            UpdateFirestoreStatus($"No data found to delete for key: {key}");
        }

        dataKeyField.text = "";
    }

    void UpdateAuthStatus(string message)
    {
        if (authStatusText != null)
            authStatusText.text = message;
        Debug.Log($"Auth Status: {message}");
    }

    void UpdateFirestoreStatus(string message)
    {
        if (firestoreStatusText != null)
            firestoreStatusText.text = message;
        Debug.Log($"Firestore Status: {message}");
    }

    void UpdateLoadedData(string data)
    {
        if (loadedDataText != null)
            loadedDataText.text = data;
    }

    void UpdateButtonStates()
    {
        if (loginButton != null)
            loginButton.interactable = !isAuthenticated;
        
        if (logoutButton != null)
            logoutButton.interactable = isAuthenticated;
        
        if (saveDataButton != null)
            saveDataButton.interactable = isAuthenticated;
        
        if (loadDataButton != null)
            loadDataButton.interactable = isAuthenticated;
        
        if (deleteDataButton != null)
            deleteDataButton.interactable = isAuthenticated;
    }
}
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using Firebase.Extensions;

/// <summary>
/// Firebase Initializer - Auth + Realtime Database فقط.
/// Firestore حُذف لأنه يستخدم gRPC يسبب bad_weak_ptr crash على iOS.
/// يبدأ تلقائياً قبل أي Scene.
/// </summary>
public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }

    public static FirebaseApp      App    { get; private set; }
    public static FirebaseAuth     Auth   { get; private set; }
    public static DatabaseReference DB    { get; private set; }
    public static bool             IsReady { get; private set; } = false;

    // ✅ Static strong refs لمنع iOS GC من حذف هذه الـ objects
    private static FirebaseApp         _appRef;
    private static FirebaseAuth        _authRef;
    private static DatabaseReference   _dbRef;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInit()
    {
        if (Instance != null || IsReady) return;
        var go = new GameObject("[FirebaseInitializer]");
        go.AddComponent<FirebaseInitializer>();
        Object.DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (!IsReady) BeginInit();
    }

    private static void BeginInit()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            try
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[Firebase] Init failed: " +
                        (task.Exception != null ? task.Exception.ToString() : "canceled"));
                    return;
                }
                if (task.Result != DependencyStatus.Available)
                {
                    Debug.LogError("[Firebase] Dependencies not available: " + task.Result);
                    return;
                }

                _appRef  = FirebaseApp.DefaultInstance;
                _authRef = FirebaseAuth.DefaultInstance;

                // ✅ نحدد الـ URL يدوياً لأنه غير موجود في GoogleService-Info.plist
                // الـ URL موجود في Firebase Console → Realtime Database
                const string dbUrl = "https://taleeq-348d6-default-rtdb.firebaseio.com";
                _dbRef = FirebaseDatabase.GetInstance(_appRef, dbUrl).RootReference;

                App   = _appRef;
                Auth  = _authRef;
                DB    = _dbRef;

                IsReady = true;
                Debug.Log("[Firebase] Initialized successfully (Auth + RealtimeDB).");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[Firebase] Exception: " + ex.Message);
            }
        });
    }
}

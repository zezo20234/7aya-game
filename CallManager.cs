using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class CallManager : MonoBehaviour
{
    public string currentUsername = "lilly";         // Set your current user
    public string currentPhone = "01681373331";      // Set current user's phone

    private DatabaseReference dbRef;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                ListenForCalls(currentUsername);
                Debug.Log("✅ Firebase Initialized");
            }
            else
            {
                Debug.LogError("❌ Firebase not available");
            }
        });
    }

    // Call someone by phone number
    public void CallNumber(string typedPhoneNumber)
    {
        FirebaseDatabase.DefaultInstance.GetReference("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.IsCompleted) return;

            DataSnapshot snapshot = task.Result;
            bool found = false;

            foreach (DataSnapshot userSnapshot in snapshot.Children)
            {
                string phone = userSnapshot.Child("phone").Value?.ToString();
                string username = userSnapshot.Key;

                if (phone == typedPhoneNumber)
                {
                    StartCall(username);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.Log("❌ Number not found");
            }
        });
    }

    // Start the call (update Firebase)
    private void StartCall(string targetUsername)
    {
        Debug.Log("📞 Calling " + targetUsername);

        dbRef.Child("users").Child(targetUsername).Child("incomingCall").SetRawJsonValueAsync(
            $"{{\"from\": \"{currentUsername}\", \"number\": \"{currentPhone}\"}}"
        );

        dbRef.Child("users").Child(currentUsername).Child("onCallWith").SetValueAsync(targetUsername);
    }

    // Listen for incoming calls
    private void ListenForCalls(string username)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(username)
            .Child("incomingCall")
            .ValueChanged += (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null) return;

            if (args.Snapshot.Exists && args.Snapshot.HasChild("from"))
            {
                string from = args.Snapshot.Child("from").Value.ToString();
                string number = args.Snapshot.Child("number").Value.ToString();

                Debug.Log($"📲 Incoming call from {number} (user: {from})");

                // TODO: Add popup or sound here
            }
        };
    }
}

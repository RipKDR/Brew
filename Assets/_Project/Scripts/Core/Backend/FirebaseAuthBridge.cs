using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Brew.Core.Backend
{
#if FIREBASE_AUTH
    using Firebase.Auth;

    public sealed class FirebaseAuthBridge
    {
        private readonly FirebaseAuthManager _authManager;
        private readonly FirebaseAuth _firebaseAuth;

        public FirebaseAuthBridge(FirebaseAuthManager authManager)
        {
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
            _firebaseAuth = FirebaseAuth.DefaultInstance;
            _firebaseAuth.StateChanged += OnFirebaseAuthStateChanged;
            RestoreExistingSession();
            Debug.Log("[FirebaseAuthBridge] Initialized.");
        }

        public async Task SignInAnonymouslyAsync()
        {
            try
            {
                var result = await _firebaseAuth.SignInAnonymouslyAsync();
                Debug.Log($"[FirebaseAuthBridge] Anonymous sign-in succeeded: {result.User.UserId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthBridge] Anonymous sign-in failed: {ex.Message}");
            }
        }

        public async Task LinkWithGoogleAsync(string idToken)
        {
            if (_firebaseAuth.CurrentUser == null)
            {
                Debug.LogWarning("[FirebaseAuthBridge] No current user to link.");
                return;
            }

            try
            {
                var credential = GoogleAuthProvider.GetCredential(idToken, null);
                var result = await _firebaseAuth.CurrentUser.LinkWithCredentialAsync(credential);
                _authManager.ApplyFirebaseAuthState(true, result.User.UserId, false);
                _authManager.LinkAccount("google.com");
                Debug.Log($"[FirebaseAuthBridge] Linked with Google: {result.User.UserId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthBridge] Google link failed: {ex.Message}");
            }
        }

        public void SignOut()
        {
            _firebaseAuth.SignOut();
        }

        private void OnFirebaseAuthStateChanged(object sender, EventArgs e)
        {
            var user = _firebaseAuth.CurrentUser;
            if (user != null)
                _authManager.ApplyFirebaseAuthState(true, user.UserId, user.IsAnonymous);
            else
                _authManager.ApplyFirebaseAuthState(false, null, false);
        }

        private void RestoreExistingSession()
        {
            var user = _firebaseAuth.CurrentUser;
            if (user != null)
            {
                _authManager.ApplyFirebaseAuthState(true, user.UserId, user.IsAnonymous);
                Debug.Log($"[FirebaseAuthBridge] Restored session for {user.UserId}");
            }
        }

        public void Dispose()
        {
            _firebaseAuth.StateChanged -= OnFirebaseAuthStateChanged;
        }
    }
#else
    public sealed class FirebaseAuthBridge
    {
        private readonly FirebaseAuthManager _authManager;

        public FirebaseAuthBridge(FirebaseAuthManager authManager)
        {
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
            Debug.LogWarning("[FirebaseAuthBridge] FIREBASE_AUTH not defined. Using stub.");
        }

        public Task SignInAnonymouslyAsync()
        {
            _authManager.SignInAnonymously();
            return Task.CompletedTask;
        }

        public Task LinkWithGoogleAsync(string idToken)
        {
            _authManager.LinkAccount("google.com");
            return Task.CompletedTask;
        }

        public void SignOut()
        {
            _authManager.SignOut();
        }

        public void Dispose() { }
    }
#endif
}

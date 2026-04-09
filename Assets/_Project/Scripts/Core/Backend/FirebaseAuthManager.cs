using System;

namespace Brew.Core.Backend
{
    /// <summary>
    /// Auth state abstraction. Pure C# — no Unity dependencies.
    /// Real Firebase Auth is bridged via <see cref="FirebaseAuthBridge"/>.
    /// </summary>
    public sealed class FirebaseAuthManager
    {
        private string _userId;

        public FirebaseAuthManager(string initialUserId = null)
        {
            if (!string.IsNullOrEmpty(initialUserId))
            {
                _userId = initialUserId;
                IsAuthenticated = true;
                IsAnonymous = true;
            }
        }

        public bool IsAuthenticated { get; private set; }

        public string UserId => _userId;

        public bool IsAnonymous { get; private set; }

        public event Action<bool, string> OnAuthStateChanged;

        public event Action<string> OnAccountLinked;

        public void SignInAnonymously()
        {
            _userId = Guid.NewGuid().ToString("N");
            IsAuthenticated = true;
            IsAnonymous = true;
            OnAuthStateChanged?.Invoke(true, _userId);
        }

        public void LinkAccount(string provider)
        {
            OnAccountLinked?.Invoke(provider ?? string.Empty);
        }

        public void SignOut()
        {
            _userId = null;
            IsAuthenticated = false;
            IsAnonymous = false;
            OnAuthStateChanged?.Invoke(false, string.Empty);
        }

        /// <summary>
        /// Applies auth snapshot from <see cref="FirebaseAuthBridge"/> (Firebase UID and flags).
        /// </summary>
        internal void ApplyFirebaseAuthState(bool isAuthenticated, string userId, bool isAnonymous)
        {
            if (isAuthenticated && string.IsNullOrEmpty(userId))
            {
                return;
            }

            _userId = isAuthenticated ? userId : null;
            IsAuthenticated = isAuthenticated;
            IsAnonymous = isAnonymous;
            OnAuthStateChanged?.Invoke(isAuthenticated, userId ?? string.Empty);
        }
    }
}

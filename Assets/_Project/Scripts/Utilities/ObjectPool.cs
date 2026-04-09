using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brew.Utilities
{
    /// <summary>
    /// Generic object pool for MonoBehaviour-based prefabs.
    /// Pre-warms on creation; returns objects to pool instead of Destroy.
    /// </summary>
    public sealed class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _available = new();
        private readonly HashSet<T> _active = new();

        public int CountActive => _active.Count;
        public int CountInactive => _available.Count;
        public int CountTotal => CountActive + CountInactive;

        public ObjectPool(T prefab, Transform parent, int preWarmCount)
        {
            _prefab = prefab ? prefab : throw new ArgumentNullException(nameof(prefab));
            _parent = parent;

            PreWarm(preWarmCount);
        }

        public T Get()
        {
            T instance;
            if (_available.Count > 0)
            {
                instance = _available.Pop();
            }
            else
            {
                instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            }

            instance.gameObject.SetActive(true);
            _active.Add(instance);
            return instance;
        }

        public void Return(T instance)
        {
            if (instance == null) return;

            instance.gameObject.SetActive(false);
            _active.Remove(instance);
            _available.Push(instance);
        }

        public void ReturnAll()
        {
            var snapshot = new List<T>(_active);
            foreach (var instance in snapshot)
                Return(instance);
        }

        private void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var instance = UnityEngine.Object.Instantiate(_prefab, _parent);
                instance.gameObject.SetActive(false);
                _available.Push(instance);
            }
        }
    }
}

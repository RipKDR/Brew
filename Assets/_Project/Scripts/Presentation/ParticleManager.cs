using System;
using Brew.Utilities;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Pooled particle VFX (max 5 concurrent systems). Uses <see cref="ObjectPool{T}"/> with a runtime template.
    /// Optional prefab slots are reserved for future art pass; all effects currently share the pooled template.
    /// </summary>
    public class ParticleManager : MonoBehaviour
    {
        private const int MaxSimultaneous = 5;
        private const int PreWarmCount = 5;

        [Header("Optional templates (future: assign per-effect art)")]
        [SerializeField] private ParticleSystem _fusionSparklesPrefab;
        [SerializeField] private ParticleSystem _cascadeTrailPrefab;
        [SerializeField] private ParticleSystem _brewBubblesPrefab;
        [SerializeField] private ParticleSystem _brewBurstPrefab;
        [SerializeField] private ParticleSystem _levelCompleteConfettiPrefab;
        [SerializeField] private ParticleSystem _chainIndicatorPrefab;

        private ObjectPool<ParticleSystem> _pool;
        private ParticleSystem _runtimeTemplate;

        private sealed class ParticleStopBridge : MonoBehaviour
        {
            public Action OnStopped;
            private bool _inStopMessage;

            private void OnParticleSystemStopped()
            {
                if (_inStopMessage) return;
                _inStopMessage = true;
                try
                {
                    Action cb = OnStopped;
                    OnStopped = null;
                    cb?.Invoke();
                }
                finally
                {
                    _inStopMessage = false;
                }
            }
        }

        private void Awake()
        {
            _runtimeTemplate = BuildRuntimeTemplate(transform);
            _pool = new ObjectPool<ParticleSystem>(_runtimeTemplate, transform, PreWarmCount);
        }

        public void PlayFusionSparkles(Vector3 position, Color color)
        {
            var ps = BorrowFromPool();
            if (ps == null) return;

            var main = ps.main;
            main.startLifetime = 0.3f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4.5f);
            main.startSize = 0.07f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 24;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Callback;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15, 20) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.12f;

            ConfigureSizeOverLifetime(ps, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
            ClearVelocityAndTrails(ps);

            ps.transform.position = position;
            ps.transform.rotation = Quaternion.identity;
            Play(ps);
        }

        public void PlayCascadeTrail(Vector3 from, Vector3 to, Color color)
        {
            var ps = BorrowFromPool();
            if (ps == null) return;

            float dist = Vector3.Distance(from, to);

            var main = ps.main;
            main.startLifetime = 0.2f;
            main.startSpeed = 0f;
            main.startSize = 0.055f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 16;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Callback;
            main.playOnAwake = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
            shape.radius = Mathf.Max(0.05f, dist * 0.5f);

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10, 14) });

            ConfigureSizeOverLifetime(ps, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
            ClearVelocityAndTrails(ps);

            Vector3 mid = (from + to) * 0.5f;
            ps.transform.position = mid;
            Vector3 dir = to - from;
            ps.transform.rotation = dir.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(Vector3.forward, dir.normalized)
                : Quaternion.identity;
            Play(ps);
        }

        public void PlayBrewBubbles(Vector3 position, Color color)
        {
            var ps = BorrowFromPool();
            if (ps == null) return;

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 28;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Callback;
            main.playOnAwake = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.18f;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12, 18) });

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.y = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);

            ConfigureSizeOverLifetime(ps, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var trails = ps.trails;
            trails.enabled = false;

            ps.transform.position = position;
            ps.transform.rotation = Quaternion.identity;
            Play(ps);
        }

        public void PlayBrewBurst(Vector3 position, Color color)
        {
            var ps = BorrowFromPool();
            if (ps == null) return;

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.09f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 36;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Callback;
            main.playOnAwake = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.04f;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0f, 20, 30),
                new ParticleSystem.Burst(0.02f, 8, 12)
            });

            ConfigureSizeOverLifetime(ps, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
            ClearVelocityAndTrails(ps);

            ps.transform.position = position;
            ps.transform.rotation = Quaternion.identity;
            Play(ps);
        }

        public void PlayLevelCompleteConfetti(int starCount)
        {
            var ps = BorrowFromPool();
            if (ps == null) return;

            int count = Mathf.Clamp(20 + starCount * 18, 20, 120);
            Vector3 spawn = ConfettiSpawnWorldPosition();
            float starT = Mathf.Clamp01(starCount / 3f);

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = count + 10;
            main.loop = false;
            main.gravityModifier = 0.55f;
            main.stopAction = ParticleSystemStopAction.Callback;
            main.playOnAwake = false;

            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.95f, 0.35f, 0.35f), 0f),
                    new GradientColorKey(new Color(0.35f, 0.55f, 0.95f), 0.25f),
                    new GradientColorKey(new Color(0.35f, 0.85f, 0.45f), 0.5f),
                    new GradientColorKey(new Color(0.95f, 0.8f, 0.25f), 0.75f),
                    new GradientColorKey(new Color(0.65f, 0.35f, 0.9f), 1f)
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            main.startColor = new ParticleSystem.MinMaxGradient(gradient);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(10f * Mathf.Lerp(0.85f, 1.15f, starT), 0.4f, 0f);

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            short burstCount = (short)Mathf.Min(count, short.MaxValue);
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, burstCount, burstCount) });

            ConfigureSizeOverLifetime(ps, AnimationCurve.Linear(0f, 1f, 1f, 0.25f));
            ClearVelocityAndTrails(ps);

            ps.transform.position = spawn;
            ps.transform.rotation = Quaternion.identity;
            Play(ps);
        }

        public void PlayChainIndicator(Vector3 from, Vector3 to, Color color)
        {
            var ps = BorrowFromPool();
            if (ps == null) return;

            float dist = Vector3.Distance(from, to);

            var main = ps.main;
            main.startLifetime = 0.15f;
            main.startSpeed = 0f;
            main.startSize = 0.035f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 10;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Callback;
            main.playOnAwake = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
            shape.radius = Mathf.Max(0.04f, dist * 0.5f);

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 6, 10) });

            ConfigureSizeOverLifetime(ps, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
            ClearVelocityAndTrails(ps);

            Vector3 mid = (from + to) * 0.5f;
            ps.transform.position = mid;
            Vector3 dir = to - from;
            ps.transform.rotation = dir.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(Vector3.forward, dir.normalized)
                : Quaternion.identity;
            Play(ps);
        }

        private ParticleSystem BorrowFromPool()
        {
            if (_pool.CountActive >= MaxSimultaneous)
                return null;

            ParticleSystem ps = _pool.Get();
            WireStopCallback(ps);
            return ps;
        }

        private void WireStopCallback(ParticleSystem ps)
        {
            var bridge = ps.GetComponent<ParticleStopBridge>();
            if (bridge == null)
                bridge = ps.gameObject.AddComponent<ParticleStopBridge>();

            bridge.OnStopped = () => ReturnToPool(ps);
        }

        private void Play(ParticleSystem ps)
        {
            ps.gameObject.SetActive(true);
            ps.Clear(true);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }

        private void ReturnToPool(ParticleSystem ps)
        {
            if (ps == null) return;

            if (ps.TryGetComponent(out ParticleStopBridge bridge))
                bridge.OnStopped = null;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            _pool.Return(ps);
        }

        private static ParticleSystem BuildRuntimeTemplate(Transform parent)
        {
            var go = new GameObject("ParticlePoolTemplate");
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Particles/Standard Unlit");
            if (shader != null)
                renderer.material = new Material(shader);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            var main = ps.main;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.Callback;

            return ps;
        }

        private static void ConfigureSizeOverLifetime(ParticleSystem ps, AnimationCurve curve)
        {
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        private static void ClearVelocityAndTrails(ParticleSystem ps)
        {
            var vel = ps.velocityOverLifetime;
            vel.enabled = false;
            var trails = ps.trails;
            trails.enabled = false;
        }

        private static Vector3 ConfettiSpawnWorldPosition()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return Vector3.up * 4f;

            if (cam.orthographic)
            {
                float y = cam.transform.position.y + cam.orthographicSize * 0.85f;
                return new Vector3(cam.transform.position.x, y, 0f);
            }

            return cam.ViewportToWorldPoint(new Vector3(0.5f, 0.95f, 8f));
        }
    }
}

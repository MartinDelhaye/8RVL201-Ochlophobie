using System.Collections;
using UnityEngine;

namespace Ochlophobia.Player
{
    /// Effet de malaise : paupières noires qui se ferment progressivement.
    /// Utilise des quads mesh (pas de Canvas) pour un rendu fiable en XR.
    /// Attacher au XR Origin (VR).
    public class MalaiseEffect : MonoBehaviour
    {
        [Header("Pré-signes")]
        [SerializeField] private float preSignInitialDelay  = 8f;
        [SerializeField] private float preSignStartInterval = 6f;
        [SerializeField] private float preSignMinInterval   = 1.5f;
        [SerializeField] private float preSignFlashDuration = 0.5f;
        [SerializeField] private float preSignMaxCoverage   = 0.35f;

        [Header("Effondrement")]
        [SerializeField] private float collapseDuration = 7f;

        [Header("Overlay")]
        [SerializeField] private float overlayDistance = 0.31f;
        [SerializeField] private float coverHalfWidth  = 1.2f;
        [SerializeField] private float coverHalfHeight = 0.8f;

        private Transform _topEyelid;
        private Transform _bottomEyelid;
        private Transform _cam;

        private bool _preSignsRunning = false;
        private bool _collapseRunning = false;

        private void Start()
        {
            _cam = Camera.main.transform;
            BuildOverlay();
        }

        // ── API publique ──────────────────────────────────────────────────────

        public void StartPreSigns()
        {
            if (_preSignsRunning || _collapseRunning) return;
            _preSignsRunning = true;
            StartCoroutine(PreSignsRoutine());
        }

        public void TriggerCollapse()
        {
            if (_collapseRunning) return;
            _collapseRunning = true;
            StopAllCoroutines();
            StartCoroutine(CollapseRoutine());
        }

        // ── Construction ──────────────────────────────────────────────────────

        private void BuildOverlay()
        {
            var anchor = new GameObject("EyelidAnchor");
            anchor.transform.SetParent(_cam, false);
            anchor.transform.localPosition = new Vector3(0f, 0f, overlayDistance);
            anchor.transform.localRotation = Quaternion.identity;

            _topEyelid    = MakeQuad("TopEyelid",    anchor.transform);
            _bottomEyelid = MakeQuad("BottomEyelid", anchor.transform);

            SetEyelidProgress(0f);
        }

        private Transform MakeQuad(string quadName, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = quadName;
            go.transform.SetParent(parent, false);

            Destroy(go.GetComponent<Collider>());

            // Cherche un shader unlit adapté au pipeline actif
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                      ?? Shader.Find("Unlit/Color")
                      ?? Shader.Find("Standard");

            var mat = new Material(shader) { color = Color.black };
            // Rendu en tout dernier, par-dessus tout
            mat.renderQueue = 5000;
            go.GetComponent<Renderer>().material = mat;

            return go.transform;
        }

        // ── Contrôle des paupières ────────────────────────────────────────────

        /// t = 0 → yeux ouverts   |   t = 1 → yeux fermés
        private void SetEyelidProgress(float t)
        {
            t = Mathf.Clamp01(t);
            float h = Mathf.Max(coverHalfHeight * t, 0.001f);
            float w = coverHalfWidth * 2f;

            // Paupière haute : bord supérieur fixe, bord inférieur descend vers le centre
            _topEyelid.localPosition    = new Vector3(0f,  coverHalfHeight - h * 0.5f, 0f);
            _topEyelid.localScale       = new Vector3(w, h, 1f);

            // Paupière basse : bord inférieur fixe, bord supérieur monte vers le centre
            _bottomEyelid.localPosition = new Vector3(0f, -coverHalfHeight + h * 0.5f, 0f);
            _bottomEyelid.localScale    = new Vector3(w, h, 1f);
        }

        private float GetCurrentProgress()
        {
            if (_topEyelid == null) return 0f;
            return Mathf.Clamp01(_topEyelid.localScale.y / coverHalfHeight);
        }

        // ── Coroutines ────────────────────────────────────────────────────────

        private IEnumerator PreSignsRoutine()
        {
            yield return new WaitForSeconds(preSignInitialDelay);

            float interval  = preSignStartInterval;
            float intensity = 0.08f;

            while (!_collapseRunning)
            {
                yield return new WaitForSeconds(interval);
                if (_collapseRunning) yield break;

                // Flash en cloche
                float elapsed = 0f;
                while (elapsed < preSignFlashDuration && !_collapseRunning)
                {
                    SetEyelidProgress(Mathf.Sin(elapsed / preSignFlashDuration * Mathf.PI) * intensity);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                SetEyelidProgress(0f);

                intensity = Mathf.Min(intensity + 0.07f, preSignMaxCoverage);
                interval  = Mathf.Max(interval  - 0.6f,  preSignMinInterval);
            }
        }

        private IEnumerator CollapseRoutine()
        {
            float start   = GetCurrentProgress();
            float elapsed = 0f;

            while (elapsed < collapseDuration)
            {
                float raw = Mathf.Lerp(start, 1f, elapsed / collapseDuration);
                SetEyelidProgress(raw * raw); // ease-in
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetEyelidProgress(1f);
        }
    }
}

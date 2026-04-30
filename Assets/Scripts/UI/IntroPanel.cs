using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;

namespace Ochlophobia.UI
{
    /// Panneau d'introduction world-space.
    /// - Disparaît automatiquement après displayDuration secondes (fondu).
    /// - Si le joueur le tient en main, le compte à rebours est suspendu.
    /// - Le bouton "J'ai compris" peut être touché avec la main pour fermer immédiatement.
    public class IntroPanel : MonoBehaviour
    {
        [SerializeField] private float distanceFromCamera = 2f;
        [SerializeField] private float heightOffset       = -0.1f;
        [SerializeField] private float displayDuration    = 9f;
        [SerializeField] private float fadeDuration       = 2f;

        private static readonly Color BgColor     = new Color(0.04f, 0.06f, 0.10f, 0.95f);
        private static readonly Color GoldColor   = new Color(0.87f, 0.72f, 0.35f, 1f);
        private static readonly Color DimWhite    = new Color(1f, 1f, 1f, 0.60f);
        private static readonly Color BtnColor    = new Color(0.87f, 0.72f, 0.35f, 1f);
        private static readonly Color BtnTxtColor = new Color(0.04f, 0.06f, 0.10f, 1f);

        private Transform   _cam;
        private CanvasGroup _canvasGroup;
        private bool        _grabbed   = false;
        private bool        _dismissed = false;

        private void Start()
        {
            var xrOrigin = FindFirstObjectByType<XROrigin>();
            _cam = xrOrigin != null ? xrOrigin.Camera.transform : Camera.main?.transform;

            PositionInFrontOfPlayer();
            BuildUI();
            SetupGrab();
            StartCoroutine(FadeOutRoutine());
        }

        private void Update()
        {
            if (_cam == null || _grabbed) return;
            Vector3 dir = transform.position - _cam.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        // ── Position ──────────────────────────────────────────────────────────

        private void PositionInFrontOfPlayer()
        {
            if (_cam == null) return;
            Vector3 forward = _cam.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();
            transform.position = _cam.position + forward * distanceFromCamera + Vector3.up * heightOffset;
            transform.rotation = Quaternion.LookRotation(forward);
        }

        // ── Grab ──────────────────────────────────────────────────────────────

        private void SetupGrab()
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity    = false;
            rb.isKinematic   = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var col    = gameObject.AddComponent<BoxCollider>();
            col.size   = new Vector3(100f, 70f, 2f);
            col.center = Vector3.zero;

            var grab = gameObject.AddComponent<
                UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grab.movementType     = UnityEngine.XR.Interaction.Toolkit.Interactables
                                        .XRBaseInteractable.MovementType.VelocityTracking;
            grab.throwOnDetach    = false;
            grab.useDynamicAttach = true;

            grab.selectEntered.AddListener(_ => _grabbed = true);
            grab.selectExited.AddListener(_  => _grabbed = false);
        }

        // ── Fondu automatique ─────────────────────────────────────────────────

        private IEnumerator FadeOutRoutine()
        {
            // Attend displayDuration, mais suspend le décompte pendant le grab
            float elapsed = 0f;
            while (elapsed < displayDuration)
            {
                if (!_grabbed) elapsed += Time.deltaTime;
                yield return null;
            }

            Dismiss();
        }

        public void Dismiss()
        {
            if (_dismissed) return;
            _dismissed = true;
            StopAllCoroutines();
            StartCoroutine(FadeAndDestroy());
        }

        private IEnumerator FadeAndDestroy()
        {
            float elapsed = 0f;
            float startAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;
            while (elapsed < fadeDuration)
            {
                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }

        // ── Construction UI ───────────────────────────────────────────────────

        private void BuildUI()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            gameObject.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
            gameObject.AddComponent<GraphicRaycaster>();

            _canvasGroup                = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;

            var rt = GetComponent<RectTransform>();
            rt.sizeDelta  = new Vector2(100f, 70f);
            rt.localScale = Vector3.one * 0.01f;

            // Fond
            MakeImage("BG", transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, BgColor);

            // Bande dorée haut
            MakeImage("TopBar", transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -5f), new Vector2(0f, 0f), GoldColor);

            // Barre dorée gauche
            MakeImage("AccentBar", transform,
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(0f, 5f), new Vector2(4f, -5f), GoldColor);

            // Titre  86 % → 97 %
            var title = AutoTMP("Title", transform,
                new Vector2(0.07f, 0.86f), new Vector2(0.97f, 0.97f),
                12f, FontStyles.Bold, GoldColor, TextAlignmentOptions.Left,
                TextWrappingModes.NoWrap);
            title.text = "GARE DE SÉOUL";

            // Sous-titre  76 % → 85 %
            var sub = AutoTMP("Subtitle", transform,
                new Vector2(0.07f, 0.76f), new Vector2(0.97f, 0.85f),
                6f, FontStyles.Normal, DimWhite, TextAlignmentOptions.Left,
                TextWrappingModes.NoWrap);
            sub.text = "Bienvenue dans la gare de Séoul";

            // Séparateur doré  74 %
            MakeImage("Divider", transform,
                new Vector2(0.07f, 0.74f), new Vector2(0.93f, 0.74f),
                new Vector2(0f, -0.5f), new Vector2(0f, 0.5f),
                new Color(0.87f, 0.72f, 0.35f, 0.5f));

            // VOTRE OBJECTIF  64 % → 73 %
            var objLabel = AutoTMP("ObjLabel", transform,
                new Vector2(0.07f, 0.64f), new Vector2(0.97f, 0.73f),
                5f, FontStyles.Bold, GoldColor, TextAlignmentOptions.Left,
                TextWrappingModes.NoWrap);
            objLabel.text             = "VOTRE OBJECTIF";
            objLabel.characterSpacing = 2f;

            // Corps  30 % → 62 %
            var body = AutoTMP("Body", transform,
                new Vector2(0.07f, 0.30f), new Vector2(0.97f, 0.62f),
                10f, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
                TextWrappingModes.Normal);
            body.text = "Rejoignez votre quai et\nmontez dans votre train.";

            // Séparateur bas  27 %
            MakeImage("DividerBot", transform,
                new Vector2(0.07f, 0.27f), new Vector2(0.93f, 0.27f),
                new Vector2(0f, -0.5f), new Vector2(0f, 0.5f),
                new Color(1f, 1f, 1f, 0.15f));

            // Bouton tactile  5 % → 22 %
            MakeTouchButton("BtnCompris", transform,
                new Vector2(0.22f, 0.05f), new Vector2(0.78f, 0.22f));
        }

        /// Bouton déclenché par contact main (XRBaseInteractor), pas par raycaster UI.
        private void MakeTouchButton(string goName, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            // Visuel
            var go  = new GameObject(goName);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = BtnColor;
            var r = img.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            var label = AutoTMP("Label", go.transform,
                new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f),
                6f, FontStyles.Bold, BtnTxtColor,
                TextAlignmentOptions.Center, TextWrappingModes.NoWrap);
            label.text = "J'ai compris";

            // Trigger de contact (séparé du BoxCollider parent)
            var triggerGO = new GameObject("BtnTrigger");
            triggerGO.transform.SetParent(go.transform, false);
            triggerGO.transform.localPosition = Vector3.zero;
            triggerGO.transform.localScale    = Vector3.one;

            var col    = triggerGO.AddComponent<BoxCollider>();
            col.isTrigger = true;
            // Taille en world space : le bouton fait ~56×12 unités canvas à scale 0.01 → 0.56×0.12 m
            col.size   = new Vector3(56f, 12f, 4f);
            col.center = Vector3.zero;

            triggerGO.AddComponent<PanelDismissTrigger>().Panel = this;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static TextMeshProUGUI AutoTMP(string goName, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            float maxSize, FontStyles style, Color color,
            TextAlignmentOptions alignment, TextWrappingModes wrap)
        {
            var go  = new GameObject(goName);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            var r   = tmp.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            tmp.enableAutoSizing = true;
            tmp.fontSizeMin      = 1f;
            tmp.fontSizeMax      = maxSize;
            tmp.fontStyle        = style;
            tmp.color            = color;
            tmp.alignment        = alignment;
            tmp.textWrappingMode = wrap;
            tmp.overflowMode     = TextOverflowModes.Truncate;
            return tmp;
        }

        private static Image MakeImage(string goName, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go  = new GameObject(goName);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            var r   = img.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = offsetMin;
            r.offsetMax = offsetMax;
            img.color   = color;
            return img;
        }

        // ── API publique ──────────────────────────────────────────────────────

        public void OnUnderstoodPressed()
        {
            Dismiss();
        }
    }

    /// Composant minimal sur le trigger du bouton.
    public class PanelDismissTrigger : MonoBehaviour
    {
        public IntroPanel Panel;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<
                UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>() != null)
                Panel?.Dismiss();
        }
    }
}

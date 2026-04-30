using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ochlophobia.UI
{
    /// Panneau d'introduction world-space affiché devant le joueur au lancement.
    /// Grabbable : le joueur peut le saisir et le déplacer librement.
    /// Billboard désactivé pendant le grab. Fondu + destruction si jamais lâché.
    public class IntroPanel : MonoBehaviour
    {
        [SerializeField] private float distanceFromPlayer = 2.5f;
        [SerializeField] private float displayDuration    = 9f;
        [SerializeField] private float fadeDuration       = 2f;

        private static readonly Color BackgroundColor = new Color(0.04f, 0.06f, 0.10f, 0.92f);
        private static readonly Color GoldColor       = new Color(0.87f, 0.72f, 0.35f, 1f);
        private static readonly Color DimWhite        = new Color(1f, 1f, 1f, 0.55f);

        private CanvasGroup _canvasGroup;
        private Transform   _cam;
        private bool        _grabbed = false;

        private void Start()
        {
            _cam = Camera.main.transform;

            Vector3 forward = _cam.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            transform.position = _cam.position + forward * distanceFromPlayer;
            transform.rotation = Quaternion.LookRotation(forward);

            BuildUI();
            SetupGrab();
            StartCoroutine(FadeOutRoutine());
        }

        private void Update()
        {
            if (_cam == null || _grabbed) return;

            // Billboard : tourne vers la caméra (axe Y uniquement)
            Vector3 dir = transform.position - _cam.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        // ── Grab physique ─────────────────────────────────────────────────────

        private void SetupGrab()
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity    = false;
            rb.isKinematic   = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Le canvas fait 80×50 unités à scale 0.01 → 0.8×0.5 m physique
            var col    = gameObject.AddComponent<BoxCollider>();
            col.size   = new Vector3(80f, 50f, 2f);
            col.center = Vector3.zero;

            var grab = gameObject.AddComponent<
                UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grab.movementType     = UnityEngine.XR.Interaction.Toolkit.Interactables
                                        .XRBaseInteractable.MovementType.VelocityTracking;
            grab.throwOnDetach    = false;
            grab.useDynamicAttach = true;
            grab.trackRotation    = true;

            grab.selectEntered.AddListener(_ => OnGrabbed());
            grab.selectExited.AddListener(_  => OnReleased());
        }

        private void OnGrabbed()
        {
            _grabbed = true;
            StopAllCoroutines();             // annule le fondu automatique
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;    // s'assure que le panneau est visible
        }

        private void OnReleased()
        {
            _grabbed = false;
            StartCoroutine(FadeOutRoutine()); // relance le compte à rebours
        }

        // ── Construction UI ───────────────────────────────────────────────────

        private void BuildUI()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            gameObject.AddComponent<GraphicRaycaster>();

            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;

            // Panneau 80 cm × 50 cm
            var rt = GetComponent<RectTransform>();
            rt.sizeDelta  = new Vector2(80f, 50f);
            rt.localScale = Vector3.one * 0.01f;

            MakeImage("BG", transform,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero,
                BackgroundColor);

            MakeImage("AccentBar", transform,
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(0f, 0f), new Vector2(5f, 0f),
                GoldColor);

            var title = MakeTMP("Title", transform,
                new Vector2(0.1f, 0.76f), new Vector2(0.95f, 0.96f));
            title.text      = "GARE DE SÉOUL";
            title.fontSize  = 9f;
            title.fontStyle = FontStyles.Bold;
            title.color     = GoldColor;
            title.alignment = TextAlignmentOptions.Left;

            var sub = MakeTMP("Subtitle", transform,
                new Vector2(0.1f, 0.63f), new Vector2(0.95f, 0.76f));
            sub.text      = "Bienvenue dans la gare";
            sub.fontSize  = 5f;
            sub.color     = DimWhite;
            sub.alignment = TextAlignmentOptions.Left;

            MakeImage("Divider", transform,
                new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.60f),
                new Vector2(0f, -0.5f), new Vector2(0f, 0.5f),
                new Color(1f, 1f, 1f, 0.2f));

            var objLabel = MakeTMP("ObjLabel", transform,
                new Vector2(0.1f, 0.48f), new Vector2(0.95f, 0.60f));
            objLabel.text             = "VOTRE OBJECTIF";
            objLabel.fontSize         = 4.5f;
            objLabel.fontStyle        = FontStyles.Bold;
            objLabel.color            = GoldColor;
            objLabel.alignment        = TextAlignmentOptions.Left;
            objLabel.characterSpacing = 2f;

            var body = MakeTMP("Body", transform,
                new Vector2(0.1f, 0.08f), new Vector2(0.95f, 0.46f));
            body.text               = "Rejoignez votre quai\net montez dans votre train.";
            body.fontSize           = 7f;
            body.color              = Color.white;
            body.alignment          = TextAlignmentOptions.Left;
            body.enableWordWrapping = true;
        }

        private static Image MakeImage(string goName, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax,
            Color color)
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

        private static TextMeshProUGUI MakeTMP(string goName, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go  = new GameObject(goName);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            var r   = tmp.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            tmp.enableWordWrapping = false;
            return tmp;
        }

        // ── Fondu + destruction ───────────────────────────────────────────────

        private IEnumerator FadeOutRoutine()
        {
            yield return new WaitForSeconds(displayDuration);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}

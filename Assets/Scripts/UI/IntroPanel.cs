using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ochlophobia.UI
{
    /// Panneau d'introduction world-space affiché devant le joueur au lancement.
    /// Se tourne en permanence vers la caméra (billboard), puis disparaît en fondu.
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
            StartCoroutine(FadeOutRoutine());
        }

        private void Update()
        {
            if (_cam == null) return;

            // Billboard : tourne vers la caméra (axe Y uniquement)
            Vector3 dir = transform.position - _cam.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

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

            // Fond principal
            MakeImage("BG", transform,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero,
                BackgroundColor);

            // Barre dorée gauche
            MakeImage("AccentBar", transform,
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(0f, 0f), new Vector2(5f, 0f),
                GoldColor);

            // Titre  — zone : 76 % → 95 %
            var title = MakeTMP("Title", transform,
                new Vector2(0.1f, 0.76f), new Vector2(0.95f, 0.96f));
            title.text      = "GARE DE SÉOUL";
            title.fontSize  = 9f;
            title.fontStyle = FontStyles.Bold;
            title.color     = GoldColor;
            title.alignment = TextAlignmentOptions.Left;

            // Sous-titre — zone : 63 % → 76 %
            var sub = MakeTMP("Subtitle", transform,
                new Vector2(0.1f, 0.63f), new Vector2(0.95f, 0.76f));
            sub.text      = "Bienvenue dans la gare";
            sub.fontSize  = 5f;
            sub.color     = DimWhite;
            sub.alignment = TextAlignmentOptions.Left;

            // Séparateur — à 61 %
            MakeImage("Divider", transform,
                new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.60f),
                new Vector2(0f, -0.5f), new Vector2(0f, 0.5f),
                new Color(1f, 1f, 1f, 0.2f));

            // Label objectif — zone : 48 % → 60 %
            var objLabel = MakeTMP("ObjLabel", transform,
                new Vector2(0.1f, 0.48f), new Vector2(0.95f, 0.60f));
            objLabel.text             = "VOTRE OBJECTIF";
            objLabel.fontSize         = 4.5f;
            objLabel.fontStyle        = FontStyles.Bold;
            objLabel.color            = GoldColor;
            objLabel.alignment        = TextAlignmentOptions.Left;
            objLabel.characterSpacing = 2f;

            // Corps — zone : 08 % → 46 %
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

        private IEnumerator FadeOutRoutine()
        {
            yield return new WaitForSeconds(displayDuration);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}

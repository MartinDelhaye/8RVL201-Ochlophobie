using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ochlophobia.UI
{
    /// Panneau d'introduction affiché en world-space devant le joueur au lancement.
    /// S'auto-détruit après fadeDelay + fadeDuration secondes.
    public class IntroPanel : MonoBehaviour
    {
        [SerializeField] private float distanceFromPlayer = 2.5f;
        [SerializeField] private float displayDuration    = 8f;
        [SerializeField] private float fadeDuration       = 1.5f;

        [TextArea(4, 10)]
        [SerializeField] private string message =
            "Bienvenue à la gare de Séoul.\n\n" +
            "Votre objectif :\n" +
            "Rejoignez votre quai\net montez dans votre train.";

        private CanvasGroup _canvasGroup;

        private void Start()
        {
            Transform cam = Camera.main.transform;

            Vector3 forward = cam.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = cam.up;
            forward.Normalize();

            transform.position = cam.position + forward * distanceFromPlayer;
            transform.rotation = Quaternion.LookRotation(forward);

            BuildUI();
            StartCoroutine(FadeOutRoutine());
        }

        private void BuildUI()
        {
            // Canvas world-space
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            gameObject.AddComponent<GraphicRaycaster>();

            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            // Taille du panneau : 60 cm × 40 cm
            var rt = GetComponent<RectTransform>();
            rt.sizeDelta  = new Vector2(60f, 40f);
            rt.localScale = Vector3.one * 0.01f;

            // Fond semi-transparent
            var bg = new GameObject("BG").AddComponent<Image>();
            bg.transform.SetParent(transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            bg.color = new Color(0f, 0f, 0f, 0.78f);

            // Texte TMP
            var textGO = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            textGO.transform.SetParent(transform, false);
            var textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0.08f, 0.08f);
            textRt.anchorMax = new Vector2(0.92f, 0.92f);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            textGO.text          = message;
            textGO.fontSize      = 8f;
            textGO.alignment     = TextAlignmentOptions.Center;
            textGO.color         = Color.white;
            textGO.enableWordWrapping = true;
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

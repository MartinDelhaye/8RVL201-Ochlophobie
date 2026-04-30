using System.Collections;
using UnityEngine;

namespace Ochlophobia.Player
{
    /// Gère la montée progressive de la charge auditive :
    ///   - Tik-taks de l'horloge qui s'accélèrent
    ///   - Ambiance foule qui monte en volume
    ///   - Acouphène procédural qui apparaît lors du collapse
    ///
    /// Attacher au XR Origin. Assigner les clips et le Transform du quai dans l'Inspector.
    public class AudioMalaiseController : MonoBehaviour
    {
        [Header("Clips audio")]
        [SerializeField] private AudioClip clockTickClip;
        [SerializeField] private AudioClip crowdClip;

        [Header("Horloge (optionnel)")]
        [Tooltip("Si assigné, synchronise aussi la vitesse visuelle de l'aiguille")]
        [SerializeField] private ClockSample.Clock clockScript;

        [Header("Zone cible")]
        [Tooltip("Transform du quai — l'intensité augmente en s'en approchant")]
        [SerializeField] private Transform quayTransform;
        [SerializeField] private float maxDistance = 30f;

        [Header("Tik-taks")]
        [SerializeField] private float tickIntervalFar  = 1.4f;
        [SerializeField] private float tickIntervalNear = 0.12f;
        [SerializeField] private float tickVolume       = 0.55f;

        [Header("Foule")]
        [SerializeField] private float crowdVolumeMin = 0.25f;
        [SerializeField] private float crowdVolumeMax = 1.0f;

        [Header("Acouphène")]
        [SerializeField] private float tinnitusFreq        = 4200f;
        [SerializeField] private float tinnitusMaxVolume   = 0.85f;
        [SerializeField] private float tinnitusFadeDuration = 4f;

        private AudioSource _clockSource;
        private AudioSource _crowdSource;
        private AudioSource _tinnitusSource;

        private float _intensity      = 0f;
        private bool  _collapseActive = false;

        // ── Init ──────────────────────────────────────────────────────────────

        private void Start()
        {
            _clockSource    = CreateSource(false, false, tickVolume);
            _crowdSource    = CreateSource(true,  true,  crowdVolumeMin);
            _tinnitusSource = CreateSource(true,  true,  0f);

            if (crowdClip != null)
            {
                _crowdSource.clip = crowdClip;
                _crowdSource.Play();
            }

            _tinnitusSource.clip = GenerateTinnitusClip();
            _tinnitusSource.Play();

            StartCoroutine(ClockTickRoutine());
        }

        private AudioSource CreateSource(bool loop, bool spatialize2D, float volume)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.loop         = loop;
            src.spatialBlend = spatialize2D ? 0f : 0f;
            src.volume       = volume;
            src.playOnAwake  = false;
            return src;
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_collapseActive || quayTransform == null) return;

            float dist = Vector3.Distance(transform.position, quayTransform.position);
            _intensity = 1f - Mathf.Clamp01(dist / maxDistance);

            _crowdSource.volume = Mathf.Lerp(crowdVolumeMin, crowdVolumeMax, _intensity);
        }

        // ── API publique ──────────────────────────────────────────────────────

        /// Déclenche le fondu de l'acouphène et coupe progressivement le reste.
        public void TriggerTinnitus()
        {
            if (_collapseActive) return;
            _collapseActive = true;
            StartCoroutine(TinnitusCollapseRoutine());
        }

        // ── Tik-tak ───────────────────────────────────────────────────────────

        private IEnumerator ClockTickRoutine()
        {
            while (true)
            {
                if (clockTickClip != null)
                    _clockSource.PlayOneShot(clockTickClip, tickVolume);

                float interval = Mathf.Lerp(tickIntervalFar, tickIntervalNear, _intensity);

                // Synchronise aussi l'aiguille de l'horloge si référencée
                if (clockScript != null)
                    clockScript.tickInterval = interval;

                yield return new WaitForSeconds(Mathf.Max(interval, 0.05f));
            }
        }

        // ── Acouphène + collapse ──────────────────────────────────────────────

        private IEnumerator TinnitusCollapseRoutine()
        {
            float startCrowd = _crowdSource.volume;
            float elapsed    = 0f;

            while (elapsed < tinnitusFadeDuration)
            {
                float t = elapsed / tinnitusFadeDuration;

                _tinnitusSource.volume = Mathf.Lerp(0f, tinnitusMaxVolume, t);
                _crowdSource.volume    = Mathf.Lerp(startCrowd, 0f, t);
                _clockSource.volume    = Mathf.Lerp(tickVolume,  0f, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            _tinnitusSource.volume = tinnitusMaxVolume;
            _crowdSource.volume    = 0f;
            _clockSource.volume    = 0f;
        }

        // ── Génération procédurale de l'acouphène ────────────────────────────

        private AudioClip GenerateTinnitusClip()
        {
            int   sampleRate = 44100;
            int   samples    = sampleRate * 2; // boucle de 2 s
            float[] data     = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Trois harmoniques pour un son plus organique
                data[i] = 0.60f * Mathf.Sin(2f * Mathf.PI * tinnitusFreq         * t)
                         + 0.25f * Mathf.Sin(2f * Mathf.PI * tinnitusFreq * 1.5f  * t)
                         + 0.15f * Mathf.Sin(2f * Mathf.PI * tinnitusFreq * 2.01f * t);
            }

            // Normalisation
            float peak = 0f;
            foreach (float s in data) peak = Mathf.Max(peak, Mathf.Abs(s));
            if (peak > 0f)
                for (int i = 0; i < data.Length; i++) data[i] /= peak;

            var clip = AudioClip.Create("Tinnitus", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}

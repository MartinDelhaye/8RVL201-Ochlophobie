using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Ochlophobia;
using Ochlophobia.UI;
using Ochlophobia.Environment;

/// <summary>
/// Menu Ochlophobia > Setup Scene
/// Crée GameManager, IntroPanel et TimerHUD dans la scène active.
/// Idempotent : relancer ne duplique pas les objets existants.
/// </summary>
public static class OchlophobiaSetup
{
    [MenuItem("Ochlophobia/Setup Scene (Intro + Timer)")]
    static void SetupScene()
    {
        SetupGameManager();
        SetupIntroPanel();
        SetupTimerHUD();
        SetupTrainTrigger();

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Ochlophobia — Setup terminé",
            "4 objets configurés dans la scène :\n\n" +
            "  • GameManager\n" +
            "  • IntroPanel      (canvas world-space)\n" +
            "  • TimerHUD        (canvas world-space)\n" +
            "  • TrainTrigger    (zone au quai Z≈22)\n\n" +
            "Dernière étape :\n" +
            "  Tagger le XR Origin avec le tag « Player »",
            "OK");
    }

    // ── GameManager ──────────────────────────────────────────────────────────

    static void SetupGameManager()
    {
        if (Object.FindObjectOfType<GameManager>() != null) return;

        var go = new GameObject("GameManager");
        Undo.RegisterCreatedObjectUndo(go, "Create GameManager");
        go.AddComponent<GameManager>();
    }

    // ── IntroPanel ───────────────────────────────────────────────────────────

    static void SetupIntroPanel()
    {
        if (GameObject.Find("IntroPanel") != null) return;

        // Canvas world-space
        var root = new GameObject("IntroPanel");
        Undo.RegisterCreatedObjectUndo(root, "Create IntroPanel");
        root.transform.SetPositionAndRotation(new Vector3(0, 1.5f, 2f), Quaternion.identity);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 500);
        root.transform.localScale = Vector3.one * 0.002f;

        root.AddComponent<TrackedDeviceGraphicRaycaster>();
        root.AddComponent<CanvasGroup>();

        var intro = root.AddComponent<IntroPanel>();

        // ── Fond semi-transparent ─────────────────────────────────────────
        var bg = CreateRect("Background", root.transform);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.06f, 0.18f, 0.93f);
        Stretch(bg.GetComponent<RectTransform>());

        // ── Titre ─────────────────────────────────────────────────────────
        var title = CreateRect("Title", bg.transform);
        var titleTmp = title.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Votre train part dans 1 minute !";
        titleTmp.fontSize = 54;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = Color.white;
        titleTmp.alignment = TextAlignmentOptions.Center;
        SetAnchors(title.GetComponent<RectTransform>(),
            new Vector2(0.05f, 0.66f), new Vector2(0.95f, 0.93f));

        // ── Icône train ───────────────────────────────────────────────────
        var icon = CreateRect("TrainIcon", bg.transform);
        var iconTmp = icon.AddComponent<TextMeshProUGUI>();
        iconTmp.text = "🚆";
        iconTmp.fontSize = 60;
        iconTmp.alignment = TextAlignmentOptions.Center;
        SetAnchors(icon.GetComponent<RectTransform>(),
            new Vector2(0.38f, 0.40f), new Vector2(0.62f, 0.65f));

        // ── Description ───────────────────────────────────────────────────
        var desc = CreateRect("Description", bg.transform);
        var descTmp = desc.AddComponent<TextMeshProUGUI>();
        descTmp.text = "Traversez la foule et rejoignez le quai\navant que le train ne parte sans vous.";
        descTmp.fontSize = 34;
        descTmp.color = new Color(0.82f, 0.82f, 0.82f, 1f);
        descTmp.alignment = TextAlignmentOptions.Center;
        SetAnchors(desc.GetComponent<RectTransform>(),
            new Vector2(0.05f, 0.26f), new Vector2(0.95f, 0.50f));

        // ── Bouton "J'ai compris" ─────────────────────────────────────────
        var btnGO = CreateRect("UnderstoodButton", bg.transform);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.55f, 1f, 1f);

        var btn = btnGO.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.30f, 0.65f, 1f, 1f);
        colors.pressedColor     = new Color(0.10f, 0.40f, 0.85f, 1f);
        btn.colors = colors;

        SetAnchors(btnGO.GetComponent<RectTransform>(),
            new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.24f));

        var btnTextGO = CreateRect("Label", btnGO.transform);
        var btnTmp = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "J'ai compris";
        btnTmp.fontSize = 38;
        btnTmp.fontStyle = FontStyles.Bold;
        btnTmp.color = Color.white;
        btnTmp.alignment = TextAlignmentOptions.Center;
        Stretch(btnTextGO.GetComponent<RectTransform>());

        // Câble le bouton → IntroPanel.OnUnderstoodPressed
        UnityEventTools.AddPersistentListener(btn.onClick, intro.OnUnderstoodPressed);
    }

    // ── TimerHUD ──────────────────────────────────────────────────────────────

    static void SetupTimerHUD()
    {
        if (GameObject.Find("TimerHUD") != null) return;

        var root = new GameObject("TimerHUD");
        Undo.RegisterCreatedObjectUndo(root, "Create TimerHUD");
        root.transform.SetPositionAndRotation(new Vector3(0.7f, 1.9f, 2f), Quaternion.identity);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, 90);
        root.transform.localScale = Vector3.one * 0.002f;

        root.AddComponent<TrackedDeviceGraphicRaycaster>();
        root.AddComponent<CanvasGroup>();

        // Fond sombre
        var bg = CreateRect("Background", root.transform);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.72f);
        Stretch(bg.GetComponent<RectTransform>());

        // Texte du timer
        var labelGO = CreateRect("TimerLabel", bg.transform);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "1:00";
        tmp.fontSize = 62;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        Stretch(labelGO.GetComponent<RectTransform>());

        // TimerHUD script + assigne la référence au label
        var hud = root.AddComponent<TimerHUD>();
        var so = new SerializedObject(hud);
        so.FindProperty("timerLabel").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();
    }

    // ── TrainTrigger ──────────────────────────────────────────────────────────

    static void SetupTrainTrigger()
    {
        // Cherche l'objet vide déjà créé par l'utilisateur, sinon en crée un nouveau
        var go = GameObject.Find("TrainTrigger");
        bool alreadyExisted = go != null;

        if (!alreadyExisted)
        {
            go = new GameObject("TrainTrigger");
            Undo.RegisterCreatedObjectUndo(go, "Create TrainTrigger");
        }

        // Position déduite des waypoints du quai (Z≈23, X≈-7.2, Y sol≈0.37)
        // On centre le collider à mi-hauteur du joueur (Y=1.5) à l'entrée du quai (Z=21.5)
        go.transform.position = new Vector3(-7.2f, 1.5f, 21.5f);

        // BoxCollider
        var col = go.GetComponent<BoxCollider>();
        if (col == null) col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = new Vector3(5f, 3f, 4f);  // largeur / hauteur / profondeur
        col.center    = Vector3.zero;

        // Script TrainTrigger
        if (go.GetComponent<TrainTrigger>() == null)
            go.AddComponent<TrainTrigger>();

        if (!alreadyExisted)
            Debug.Log("[OchlophobiaSetup] TrainTrigger créé à la position du quai.");
        else
            Debug.Log("[OchlophobiaSetup] Objet TrainTrigger existant configuré (BoxCollider + script).");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ── AutoDoor sur toutes les portes ────────────────────────────────────────

    [MenuItem("Ochlophobia/Appliquer AutoDoor à toutes les portes")]
    static void ApplyAutoDoorToAllDoors()
    {
        int added   = 0;
        int skipped = 0;

        // Cherche tous les MeshRenderer dans la scène (y compris inactifs)
        var renderers = Object.FindObjectsOfType<MeshRenderer>(true);

        foreach (var mr in renderers)
        {
            var go = mr.gameObject;

            if (!IsDoorObject(go)) continue;

            if (go.GetComponent<AutoDoor>() != null)
            {
                skipped++;
                continue;
            }

            Undo.RecordObject(go, "Add AutoDoor");
            go.AddComponent<AutoDoor>();
            EditorUtility.SetDirty(go);
            added++;
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[OchlophobiaSetup] AutoDoor : {added} portes configurées, {skipped} déjà faites.");
        EditorUtility.DisplayDialog(
            "AutoDoor — terminé",
            $"{added} portes configurées.\n{skipped} portes déjà équipées.\n\n" +
            "Vérifie que l'angle openAngle est correct pour chaque porte " +
            "(90 ou -90 selon le sens d'ouverture).",
            "OK");
    }

    /// Retourne true si le GameObject est une porte du pack Seoul Station.
    static bool IsDoorObject(GameObject go)
    {
        // Nom direct
        if (go.name.IndexOf("door", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        // Nom du prefab source
        var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
        if (source != null &&
            source.name.IndexOf("door", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        return false;
    }
}

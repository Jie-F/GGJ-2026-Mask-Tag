using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


public class RubbleBakeWindow : EditorWindow
{
    Transform root;
    bool disableRigidbodies = true;
    bool disableColliders = false;
    bool removeRigidbodies = true;

    [MenuItem("Tools/Rubble/Bake Physics To Transforms")]
    static void Open() => GetWindow<RubbleBakeWindow>("Rubble Bake");

    void OnGUI()
    {
        root = (Transform)EditorGUILayout.ObjectField("Rubble Root", root, typeof(Transform), true);

        EditorGUILayout.Space();
        disableRigidbodies = EditorGUILayout.Toggle("Disable Rigidbodies", disableRigidbodies);
        removeRigidbodies = EditorGUILayout.Toggle("Remove Rigidbodies", removeRigidbodies);
        disableColliders = EditorGUILayout.Toggle("Disable Colliders", disableColliders);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(root == null))
        {
            if (GUILayout.Button("Bake Now (Record + Freeze)"))
                Bake(root, disableRigidbodies, removeRigidbodies, disableColliders);
        }

        EditorGUILayout.HelpBox(
            "Tip: Enter Play Mode, let rubble settle, pause, then click Bake. " +
            "If you exit Play Mode without baking, Unity will revert transforms unless you bake.",
            MessageType.Info
        );
    }

    static void Bake(Transform root, bool disableRB, bool removeRB, bool disableCols)
    {
        if (!root) return;

        // --- If we're in Play Mode, we can't mark the scene dirty.
        // Instead: capture snapshot now, apply after exiting Play Mode.
        if (Application.isPlaying)
        {
            var pb = new PendingBake
            {
                rootFullPath = GetFullPath(root),
                disableRB = disableRB,
                removeRB = removeRB,
                disableCols = disableCols,
            };

            // Capture all child transforms (excluding root itself)
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t == root) continue;

                pb.items.Add(new BakeItem
                {
                    relPath = GetRelativePath(t, root),
                    pos = t.position,
                    rot = t.rotation,
                    scale = t.localScale,
                });
            }

            _pending = pb;
            EnsurePlaymodeHook();

            // Optional: log so you know it worked
            Debug.Log($"Rubble bake captured ({pb.items.Count} items). Stop Play Mode to apply to the scene.");
            return;
        }

        // --- Edit Mode: apply immediately and mark dirty (allowed)
        ApplyPendingBake(new PendingBake
        {
            rootFullPath = GetFullPath(root),
            items = CaptureItemsNow(root),
            disableRB = disableRB,
            removeRB = removeRB,
            disableCols = disableCols,
        });
    }

    static List<BakeItem> CaptureItemsNow(Transform root)
    {
        var items = new List<BakeItem>();
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t == root) continue;

            items.Add(new BakeItem
            {
                relPath = GetRelativePath(t, root),
                pos = t.position,
                rot = t.rotation,
                scale = t.localScale,
            });
        }
        return items;
    }

    static void ApplyPendingBake(PendingBake pb)
    {
        var rootEdit = FindByFullPath(pb.rootFullPath);
        if (!rootEdit)
        {
            Debug.LogWarning($"Rubble bake apply failed: couldn't find root '{pb.rootFullPath}' in scene.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(rootEdit.gameObject, "Bake Rubble");

        // Apply transforms
        foreach (var item in pb.items)
        {
            var t = string.IsNullOrEmpty(item.relPath) ? rootEdit : rootEdit.Find(item.relPath);
            if (!t) continue;

            Undo.RecordObject(t, "Bake Rubble Transform");
            t.position = item.pos;
            t.rotation = item.rot;
            t.localScale = item.scale;
        }

        // Freeze physics components on the EDIT-MODE objects
        var rbs = rootEdit.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            if (pb.removeRB)
            {
                Undo.DestroyObjectImmediate(rb);
            }
            else if (pb.disableRB)
            {
                Undo.RecordObject(rb, "Disable Rigidbody");
                rb.isKinematic = true;
            }
        }

        if (pb.disableCols)
        {
            var cols = rootEdit.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols)
            {
                Undo.RecordObject(c, "Disable Collider");
                c.enabled = false;
            }
        }

        // Now we're in Edit Mode, so marking dirty is legal
        EditorUtility.SetDirty(rootEdit);
        EditorSceneManager.MarkSceneDirty(rootEdit.gameObject.scene);

        Debug.Log("Rubble bake applied to scene. Save the scene to persist.");
    }







    [Serializable]
    class BakeItem
    {
        public string relPath;     // path relative to rubble root
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
    }

    [Serializable]
    class PendingBake
    {
        public string rootFullPath; // full scene path to the root object
        public List<BakeItem> items = new List<BakeItem>();

        public bool disableRB;
        public bool removeRB;
        public bool disableCols;
    }

    static PendingBake _pending;   // kept while in play mode
    static bool _hooked;

    // Full path from scene root, e.g. "World/RubbleRoot/Chunk_01"
    static string GetFullPath(Transform t)
    {
        var path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    // Path relative to a given root, e.g. "Chunk_01/SubChunk"
    static string GetRelativePath(Transform t, Transform root)
    {
        if (t == root) return "";
        var parts = new List<string>();
        while (t != null && t != root)
        {
            parts.Add(t.name);
            t = t.parent;
        }
        parts.Reverse();
        return string.Join("/", parts);
    }

    // Find a transform in the active scene by full path
    static Transform FindByFullPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return null;

        var parts = fullPath.Split('/');
        if (parts.Length == 0) return null;

        var go = GameObject.Find(parts[0]);
        if (!go) return null;

        Transform t = go.transform;
        for (int i = 1; i < parts.Length; i++)
        {
            t = t.Find(parts[i]);
            if (!t) return null;
        }
        return t;
    }

    static void EnsurePlaymodeHook()
    {
        if (_hooked) return;
        _hooked = true;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // We only apply once we've returned to Edit Mode
        if (state != PlayModeStateChange.EnteredEditMode) return;
        if (_pending == null) return;

        ApplyPendingBake(_pending);
        _pending = null;
    }

}

using UnityEngine;

public class UnitFogVisibility : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private Renderer[] renderersToControl;

    [Header("Player Click Colliders")]
    [SerializeField] private Collider[] collidersToControl;

    [Header("Overhead UI")]
    [SerializeField] private CanvasGroup overheadCanvasGroup;

    [Header("Options")]
    [SerializeField] private bool collectOnAwake = true;
    [SerializeField] private bool disableCollidersWhenHidden = true;

    public bool IsVisibleToPlayer { get; private set; } = true;

    private void Awake()
    {
        if (collectOnAwake)
            CollectReferences();
    }

    private void CollectReferences()
    {
        if (renderersToControl == null || renderersToControl.Length == 0)
            renderersToControl = GetComponentsInChildren<Renderer>(true);

        if (collidersToControl == null || collidersToControl.Length == 0)
            collidersToControl = GetComponentsInChildren<Collider>(true);

        if (overheadCanvasGroup == null)
            overheadCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
    }

    public void SetVisibleToPlayer(bool visible)
    {
        IsVisibleToPlayer = visible;

        if (renderersToControl != null)
        {
            foreach (Renderer r in renderersToControl)
            {
                if (r != null)
                    r.enabled = visible;
            }
        }

        if (disableCollidersWhenHidden && collidersToControl != null)
        {
            foreach (Collider c in collidersToControl)
            {
                if (c != null)
                    c.enabled = visible;
            }
        }

        if (overheadCanvasGroup != null)
        {
            overheadCanvasGroup.alpha = visible ? 1f : 0f;
            overheadCanvasGroup.interactable = false;
            overheadCanvasGroup.blocksRaycasts = false;
        }
    }
}
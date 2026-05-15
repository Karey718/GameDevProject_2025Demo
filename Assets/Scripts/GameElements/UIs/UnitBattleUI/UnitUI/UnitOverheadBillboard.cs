using UnityEngine;

public class UnitOverheadBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [Header("Mode")]
    [SerializeField] private bool copyCameraRotation = true;

    [Header("Optional")]
    [SerializeField] private bool useInitialRotationOffset = false;

    private Quaternion initialLocalRotation;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        initialLocalRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        if (!copyCameraRotation)
            return;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
                return;
        }

        if (useInitialRotationOffset)
            transform.rotation = targetCamera.transform.rotation * initialLocalRotation;
        else
            transform.rotation = targetCamera.transform.rotation;
    }
}
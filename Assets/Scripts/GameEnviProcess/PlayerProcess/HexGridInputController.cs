using UnityEngine;

public class HexGridInputController : MonoBehaviour
{
    [SerializeField] private HexGridMapManager hexGridMapManager;

    private void Awake()
    {
        if (hexGridMapManager == null)
        {
            hexGridMapManager = HexGridMapManager.Instance;
        }
    }

    private void Update()
    {
        if (hexGridMapManager == null || BattleSceneLoader.IsBattleActive)
        {
            return;
        }

        hexGridMapManager.HandleMouseInput();
        hexGridMapManager.HandleTestSpawn();
    }
}

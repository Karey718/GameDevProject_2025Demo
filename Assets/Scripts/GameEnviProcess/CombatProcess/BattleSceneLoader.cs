using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleSceneLoader : MonoBehaviour
{
    public static BattleSceneLoader Instance;
    public static bool IsBattleActive { get; private set; }

    private bool isBattleLoading = false;
    private const string BATTLE_SCENE_NAME = "BattleScene";

    private Camera mainCam;
    private Scene mainScene;
    private GameObject transitionMask;
    private Vector3 battleSceneOffset = new Vector3(1000, 0, 0);

    private readonly List<(Light light, bool wasEnabled)> cachedMainSceneLights = new List<(Light, bool)>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartBattle(BattleRequest request)
    {
        if (isBattleLoading || IsBattleActive)
        {
            return;
        }

        StartCoroutine(LoadBattleSceneRoutine(request));
    }

    #region Load

    IEnumerator LoadBattleSceneRoutine(BattleRequest request)
    {
        isBattleLoading = true;
        IsBattleActive = true;

        mainScene = SceneManager.GetActiveScene();
        mainCam = Camera.main;

        ShowTransitionMask();
        CacheAndDisableMainSceneLights();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(BATTLE_SCENE_NAME, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Scene battleScene = SceneManager.GetSceneByName(BATTLE_SCENE_NAME);


        foreach (GameObject rootObj in battleScene.GetRootGameObjects())
        {
            rootObj.transform.position += battleSceneOffset;
        }
        
        SceneManager.SetActiveScene(battleScene);

        BattleManager_TacticalBattlefieldHexTile battleManager = FindObjectOfType<BattleManager_TacticalBattlefieldHexTile>();
        battleManager.Init(request);

        isBattleLoading = false;
    }

    #endregion

    #region Unload

    public void EndBattle(BattleResult result)
    {
        StartCoroutine(UnloadBattleSceneRoutine(result));
    }

    IEnumerator UnloadBattleSceneRoutine(BattleResult result)
    {
        ApplyResult(result);

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(BATTLE_SCENE_NAME);

        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(mainScene);

        RestoreMainSceneLights();
        HideTransitionMask();

        IsBattleActive = false;
        isBattleLoading = false;
    }

    #endregion

    #region Visual Mask & Main Scene Light Control

    void ShowTransitionMask()
    {
        if (transitionMask == null)
        {
            transitionMask = new GameObject("BattleTransitionMask");

            Canvas canvas = transitionMask.AddComponent<Canvas>();
            if (mainCam != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = mainCam;
                canvas.planeDistance = 1f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            canvas.sortingOrder = 999;

            transitionMask.AddComponent<GraphicRaycaster>();

            GameObject imageObj = new GameObject("MaskImage");
            imageObj.transform.SetParent(transitionMask.transform, false);

            RectTransform rect = imageObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = imageObj.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.55f);
            image.raycastTarget = true;

            DontDestroyOnLoad(transitionMask);
        }

        Canvas existingCanvas = transitionMask.GetComponent<Canvas>();
        if (existingCanvas != null && mainCam != null)
        {
            existingCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            existingCanvas.worldCamera = mainCam;
        }

        transitionMask.SetActive(true);
    }

    void HideTransitionMask()
    {
        if (transitionMask != null)
        {
            transitionMask.SetActive(false);
        }
    }

    void CacheAndDisableMainSceneLights()
    {
        cachedMainSceneLights.Clear();

        foreach (GameObject root in mainScene.GetRootGameObjects())
        {
            Light[] lights = root.GetComponentsInChildren<Light>(true);
            foreach (Light lightComp in lights)
            {
                cachedMainSceneLights.Add((lightComp, lightComp.enabled));
                lightComp.enabled = false;
            }
        }
    }

    void RestoreMainSceneLights()
    {
        foreach (var lightState in cachedMainSceneLights)
        {
            if (lightState.light != null)
            {
                lightState.light.enabled = lightState.wasEnabled;
            }
        }

        cachedMainSceneLights.Clear();
    }

    #endregion

    #region Apply Result

    void ApplyResult(BattleResult result)
    {
        result.attacker.currentHP = result.attackerHP;
        result.defender.currentHP = result.defenderHP;
    }

    #endregion
}

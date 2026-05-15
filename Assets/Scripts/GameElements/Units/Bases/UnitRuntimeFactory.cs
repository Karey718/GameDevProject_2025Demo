using UnityEngine;

public static class UnitRuntimeFactory
{
    public static UnitBase CreateUnit(
        UnitDefinition definition,
        int campId,
        HexGridTile_Base startTile,
        Transform parent = null)
    {
        if (definition == null)
        {
            Debug.LogError("CreateUnit failed: UnitDefinition is null.");
            return null;
        }

        UnitInstanceData instanceData = new UnitInstanceData(definition, campId);

        return CreateUnit(instanceData, startTile, parent);
    }

    public static UnitBase CreateUnit(
        UnitInstanceData instanceData,
        HexGridTile_Base startTile,
        Transform parent = null)
    {
        if (instanceData == null)
        {
            Debug.LogError("CreateUnit failed: UnitInstanceData is null.");
            return null;
        }

        GameObject prefab = instanceData.definition != null
            ? instanceData.definition.battleMapPrefab
            : null;

        GameObject go;

        if (prefab != null)
        {
            go = Object.Instantiate(prefab, parent);
        }
        else
        {
            go = new GameObject(instanceData.DisplayName);

            if (parent != null)
                go.transform.SetParent(parent);
        }

        UnitBase unit = go.GetComponent<UnitBase>();

        if (unit == null)
            unit = go.AddComponent<UnitBase>();

        unit.Initialize(instanceData, startTile);

        return unit;
    }

    public static UnitInstanceData CreateInstance(UnitDefinition definition, int campId)
    {
        if (definition == null)
        {
            Debug.LogError("CreateInstance failed: UnitDefinition is null.");
            return null;
        }

        return new UnitInstanceData(definition, campId);
    }
}
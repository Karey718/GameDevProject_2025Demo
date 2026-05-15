[System.Serializable]
public class PlannedActionData
{
    public UnitBase ownerUnit;
    public ActionCardDefinition card;
    public ActionTargetData targetData;

    public int slotIndex;
    public int cachedAPCost;

    public ActionSlotState state;

    public bool IsEmpty => card == null;

    public bool IsReady
    {
        get
        {
            return card != null && state == ActionSlotState.Ready;
        }
    }

    public PlannedActionData(UnitBase ownerUnit, ActionCardDefinition card, int slotIndex)
    {
        this.ownerUnit = ownerUnit;
        this.card = card;
        this.slotIndex = slotIndex;

        targetData = ActionTargetData.None();
        cachedAPCost = 0;
        state = ActionSlotState.CardPlaced;
    }

    public void SetTarget(ActionTargetData target)
    {
        targetData = target ?? ActionTargetData.None();
        state = ActionSlotState.Ready;
    }

    public void SetWaitingForTarget()
    {
        state = ActionSlotState.WaitingForTarget;
    }

    public void SetInvalid()
    {
        state = ActionSlotState.Invalid;
    }

    public void SetReadyWithoutTarget()
    {
        targetData = ActionTargetData.None();
        state = ActionSlotState.Ready;
    }
}
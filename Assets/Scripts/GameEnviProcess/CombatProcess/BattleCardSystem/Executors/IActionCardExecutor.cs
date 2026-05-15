using System.Collections;

public interface IActionCardExecutor
{
    bool CanExecute(PlannedActionData action);
    IEnumerator Execute(PlannedActionData action);
}
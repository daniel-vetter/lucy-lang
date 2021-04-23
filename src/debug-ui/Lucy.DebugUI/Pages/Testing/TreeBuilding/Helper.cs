namespace Lucy.DebugUI.Pages.Testing.TreeBuilding
{
    public class Helper
    {
        public static string? GetColor(TestCaseStatus status)
        {
            return status switch
            {
                TestCaseStatus.Failed => "red",
                TestCaseStatus.Running => "dodgerblue",
                TestCaseStatus.Success => "green",
                _ => null
            };
        }
    }

}

using Lucy.DebugUI.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Lucy.DebugUI.Pages.Testing.TreeBuilding
{
    public class DirectoryTreeItem : ITreeViewNode
    {
        public string Name { get; set; } = "";
        public List<ITreeViewNode> ChildItems { get; } = new List<ITreeViewNode>();

        public IEnumerable<ITreeViewNode> ChildNodes => ChildItems;
        public IEnumerable<TreeViewNodeElement> Elements
        {
            get
            {
                
                int runningCount = 0;
                int failedCount = 0;
                int successCount = 0;
                int waitingCount = 0;
                foreach (var test in GetChildTestTreeItems(this))
                {
                    if (!test.IsRunning && test.TestResults.Length == 0)
                        waitingCount = 0;
                    if (test.IsRunning)
                        runningCount++;
                    if (test.TestResults.Length > 0 && test.TestResults.Any(x => x.Error != null))
                        failedCount++;
                    if (test.TestResults.Length > 0 && test.TestResults.All(x => x.Error == null))
                        successCount++;
                }

                string? color = null;
                if (runningCount > 0)
                    color = "dodgerblue";
                else if (waitingCount > 0)
                    color = null;
                else if (failedCount > 0)
                    color = "red";
                else if (successCount > 0)
                    color = "green";

                yield return new TreeViewNodeElement(Name, color);
                yield return new TreeViewNodeElement($"({runningCount}, {successCount}, {failedCount})", null, 0.5);
            }
        }

        private IEnumerable<TestCaseTreeItem> GetChildTestTreeItems(DirectoryTreeItem parent)
        {
            foreach (var child in parent.ChildNodes)
            {
                if (child is TestCaseTreeItem t)
                    yield return t;
                if (child is DirectoryTreeItem d)
                    foreach (var subChild in GetChildTestTreeItems(d))
                        yield return subChild;
            }
        }

        public bool Expanded { get; set; }
    }

}

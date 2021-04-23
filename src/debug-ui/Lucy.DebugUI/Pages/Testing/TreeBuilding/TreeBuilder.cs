using Lucy.DebugUI.Shared;
using Lucy.Testing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.DebugUI.Pages.Testing.TreeBuilding
{
    public class TreeBuilder
    {
        private DirectoryTreeItem _root = new DirectoryTreeItem();
        private readonly Func<TestCase, ImmutableArray<TestResult>, Task> _onTestSelected;

        public IEnumerable<ITreeViewNode> RootNodes => _root.ChildItems;

        public TreeBuilder(Func<TestCase, ImmutableArray<TestResult>, Task> onTestSelected)
        {
            _onTestSelected = onTestSelected;
        }

        public void Process(TestProgress progress)
        {
            if (progress is RunningTests r)
            {
                InitTree(r.TestCases);
            }

            if (progress is TestStarted ts)
            {
                FindTreeItem(ts.TestCase).IsRunning = true;
            }

            if (progress is TestCompleted tc)
            {
                var item = FindTreeItem(tc.TestCase);
                item.TestResults = tc.Results;
                item.IsRunning = false;
            }
        }

        private void InitTree(ImmutableArray<TestCase> testCases)
        {
            _root = new DirectoryTreeItem();
            foreach (var testCase in testCases.OrderBy(x => x.Name))
                FindTreeItem(testCase);
        }

        private TestCaseTreeItem FindTreeItem(TestCase testCase)
        {
            var parts = testCase.Name.Split("/");
            DirectoryTreeItem dir = _root;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var match = dir.ChildItems.OfType<DirectoryTreeItem>().FirstOrDefault(x => x.Name == parts[i]);
                if (match != null)
                {
                    dir = match;
                    continue;
                }

                var newDir = new DirectoryTreeItem() { Name = parts[i] };
                dir.ChildItems.Add(newDir);
                dir = newDir;
            }

            var item = dir.ChildItems.OfType<TestCaseTreeItem>().FirstOrDefault(x => x.TestCase.Name == testCase.Name);
            if (item != null)
                return item;

            var newItem = new TestCaseTreeItem(testCase, _onTestSelected);
            dir.ChildItems.Add(newItem);
            return newItem;
        }
    }
}

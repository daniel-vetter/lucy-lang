using Lucy.DebugUI.Shared;
using Lucy.Testing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.DebugUI.Pages.Testing.TreeBuilding
{
    public class TestCaseTreeItem : ITreeViewNode
    {
        private readonly Func<TestCase, ImmutableArray<TestResult>, Task> _onTestSelected;

        public TestCaseTreeItem(TestCase testCase, Func<TestCase, ImmutableArray<TestResult>, Task> onTestSelected)
        {
            TestCase = testCase;
            _onTestSelected = onTestSelected;
        }

        public TestCase TestCase { get; }
        public bool IsRunning { get; set; }

        public ImmutableArray<TestResult> TestResults { get; set; } = ImmutableArray<TestResult>.Empty;

        public IEnumerable<TreeViewNodeElement> Elements
        {
            get
            {
                string? color = null;
                if (IsRunning)
                    color = "dodgerblue";
                else if (TestResults.Length > 0 && TestResults.Any(x => x.Error != null))
                    color = "red";
                else if (TestResults.Length > 0 && TestResults.All(x => x.Error == null))
                    color = "green";

                var element = new TreeViewNodeElement(TestCase.Name, color);
                element.OnClick = async () => { await _onTestSelected(TestCase, TestResults); };
                yield return element;
            }
        }

        public IEnumerable<ITreeViewNode> ChildNodes => Array.Empty<ITreeViewNode>();
        public bool Expanded { get; set; }
        
    }

}

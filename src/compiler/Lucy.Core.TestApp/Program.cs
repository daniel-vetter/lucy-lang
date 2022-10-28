
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;

var ws = await Workspace.CreateFromPath("SampleApp");
using var sa = new SemanticAnalyzer(ws);


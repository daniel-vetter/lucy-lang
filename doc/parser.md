# Syntax Tree Parser

The syntax parser is implemented in the `Lucy.Core` project. It takes a string as input and returns multiple data structures as output:

- A tree of nodes which represent the document structure ([Model](/syntax-tree.md))
- A `ImmutableDictionary<NodeId, SyntaxTreeNode`> to quickly access a node based by its id
- A `ImmutableDictionary<NodeId, NodeId>` to quickly access the parents node id based on a node id
- A `ImmutableDictionary<Type, ImmutableArray<NodeId>>` to quickly find nodes by its type


<br>
The following features / guarantees are given by the parser:

- The parsing will never fail. No exception will be thrown. The parser will always return a syntax tree. If errors are found, they will be added to the syntax tree:
  -  **Unexpected tokens**: A UnexpectedToken will be added to the syntax tree.
  -  **Missing tokens**: Synthetic nodes will be generated. A error will be added to the SyntaxError list on these nodes.
-
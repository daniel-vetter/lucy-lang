# Development Status

This document gives a rough overview over all planned and implemented features. The list will constantly grow/shrink depending the the development progress.

- Items with a checkmark are implemented. This does not mean the implementation is feature complete or exposed to the end user, but the source can be found in the repository. (This definition will probably change with a growing maturity of the project)
- Items without a checkmark are planned but not implemented


## Features / Tasks
- [x] Language implementation
  - [x] [Language model (AST)](/syntax-tree.md)
  - [x] [Incremental Parser](/parser.md)
    - [x] Incremental syntax tree parser
    - [ ] Syntax tree merger
    - [ ] Syntax tree differ
    - [ ] Incremental id map
    - [ ] Incremental parent map
    - [ ] Incremental type map
  - [x] Incremental semantic analyzer
    - [x] Salsa like framework
  - [x] Binary emitter
    - [x] OS support
      - [x] Windows
      - [ ] Linux
    - [x] Architecture support
      - [x] x86
      - [ ] x64
      - [ ] arm32
      - [ ] arm64
- [x] Language server
  - [x] Json RPC server
  - [x] Live error reporting
  - [x] Jump to referenced file on click on an import statement
- [ ] VSCode Plugin
  - [x] Syntax highlighting
  - [x] Language server integration
  - [ ] Automatic publishing of the extension to the marketplace
  - [ ] Automatic SDK installation / update via the vscode extension
- [ ] Documentation
  - [x] Development documentation
  - [ ] "How to use" document
  - [ ] A language description

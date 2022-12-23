# Lucy Language SDK

This repository contains the Lucy programming language SDK.

### Current status

This is not a ready-to-use language implementation. Everything is under heavy development and will change in the future.

### Goals of this project

- This is mainly a learning exercise. The current main goal is to be able to compile a lucy project to a windows executable that prints out "Hello World". 
- As few dependencies as possible. The full stack should be implemented in this project. After reading the code, everyone should be able to tell how source is transformed into machine code. This means: no LLVM or parser generator.
- A good developer experience: Nobody wants to code in a plain text editor, so IDE support should be provided via a language server. Nowadays expected features like "Go to definition", Refactoring support or autocomplete should be available.
- Acceptable performance: Especially for the language server. It should be possible to open a project containing thousands of files and still be able to refactor some code or get autocomplete without any noticeable delay.

### Current implementation status

- [x] Language implementation
  - [x] [Language model (AST)](/doc/syntax-tree.md)
  - [x] [Incremental Parser](/doc/parser.md)
  - [x] Incremental semantic analyzer
    - [x] Salsa like framework
      - [ ] Cancellation support
      - [ ] Multithreading support
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
  - [ ] Asynchronous cancelable error reporting 
- [ ] VSCode Plugin
  - [x] Syntax highlighting
  - [x] Language server integration
  - [ ] Automatic publishing of the extension to the marketplace
  - [ ] Automatic SDK installation / update via the vscode extension
- [ ] Documentation
  - [x] Development documentation
  - [ ] "How to use" document
  - [ ] A language description

### Further reading

- [The Syntax Tree Model](/doc/syntax-tree.md)
- [The Parser](/doc/parser.md)
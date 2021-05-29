using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace Lucy.Feature.LanguageServer.Models
{
    public class RpcInitializeResult
    {
        /// <summary>
        /// The capabilities the language server provides.
        /// </summary>
        public RpcServerCapabilities Capabilities { get; set; } = new RpcServerCapabilities();

        /// <summary>
        /// Information about the server.
        /// </summary>
        public RpcServerInfo ServerInfo { get; set; } = new RpcServerInfo();
    }

    /// <summary>
    /// The capabilities the language server provides.
    /// </summary>
    public class RpcServerCapabilities
    {
        /// <summary>
        /// Defines how text documents are synced. Is either a detailed structure defining each notification or for backwards compatibility the TextDocumentSyncKind number.If omitted it defaults to `TextDocumentSyncKind.None`.
        /// </summary>
        public RpcTextDocumentSyncOptions? TextDocumentSync { get; set; }

        /// <summary>
        /// The server provides completion support.
        /// </summary>
        public RpcCompletionOptions? CompletionProvider { get; set; }

        /// <summary>
        /// The server provides hover support.
        /// </summary>
        public bool? HoverProvider { get; set; }

        /// <summary>
        /// The server provides signature help support.
        /// </summary>
        public RpcSignatureHelpOptions? SignatureHelpProvider { get; set; }

        /// <summary>
        /// The server provides go to declaration support.
        /// </summary>
        public bool? DeclarationProvider { get; set; }

        /// <summary>
        /// The server provides goto definition support.
        /// </summary>
        public bool? DefinitionProvider { get; set; }

        /// <summary>
        /// The server provides goto type definition support.
        /// </summary>
        public bool? TypeDefinitionProvider { get; set; }

        /// <summary>
        /// The server provides goto implementation support.
        /// </summary>
        public bool? ImplementationProvider { get; set; }

        /// <summary>
        /// The server provides find references support.
        /// </summary>
        public bool? ReferencesProvider { get; set; }

        /// <summary>
        /// The server provides document highlight support.
        /// </summary>
        public bool? DocumentHighlightProvider { get; set; }

        /// <summary>
        /// The server provides document symbol support.
        /// </summary>
        public bool? DocumentSymbolProvider { get; set; }

        /// <summary>
        /// The server provides code actions. The `CodeActionOptions` return type is only
        /// valid if the client signals code action literal support via the property
        /// `textDocument.codeAction.codeActionLiteralSupport`.
        /// </summary>
        public bool? CodeActionProvider { get; set; }

        /// <summary>
        /// The server provides code lens.
        /// </summary>
        public RpcCodeLensOptions? CodeLensProvider { get; set; }

        /// <summary>
        /// The server provides document link support.
        /// </summary>
        public RpcDocumentLinkOptions? DocumentLinkProvider { get; set; }

        /// <summary>
        /// The server provides color provider support.
        /// </summary>
        public bool? ColorProvider { get; set; }

        /// <summary>
        /// The server provides document formatting.
        /// </summary>
        public bool? DocumentFormattingProvider { get; set; }

        /// <summary>
        /// The server provides document range formatting.
        /// </summary>
        public bool? documentRangeFormattingProvider { get; set; }

        /// <summary>
        /// The server provides document formatting on typing.
        /// </summary>
        public RpcDocumentOnTypeFormattingOptions? documentOnTypeFormattingProvider { get; set; }

        /// <summary>
        /// The server provides rename support. RenameOptions may only be
        /// specified if the client states that it supports
        /// `prepareSupport` in its initial `initialize` request.
        /// </summary>
        public bool? RenameProvider { get; set; }

        /// <summary>
        /// The server provides folding provider support.
        /// </summary>
        public bool? FoldingRangeProvider { get; set; }

        /// <summary>
        /// The server provides execute command support.
        /// </summary>
        public RpcExecuteCommandOptions? ExecuteCommandProvider { get; set; }

        /// <summary>
        /// The server provides selection range support.
        /// </summary>
        public bool? SelectionRangeProvider { get; set; }

        /// <summary>
        /// The server provides workspace symbol support.
        /// </summary>
        public bool? WorkspaceSymbolProvider { get; set; }

        /// <summary>
        /// Workspace specific server capabilities
        /// </summary>
        public RpcWorkspaceOptions? Workspace { get; set; }
    }

    /// <summary>
    /// Workspace specific server capabilities
    /// </summary>
    public class RpcWorkspaceOptions
    {
        /// <summary>
        /// The server supports workspace folder.
        /// </summary>
        public RpcWorkspaceFoldersServerCapabilities? WorkspaceFolders { get; set; }

        ///<summary>
        ///The server is interested in file notifications/requests.
        ///</summary>
        public RpcWorkspaceFileOperationsOptions? FileOperations { get; set; }
    }

    public class RpcWorkspaceFileOperationsOptions
    {
        /// <summary>
        /// The server is interested in receiving didCreateFiles notifications.
        /// </summary>
        public RpcFileOperationRegistrationOptions? DidCreate { get; set; }

        /// <summary>
        /// The server is interested in receiving willCreateFiles requests.
        /// </summary>
        public RpcFileOperationRegistrationOptions? WillCreate { get; set; }

        /// <summary>
        /// The server is interested in receiving didRenameFiles notifications.
        /// </summary>
        public RpcFileOperationRegistrationOptions? DidRename { get; set; }

        /// <summary>
        /// The server is interested in receiving willRenameFiles requests.
        /// </summary>
        public RpcFileOperationRegistrationOptions? WillRename { get; set; }

        /// <summary>
        /// The server is interested in receiving didDeleteFiles notifications.
        /// </summary>
        public RpcFileOperationRegistrationOptions? DidDelete { get; set; }

        /// <summary>
        /// The server is interested in receiving willDeleteFiles requests.
        /// </summary>
        public RpcFileOperationRegistrationOptions? WillDelete { get; set; }
    }

    /// <summary>
    /// The options to register for file operations.
    /// </summary>
    public class RpcFileOperationRegistrationOptions
    {
        public RpcFileOperationFilter[] Filters { get; set; } = null!;
    }

    /// <summary>
    /// A filter to describe in which file operation requests or notifications the server is interested in.
    /// </summary>
    public class RpcFileOperationFilter
    {
        /// <summary>
        /// A Uri like `file` or `untitled`.
        /// </summary>
        public string? Scheme { get; set; }

        public RpcFileOperationPattern Pattern { get; set; } = null!;
    }

    /// <summary>
    /// A pattern to describe in which file operation requests or notifications the server is interested in.
    /// </summary>
    public class RpcFileOperationPattern
    {
        /// <summary>
        /// he glob pattern to match. Glob patterns can have the following syntax:
        /// - `*` to match one or more characters in a path segment
        /// - `?` to match on one character in a path segment
        /// - `**` to match any number of path segments, including none
        /// - `{}` to group conditions(e.g. `**​/*.{ts,js}` matches all TypeScript
        /// and JavaScript files)
        /// - `[]` to declare a range of characters to match in a path segment
        /// (e.g., `example.[0-9]` to match on `example.0`, `example.1`, …)
        /// - `[!...]` to negate a range of characters to match in a path segment
        /// (e.g., `example.[!0-9]` to match on `example.a`, `example.b`, but
	    /// not `example.0`)
        /// </summary>
        public string Glob { get; set; } = null!;

        /// <summary>
        /// Whether to match files or folders with this pattern. Matches both if undefined.
        /// </summary>
        public RpcFileOperationPatternKind? Matches { get; set; }

        /// <summary>
        /// Additional options used during matching.
        /// </summary>
        public RpcFileOperationPatternOptions? Options { get; set; }
    }

    /// <summary>
    /// A pattern kind describing if a glob pattern matches a file a folder or both.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RpcFileOperationPatternKind
    {
        [EnumMember(Value = "file")] File,
        [EnumMember(Value = "folder")] Folder
    }

    /// <summary>
    /// Matching options for the file operation pattern.
    /// </summary>
    public class RpcFileOperationPatternOptions
    {
        /// <summary>
        /// The pattern should be matched ignoring casing.
        /// </summary>
        public bool IgnoreCase { get; set; }
    }

    public class RpcWorkspaceFoldersServerCapabilities
    {
        /// <summary>
        /// The server has support for workspace folders
        /// </summary>
        public bool? Supported { get; set; }

        /// <summary>
        /// Whether the server wants to receive workspace folder change notifications.
        /// If a string is provided, the string is treated as an ID
        /// under which the notification is registered on the client
        /// side.The ID can be used to unregister for these events
        /// using the `client/unregisterCapability` request.
        /// </summary>
        public string? ChangeNotifications { get; set; }
    }

    /// <summary>
    /// The server provides execute command support.
    /// </summary>
    public class RpcExecuteCommandOptions
    {
        /// <summary>
        /// The commands to be executed on the server
        /// </summary>
        public string[] Commands { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// The server provides document formatting on typing.
    /// </summary>
    public class RpcDocumentOnTypeFormattingOptions
    {
        /// <summary>
        /// A character on which formatting should be triggered, like `}`.
        /// </summary>
        public string FirstTriggerCharacter { get; set; } = "}";

        /// <summary>
        /// More trigger characters.
        /// </summary>
        public string[]? MoreTriggerCharacter { get; set; }
    }

    /// <summary>
    /// The server provides document link support.
    /// </summary>
    public class RpcDocumentLinkOptions
    {
        /// <summary>
        /// Document links have a resolve provider as well.
        /// </summary>
        public bool? ResolveProvider { get; set; }
    }

    /// <summary>
    /// The server provides code lens.
    /// </summary>
    public class RpcCodeLensOptions
    {
        /// <summary>
        /// Code lens has a resolve provider as well.
        /// </summary>
        public bool? ResolveProvider { get; set; }
    }

    public class RpcSignatureHelpOptions
    {
        /// <summary>
        /// The characters that trigger signature help automatically.
        /// </summary>
        public string[]? TriggerCharacters { get; set; }

        /// <summary>
        /// List of characters that re-trigger signature help.
        /// These trigger characters are only active when signature help is already showing.All trigger characters
        /// are also counted as re-trigger characters.
        /// </summary>
        public string[]? RetriggerCharacters { get; set; }
    }

    /// <summary>
    /// The server provides completion support.
    /// </summary>
    public class RpcCompletionOptions
    {
        /// <summary>
        /// Most tools trigger completion request automatically without explicitly requesting
        /// it using a keyboard shortcut(e.g.Ctrl+Space). Typically they do so when the user
        /// 
        /// starts to type an identifier.For example if the user types `c` in a JavaScript file
        /// code complete will automatically pop up present `console` besides others as a
        /// completion item.Characters that make up identifiers don't need to be listed here.
        /// 
        /// If code complete should automatically be trigger on characters not being valid inside
        /// an identifier (for example `.` in JavaScript) list them in `triggerCharacters`.
        /// </summary>
        public string[]? TriggerCharacters { get; set; }

        /// <summary>
        /// The list of all possible characters that commit a completion. This field can be used
        /// if clients don't support individual commit characters per completion item. See
        /// `ClientCapabilities.textDocument.completion.completionItem.commitCharactersSupport`.
        /// 
        /// If a server provides both `allCommitCharacters` and commit characters on an individual
        /// completion item the ones on the completion item win.
	    ///</summary>
        public string[]? AllCommitCharacters { get; set; }

        /// <summary>
        /// The server provides support to resolve additional information for a completion item.
        /// </summary>
        public bool? ResolveProvider { get; set; }
    }

    public class RpcTextDocumentSyncOptions
    {
        /// <summary>
        /// Open and close notifications are sent to the server. If omitted open close notification should not be sent.
        /// </summary>
        public bool OpenClose { get; set; }

        /// <summary>
        /// Change notifications are sent to the server. See TextDocumentSyncKind.None, TextDocumentSyncKind.Full and TextDocumentSyncKind.Incremental.If omitted it defaults to TextDocumentSyncKind.None.
        /// </summary>
        public RpcTextDocumentSyncKind Change { get; set; }
    }

    public enum RpcTextDocumentSyncKind
    {
        None = 0,
        Full = 1,
        Incremental = 2
    }

    /// <summary>
    /// Information about the server.
    /// </summary>
    public class RpcServerInfo
    {
        /// <summary>
        /// The name of the server as defined by the server.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The server's version as defined by the server.
        /// </summary>
        public string? Version { get; set; }
    }

    public class RpcInitializeParams
    {
        /// <summary>
        /// The process Id of the parent process that started
        /// the server. Is null if the process has not been started by another process.
        /// If the parent process is not alive then the server should exit (see exit notification) its process.
        /// </summary>
        public long? ProcessId { get; set; }

        /// <summary>
        /// Information about the client
        /// </summary>
        public RpcClientInfo? ClientInfo { get; set; }

        /// <summary>
        /// The rootPath of the workspace. Is null if no folder is open.
        /// </summary>
        public string? RootPath { get; set; }

        /// <summary>
        /// The rootUri of the workspace. Is null if no folder is open. If both `rootPath` and `rootUri` are set `rootUri` wins.
        /// </summary>
        public Uri? RootUri { get; set; }

        /// <summary>
        /// The capabilities provided by the client (editor or tool)
        /// </summary>
        public RpcClientCapabilities Capabilities { get; set; } = new RpcClientCapabilities();

        /// <summary>
        /// The initial trace setting. If omitted trace is disabled ('off').
        /// </summary>
        public RpcTrace? Trace { get; set; }

        /// <summary>
        /// The workspace folders configured in the client when the server starts.
        /// This property is only available if the client supports workspace folders.
        /// It can be `null` if the client supports workspace folders but none are
        /// configured.
        /// </summary>
        public RpcWorkspaceFolder[]? WorkspaceFolders { get; set; }
    }

    public enum RpcTrace
    {
        Off,
        Messages,
        Verbose
    }

    public class RpcClientCapabilities
    {
        /// <summary>
        /// Workspace specific client capabilities.
        /// </summary>
        public RpcWorkspaceClientCapabilities? Workspace { get; set; }

        /// <summary>
        /// Text document specific client capabilities.
        /// </summary>
        public RpcTextDocumentClientCapabilities? TextDocument { get; set; }
    }

    public class RpcTextDocumentClientCapabilities
    {
        public RpcTextDocumentSyncClientCapabilities? Synchronization { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/completion` request.
        /// </summary>
        public RpcCompletionClientCapabilities? Completion { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/hover` request.
        /// </summary>
        public RpcHoverClientCapabilities? Hover { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/signatureHelp` request.
        /// </summary>
        public RpcSignatureHelpClientCapabilities? SignatureHelp { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/declaration` request.
        /// </summary>
        public RpcDeclarationClientCapabilities? Declaration { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/definition` request.
        /// </summary>
        public RpcDefinitionClientCapabilities? Definition { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/typeDefinition` request.
        /// </summary>
        public RpcTypeDefinitionClientCapabilities? TypeDefinition { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/implementation` request.
        /// </summary>
        public RpcImplementationClientCapabilities? Implementation { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/references` request.
        /// </summary>
        public RpcReferenceClientCapabilities? References { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/documentHighlight` request.
        /// </summary>
        public RpcDocumentHighlightClientCapabilities? DocumentHighlight { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/documentSymbol` request.
        /// </summary>
        public RpcDocumentSymbolClientCapabilities? DocumentSymbol { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/codeAction` request.
        /// </summary>
        public RpcCodeActionClientCapabilities? CodeAction { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/codeLens` request.
        /// </summary>
        public RpcCodeLensClientCapabilities? CodeLens { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/documentLink` request.
        /// </summary>
        public RpcDocumentLinkClientCapabilities? DocumentLink { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/documentColor` and the `textDocument/colorPresentation` request.
        /// </summary>
        public RpcDocumentColorClientCapabilities? ColorProvider { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/formatting` request.
        /// </summary>
        public RpcDocumentFormattingClientCapabilities? Formatting { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/rangeFormatting` request.
        /// </summary>
        public RpcDocumentRangeFormattingClientCapabilities? RangeFormatting { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/onTypeFormatting` request.
        /// </summary>
        public RpcDocumentOnTypeFormattingClientCapabilities? OnTypeFormatting { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/rename` request.
        /// </summary>
        public RpcRenameClientCapabilities? Rename { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/publishDiagnostics` notification.
        /// </summary>
        public RpcPublishDiagnosticsClientCapabilities? PublishDiagnostics { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/foldingRange` request.
        /// </summary>
        public RpcFoldingRangeClientCapabilities? FoldingRange { get; set; }

        /// <summary>
        /// Capabilities specific to the `textDocument/selectionRange` request.
        /// </summary>
        public RpcSelectionRangeClientCapabilities? SelectionRange { get; set; }
    }

    public class RpcSelectionRangeClientCapabilities
    {
        //TODO
    }

    public class RpcFoldingRangeClientCapabilities
    {
        //TODO
    }

    public class RpcPublishDiagnosticsClientCapabilities
    {
        //TODO
    }

    public class RpcRenameClientCapabilities
    {
        //TODO
    }

    public class RpcDocumentOnTypeFormattingClientCapabilities
    {
    }

    public class RpcDocumentRangeFormattingClientCapabilities
    {
        //TODO
    }

    public class RpcDocumentFormattingClientCapabilities
    {
        //TODO
    }

    public class RpcDocumentColorClientCapabilities
    {
        //TODO
    }

    public class RpcDocumentLinkClientCapabilities
    {
        //TODO
    }

    public class RpcCodeLensClientCapabilities
    {
        //TODO
    }

    public class RpcCodeActionClientCapabilities
    {
        //TODO
    }

    public class RpcDocumentSymbolClientCapabilities
    {
        //TODO
    }

    public class RpcDocumentHighlightClientCapabilities
    {
        //TODO
    }

    public class RpcReferenceClientCapabilities
    {
        //TODO
    }

    public class RpcImplementationClientCapabilities
    {
        //TODO
    }

    public class RpcTypeDefinitionClientCapabilities
    {
        //TODO
    }

    public class RpcDefinitionClientCapabilities
    {
        //TODO
    }

    public class RpcDeclarationClientCapabilities
    {
        //TODO
    }

    public class RpcSignatureHelpClientCapabilities
    {
        //TODO
    }

    public class RpcHoverClientCapabilities
    {
        /// <summary>
        /// Whether hover supports dynamic registration.
        /// </summary>
        public bool? DynamicRegistration { get; set; }

        /// <summary>
        /// Client supports the follow content formats for the content
        /// property. The order describes the preferred format of the client.
        /// </summary>
        public RpcMarkupKind[]? ContentFormat { get; set; }
    }

    public class RpcCompletionClientCapabilities
    {
        //TODO
    }

    public class RpcTextDocumentSyncClientCapabilities
    {
        /// <summary>
        /// Whether text document synchronization supports dynamic registration.
        /// </summary>
        public bool? DynamicRegistration { get; set; }

        /// <summary>
        /// The client supports sending will save notifications.
        /// </summary>
        public bool? WillSave { get; set; }

        /// <summary>
        /// The client supports sending a will save request and waits for a response providing text edits which will be applied to the document before it is saved.
        /// </summary>
        public bool? WillSaveWaitUntil { get; set; }

        /// <summary>
        /// The client supports did save notifications.
        /// </summary>
        public bool? DidSave { get; set; }
    }

    public class RpcWorkspaceClientCapabilities
    {
        /// <summary>
        ///  The client supports applying batch edits to the workspace by supporting the request 'workspace/applyEdit'
        /// </summary>
        public bool? ApplyEdit { get; set; }

        /// <summary>
        /// Capabilities specific to `WorkspaceEdit`s
        /// </summary>
        public RpcWorkspaceEditClientCapabilities? WorkspaceEdit { get; set; }

        /// <summary>
        /// Capabilities specific to the `workspace/didChangeConfiguration` notification.
        /// </summary>
        public RpcDidChangeConfigurationClientCapabilities? DidChangeConfiguration { get; set; }

        /// <summary>
        /// Capabilities specific to the `workspace/didChangeWatchedFiles` notification.
        /// </summary>
        public RpcDidChangeWatchedFilesClientCapabilities? DidChangeWatchedFiles { get; set; }

        /// <summary>
        /// Capabilities specific to the `workspace/symbol` request.
        /// </summary>
        public RpcWorkspaceSymbolClientCapabilities? Symbol { get; set; }

        /// <summary>
        /// Capabilities specific to the `workspace/executeCommand` request.
        /// </summary>
        public RpcExecuteCommandClientCapabilities? ExecuteCommand { get; set; }

        /// <summary>
        /// The client has support for workspace folders.
        /// </summary>
        public bool? WorkspaceFolders { get; set; }

        /// <summary>
        /// The client supports `workspace/configuration` requests.
        /// </summary>
        public bool? Configuration { get; set; }
    }

    public class RpcExecuteCommandClientCapabilities
    {
        //TODO
    }

    public class RpcWorkspaceSymbolClientCapabilities
    {
        //TODO
    }

    public class RpcDidChangeWatchedFilesClientCapabilities
    {
        //TODO
    }

    public class RpcDidChangeConfigurationClientCapabilities
    {
        //TODO
    }

    public class RpcWorkspaceEditClientCapabilities
    {
        //TODO
    }

    public class RpcWorkspaceFolder
    {
        /// <summary>
        /// The associated URI for this workspace folder.
        /// </summary>
        public string Uri { get; set; } = "";

        /// <summary>
        /// The name of the workspace folder. Used to refer to this workspace folder in the user interface.
        /// </summary>
        public string Name { get; set; } = "";
    }

    public class RpcClientInfo
    {
        /// <summary>
        /// The name of the client as defined by the client.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The client's version as defined by the client.
        /// </summary>
        public string? Version { get; set; }
    }

    public class RpcTextDocumentIdentifier
    {
        /// <summary>
        /// The text document's URI.
        /// </summary>
        public Uri Uri { get; set; } = null!;
    }

    public class RpcVersionedTextDocumentIdentifier : RpcTextDocumentIdentifier
    {
        /// <summary>
        /// The version number of this document.
        /// 
        /// The version number of a document will increase after each change,
        /// including undo/redo. The number doesn't need to be consecutive.
        /// </summary>
        public int Version { get; set; }
    }

    public class RpcPosition
    {
        /// <summary>
        /// Line position in a document (zero-based).
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Character offset on a line in a document (zero-based). Assuming that the line is
        /// represented as a string, the `character` value represents the gap between the
        /// `character` and `character + 1`.
        /// 
        /// If the character value is greater than the line length it defaults back to the
        /// line length.
        /// </summary>
        public int Character { get; set; }

        public override string ToString() => $"{Line}:{Character}";
    }

    public class RpcMarkupContent
    {
        /// <summary>
        /// The type of the Markup
        /// </summary>
        public RpcMarkupKind Kind { get; set; } = RpcMarkupKind.Plaintext;

        /// <summary>
        /// The content itself
        /// </summary>
        public string Value { get; set; } = "";
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RpcMarkupKind
    {
        [EnumMember(Value = "plaintext")] Plaintext,
        [EnumMember(Value = "markdown")] Markdown
    }

    public class RpcRange
    {
        /// <summary>
        /// The range's start position.
        /// </summary>
        public RpcPosition Start { get; set; } = new RpcPosition();

        /// <summary>
        /// The range's end position.
        /// </summary>
        public RpcPosition End { get; set; } = new RpcPosition();

        public override string ToString() => $"{Start}-{End}";
    }

    /// <summary>
    /// An event describing a change to a text document. If range and rangeLength are
    /// omitted the new text is considered to be the full content of the document.
    /// </summary>
    public class RpcTextDocumentContentChangeEvent
    {
        /// <summary>
        /// The range of the document that changed.
        /// </summary>
        public RpcRange? Range { get; set; } = new RpcRange();

        /// <summary>
        /// The new text for the provided range.
        /// </summary>
        public string Text { get; set; } = "";
    }


    public class RpcTextDocumentItem
    {
        /// <summary>
        /// The text document's URI.
        /// </summary>
        public Uri Uri { get; set; } = null!;

        /// <summary>
        /// The text document's language identifier.
        /// </summary>
        public string LanguageId { get; set; } = null!;

        /// <summary>
        /// The version number of this document (it will increase after each
        /// change, including undo/redo).
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The content of the opened text document.
        /// </summary>
        public string Text { get; set; } = null!;
    }

    public class RpcDiagnostic
    {
        /// <summary>
        /// The range at which the message applies.
        /// </summary>
        public RpcRange Range { get; set; } = null!;

        /// <summary>
        /// The diagnostic's severity. Can be omitted. If omitted it is up to the
        /// client to interpret diagnostics as error, warning, info or hint.
        /// </summary>
        public RpcDiagnosticSeverity? Severity { get; set; }

        /// <summary>
        /// The diagnostic's code, which might appear in the user interface.
        /// </summary>
        public string? Code { get; set; } = null;

        /// <summary>
        /// An optional property to describe the error code.
        /// </summary>
        public RpcCodeDescription? CodeDescription { get; set; } = null;

        /// <summary>
        /// A human-readable string describing the source of this
        /// diagnostic, e.g. 'typescript' or 'super lint'.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// The diagnostic's message.
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Additional metadata about the diagnostic.
        /// </summary>
        public RpcDiagnosticTag[]? Tags { get; set; }

        /// <summary>
        /// An array of related diagnostic information, e.g. when symbol-names within
        /// a scope collide all definitions can be marked via this property.
        /// </summary>
        public RpcDiagnosticRelatedInformation[]? RelatedInformation { get; set; }
    }

    public enum RpcDiagnosticSeverity
    {
        /// <summary>
        /// Reports an error.
        /// </summary>
        Error = 1,

        /// <summary>
        /// Reports a warning.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Information
        /// </summary>
        Information = 3,

        /// <summary>
        /// Hint
        /// </summary>
        Hint = 4
    }

    /// <summary>
    /// The diagnostic tags.
    /// </summary>
    public enum RpcDiagnosticTag
    {
        /// <summary>
        /// Unused or unnecessary code.
        /// 
        /// Clients are allowed to render diagnostics with this tag faded out
        /// instead of having an error squiggle.
        /// </summary>
        Unnecessary = 1,

        /// <summary>
        /// Deprecated or obsolete code.
        /// 
        /// Clients are allowed to rendered diagnostics with this tag strike through.
        /// </summary>
        DiagnosticTag = 2
    }

    /// <summary>
    /// Structure to capture a description for an error code.
    /// </summary>
    public class RpcCodeDescription
    {
        /// <summary>
        /// An URI to open with more information about the diagnostic error.
        /// </summary>
        public Uri Href { get; set; } = null!;
    }

    /// <summary>
    /// Represents a related message and source code location for a diagnostic.
    /// This should be used to point to code locations that cause or are related to
    /// a diagnostics, e.g when duplicating a symbol in a scope.
    /// </summary>
    public class RpcDiagnosticRelatedInformation
    {
        /// <summary>
        /// The location of this related diagnostic information.
        /// </summary>
        public RpcLocation Location { get; set; } = null!;

        /// <summary>
        /// The message of this related diagnostic information.
        /// </summary>
        public string? Message { get; set; }
    }

    public class RpcLocation
    {
        public Uri Uri { get; set; } = null!;
        public RpcRange Range { get; set; } = null!;
    }
}
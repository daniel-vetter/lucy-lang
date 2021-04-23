import { ExtensionContext, OutputChannel, TextDocument, window, workspace } from "vscode"
import { LanguageClient } from "vscode-languageclient";
import { getLanguageClientFor, shutdownAllLanguageClients } from "./language-client-provider";

const clients: Map<string, LanguageClient> = new Map();
let _outputChannel: OutputChannel;

export function activate(context: ExtensionContext) {
    _outputChannel = window.createOutputChannel('Lucy');

    workspace.onDidOpenTextDocument(x => onDidOpenTextDocument(x));
    workspace.textDocuments.forEach(x => onDidOpenTextDocument(x));
}

export async function deactivate(): Promise<void> {
    shutdownAllLanguageClients();
}


function onDidOpenTextDocument(document: TextDocument): void {
    if (document.languageId !== 'lucy') {
        return;
    }

    const workspaceFolder = workspace.getWorkspaceFolder(document.uri);
    if (workspaceFolder) {
        getLanguageClientFor(workspaceFolder);
    }
}

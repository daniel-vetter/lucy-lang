import { ExtensionContext, OutputChannel, TextDocument, window, workspace, commands, ViewColumn } from "vscode"
import { LanguageClient } from "vscode-languageclient";
import { getLanguageClientFor, getSingleLanguageClient, shutdownAllLanguageClients } from "./language-client-provider";

const clients: Map<string, LanguageClient> = new Map();
let _outputChannel: OutputChannel;

export function activate(context: ExtensionContext) {
    _outputChannel = window.createOutputChannel('Lucy');

    workspace.onDidOpenTextDocument(x => onDidOpenTextDocument(x));
    workspace.textDocuments.forEach(x => onDidOpenTextDocument(x));

    context.subscriptions.push(commands.registerCommand("lucy.openSyntaxTree", async () => await openSyntaxTree()));
    context.subscriptions.push(commands.registerCommand("lucy.openScopeTree", async () => await openScopeTree()));
    context.subscriptions.push(commands.registerCommand("lucy.openAssembly", async () => await openAssembly()));
    context.subscriptions.push(commands.registerCommand("lucy.attachDebugger", async () => await attachDebugger()));
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

async function attachDebugger(): Promise<void> {
    var client = getSingleLanguageClient();
    if (client != undefined) {
        await client.sendRequest<string>("debug/attachDebugger");
    }
}

async function openSyntaxTree(): Promise<void> {
    var client = getSingleLanguageClient();

    if (client == undefined) {
        return;
    }

    const result = await client.sendRequest<string>("debug/getSyntaxTree", {
        Uri: window.activeTextEditor?.document.uri + ""
    });

    const panel = window.createWebviewPanel("lucy-syntax-tree", "Lucy Syntax Tree", ViewColumn.Active, {
        enableScripts: true,
        retainContextWhenHidden: true
    });
    panel.webview.html = result;
}

async function openScopeTree(): Promise<void> {
    var client = getSingleLanguageClient();

    if (client == undefined) {
        return;
    }

    const result = await client.sendRequest<string>("debug/getScopeTree", {
        Uri: window.activeTextEditor?.document.uri + ""
    });

    const panel = window.createWebviewPanel("lucy-scope-tree", "Lucy Scope Tree", ViewColumn.Active, {
        enableScripts: true,
        retainContextWhenHidden: true
    });
    panel.webview.html = result;
}

async function openAssembly(): Promise<void> {
    var client = getSingleLanguageClient();

    if (client != undefined) {
        const result = await client.sendRequest<string>("debug/getAssembly");

        const doc = await workspace.openTextDocument({
            language: "asm-collection",
            content: result
        });

        window.showTextDocument(doc);
    }
}

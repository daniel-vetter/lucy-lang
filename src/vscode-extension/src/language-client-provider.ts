import { WorkspaceFolder, workspace } from "vscode";
import { LanguageClient, LanguageClientOptions, ServerOptions, TransportKind } from "vscode-languageclient";

const _clients: Map<string, LanguageClient> = new Map();

export function getLanguageClientFor(workespaceFolder: WorkspaceFolder): LanguageClient {

    const existing = _clients.get(workespaceFolder.uri.toString());
    if (existing) {
        return existing;
    }
    
    const serverOptions: ServerOptions = {
        command: "C:\\Dropbox\\coding\\csharp\\Lucy2\\src\\compiler\\Lucy.App\\bin\\Debug\\net5.0\\Lucy2.App.exe",
        args: ["language-server"],
        transport: TransportKind.stdio
    }
    const clientOptions: LanguageClientOptions = {
        documentSelector: [
            { scheme: "file", language: "lucy" }
        ],
        diagnosticCollectionName: 'lucy-language-extension',
        workspaceFolder: workespaceFolder,
        synchronize: {
            fileEvents: workspace.createFileSystemWatcher("**/*.lucy")
        }
    };
    const client = new LanguageClient("lucy-language-extension", "Lucy Language Server", serverOptions, clientOptions);
    client.start();
    _clients.set(workespaceFolder.uri.toString(), client);
    return client;
}

export function shutdownAllLanguageClients() {
    let promises: Thenable<void>[] = [];
	for (let client of _clients.values()) {
		promises.push(client.stop());
	}
    return Promise.all(promises).then(() => undefined);
}
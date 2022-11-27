import { WorkspaceFolder, workspace } from "vscode";
import { LanguageClient, LanguageClientOptions, ServerOptions, TransportKind } from "vscode-languageclient";
import * as net from "net";

const _clients: Map<string, LanguageClient> = new Map();

export function getLanguageClientFor(workespaceFolder: WorkspaceFolder): LanguageClient {

    const existing = _clients.get(workespaceFolder.uri.toString());
    if (existing) {
        return existing;
    }
    
    let serverOptions: ServerOptions = {
        command: "C:\\OneDrive\\coding\\csharp\\lucy-lang\\src\\compiler\\Lucy.App.LanguageServer\\bin\\Debug\\net7.0\\Lucy.App.LanguageServer.exe",
        transport: TransportKind.stdio
    }

    if (process.env.LUCY_LANGUAGE_SERVER_NETWORK_ENDPOINT_PORT) {
        serverOptions = () => {
            let socket = net.connect({
                port: +process.env.LUCY_LANGUAGE_SERVER_NETWORK_ENDPOINT_PORT!
            });

            return Promise.resolve({
                writer: socket,
                reader: socket
            })
        }
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

export function getSingleLanguageClient() : LanguageClient | undefined {
    if (_clients.size == 1)
        return _clients.values().next().value;
    return undefined;
}
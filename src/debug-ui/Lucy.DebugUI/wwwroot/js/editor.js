let ensureMonacoIsLoaded = new Promise(resolve => {
    require(['vs/editor/editor.main'], function () {
        resolve();
    });
});

export async function createEditor(component, element, content) {
    await ensureMonacoIsLoaded;
    element.component = component;
    element.editor = monaco.editor.create(element, {
        value: content,
        language: 'json',
        automaticLayout: true,
        scrollBeyondLastLine: false,
        readOnly: true,
        matchBrackets: "never",
        theme: "vs-dark",
        minimap: {
            enabled: false
        }
    });

    element.editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter, () => {
        element.component.invokeMethodAsync('OnCtrlEnterPressed');
    });

    element.editor.addCommand(monaco.KeyCode.Escape, () => {
        element.component.invokeMethodAsync('OnEscapePressed');
    });

    monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
        validate: true,
        enableSchemaRequest: true
    });
}


export async function focusEditor(element) {
    await ensureMonacoIsLoaded;
    setTimeout(() => {
        element.editor.focus();
    }, 100);
}

export async function setText(element, text) {
    await ensureMonacoIsLoaded;
    element.editor.getModel().setValue(text);
}

export async function getText(element) {
    await ensureMonacoIsLoaded;
    return element.editor.getModel().getValue();
}

export async function setCursorPosition(element, line, column) {
    await ensureMonacoIsLoaded;
    element.editor.setPosition({ lineNumber: line, column: column });
}
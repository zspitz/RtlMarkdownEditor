namespace RtlMarkdownEditor;

public record MessageFromWebView(string command, string value, bool isDirty);

public record MessageToWebView(bool success, string? newValue);

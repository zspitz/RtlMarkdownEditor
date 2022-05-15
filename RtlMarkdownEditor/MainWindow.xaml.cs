using Ookii.Dialogs.Wpf;
using System;
using static System.IO.Path;
using static System.IO.File;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using ZSpitz.Util;
using System.Threading;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Reflection;

namespace RtlMarkdownEditor {
    public partial class MainWindow : Window {
        private string? currentFilePath;
        static readonly string execPath = GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        static readonly string symlinkPath = Combine(execPath, "assets");
        static readonly string dummySymlinkTarget = Combine(execPath, "assets1");

        private void recreateSymlink(string target) {
            if (Directory.Exists(symlinkPath)) {
                Directory.Delete(symlinkPath);
            }
            Directory.CreateSymbolicLink(symlinkPath, target);
        }

        private string? CurrentFilePath {
            get => currentFilePath;
            set {
                currentFilePath = value;
                var symlinkTarget =
                    currentFilePath.IsNullOrWhitespace() ?
                        dummySymlinkTarget :
                        GetDirectoryName(currentFilePath)!;
                recreateSymlink(symlinkTarget);
            }
        }

        public MainWindow() {
            InitializeComponent();
            Loaded += async (s, e) => await initializeAsync();
            Closing += (s, e) =>
                e.Cancel = MessageBoxResult.No == MessageBox.Show(
                    "Warning: if there are unsaved changes they will be lost.\nClose anyway?",
                    "",
                    MessageBoxButton.YesNo
                );
        }

        private bool promptSave(string value, bool forceNewFilename = false) {
            if (forceNewFilename || CurrentFilePath.IsNullOrWhitespace()) {
                var dlgSave = new VistaSaveFileDialog() {
                    AddExtension = true,
                    DefaultExt = ".md",
                    Filter = "Markdown|*.md|All Files|*.*",
                    FilterIndex = 0,
                    OverwritePrompt = true,
                    RestoreDirectory = true,
                    Title = "Save file to:"
                };
                var result = dlgSave.ShowDialog() ?? false;
                if (!result) { return false; }
                CurrentFilePath = dlgSave.FileName;
            }
            WriteAllText(CurrentFilePath, value);
            return true;
        }

        private async Task initializeAsync() {
            await webview.EnsureCoreWebView2Async();

            recreateSymlink(dummySymlinkTarget);
            
            webview.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "assets",
                symlinkPath,
                CoreWebView2HostResourceAccessKind.Allow
            );

            var html = await ReadAllTextAsync("container.html");
            webview.NavigateToString(html);
            webview.CoreWebView2.WebMessageReceived += (s, e) =>
                SynchronizationContext.Current!.Post(_ => {
                    var (command, value, isDirty) =
                        JsonSerializer.Deserialize<MessageFromWebView>(e.WebMessageAsJson) ??
                        throw new InvalidOperationException("Invalid message");

                    var success = false;
                    string? newValue = null;

                    switch (command) {
                        case "save":
                            if (!isDirty) { return; }
                            success = promptSave(value);
                            break;

                        case "saveas":
                            success = promptSave(value, true);
                            break;

                        case "open":
                        case "new":
                            if (isDirty) {
                                var saveChanges = MessageBox.Show("Save changes?", "", MessageBoxButton.YesNoCancel);
                                switch (saveChanges) {
                                    case MessageBoxResult.Cancel: return;
                                    case MessageBoxResult.Yes:
                                        success = promptSave(value);
                                        break;
                                    case MessageBoxResult.No:
                                        success = true;
                                        break;
                                }
                            }

                            if (success || !isDirty) {
                                if (command == "open") {
                                    var dlgOpen = new VistaOpenFileDialog() {
                                        CheckFileExists = true,
                                        CheckPathExists = true,
                                        DefaultExt = ".md",
                                        Filter = "Markdown|*.md|All Files|*.*",
                                        FilterIndex = 0,
                                        Multiselect = false,
                                        RestoreDirectory = true,
                                        ShowReadOnly = false,
                                        Title = "Open file"
                                    };
                                    var result = dlgOpen.ShowDialog() ?? false;
                                    if (!result) { return; }
                                    CurrentFilePath = dlgOpen.FileName;
                                    newValue = ReadAllText(CurrentFilePath);
                                    success = true;
                                } else { // "new"
                                    CurrentFilePath = "";
                                    newValue = "";
                                }
                            }

                            var json = JsonSerializer.Serialize(new MessageToWebView(success, newValue));
                            webview.CoreWebView2.PostWebMessageAsJson(json);

                            // TODO replace with databinding
                            Title = CurrentFilePath ?? "";
                            break;
                    }
                }, null);
        }
    }
}

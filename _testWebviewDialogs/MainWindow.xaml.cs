using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace _testWebviewDialogs;
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        Loaded += async (s, e) => await initializeAsync();
    }

    private async Task initializeAsync() {
        await webview.EnsureCoreWebView2Async();

        var symlinkPath =
            Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location!
                )!,
                "assets"
            );

        Directory.CreateDirectory(symlinkPath);

        webview.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "assets",
            symlinkPath,
            CoreWebView2HostResourceAccessKind.Allow
        );

        webview.CoreWebView2.WebMessageReceived += (s, e) => {
            Directory.Delete(symlinkPath);
            Directory.CreateSymbolicLink(symlinkPath, @"C:\Users\Spitz\Desktop\testmd");
            webview.CoreWebView2.PostWebMessageAsString(@"breakpoint.bmp");
        };

        var html = await File.ReadAllTextAsync("container.html");
        webview.NavigateToString(html);
    }
}

using System.IO;
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

        webview.CoreWebView2.WebMessageReceived += (s, e) => {
            webview.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "assets",
                @"C:\Users\Spitz\Desktop\testmd",
                CoreWebView2HostResourceAccessKind.Allow
            );
            webview.CoreWebView2.PostWebMessageAsString(@"breakpoint.bmp");
        };

        var html = await File.ReadAllTextAsync("container.html");
        webview.NavigateToString(html);
    }
}

using System.Collections.Generic;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace WebViewControl {

    partial class WebView {
        private class InternalDialogHandler : DialogHandler {

            private WebView OwnerWebView { get; }
            internal bool WasDialogOpened { get; set; }

            private List<string> _medias;
            internal List<string> FilesPaths {
                get => _medias;
                set {
                    if (_medias?.Count > 0) {
                        WasDialogOpened = false;
                    }

                    _medias = value;
                }
            }
            public InternalDialogHandler(WebView webView) {
                OwnerWebView = webView;
            }

            protected override bool OnFileDialog(CefBrowser browser, CefFileDialogMode mode, string title, string defaultFilePath, string[] acceptFilters, int selectedAcceptFilter, CefFileDialogCallback callback) {
                if (OwnerWebView.DisableFileDialogs) {
                    return true;
                }
                return false;
            }
        }
    }
}

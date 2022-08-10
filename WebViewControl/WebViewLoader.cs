using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using Xilium.CefGlue.Common.Shared;

namespace WebViewControl {

    internal static class WebViewLoader {

        private static string[] CustomSchemes { get; } = new[] {
            ResourceUrl.LocalScheme,
            ResourceUrl.EmbeddedScheme,
            ResourceUrl.CustomScheme,
            Uri.UriSchemeHttp,
            Uri.UriSchemeHttps
        };

        private static GlobalSettings globalSettings;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Initialize(GlobalSettings settings) {
            if (CefRuntimeLoader.IsLoaded) {
                return;
            }

            globalSettings = settings;


            var cefSettings = new CefSettings {
                LogSeverity = string.IsNullOrWhiteSpace(settings.LogFile) ? CefLogSeverity.Disable : (settings.EnableErrorLogOnly ? CefLogSeverity.Error : CefLogSeverity.Verbose),
                LogFile = settings.LogFile,
                UncaughtExceptionStackSize = 100, // enable stack capture
                CachePath = settings.CachePath, // enable cache for external resources to speedup loading
                WindowlessRenderingEnabled = settings.OsrEnabled,
                RemoteDebuggingPort = settings.GetRemoteDebuggingPort(),
                UserAgent = settings.UserAgent,
                CommandLineArgsDisabled = false,
                UserDataPath = settings.CachePath,
                PersistSessionCookies = true,
                PersistUserPreferences = true,
                Locale = CultureInfo.CurrentCulture.IetfLanguageTag,
                MultiThreadedMessageLoop = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            };

            settings.PersistCache = true;

            var customSchemes = CustomSchemes.Select(s => new CustomScheme() {
                SchemeName = s,
                SchemeHandlerFactory = new SchemeHandlerFactory()
            }).ToArray();

            var customFlags = new[] {
                // enable experimental feature flags
                new KeyValuePair<string, string>("enable-experimental-web-platform-features", null),
                new KeyValuePair<string, string>("enable-usermedia-screen-capturing", "1"),
                new KeyValuePair<string, string>("enable-media-stream", "1"),
                new KeyValuePair<string, string>("disable-blink-features", "AutomationControlled"),
                new KeyValuePair<string, string>("use-fake-ui-for-media-stream", "1"),
                new KeyValuePair<string, string>("persist_session_cookies", "1"),
            };

            List<string> args = new List<string>()
            {
                "--disable-blink-features=AutomationControlled"
            };

            CefRuntimeLoader.Initialize(settings: cefSettings, args, flags: customFlags, customSchemes: customSchemes);

            AppDomain.CurrentDomain.ProcessExit += delegate { Cleanup(); };
        }

        /// <summary>
        /// Release all resources and shutdown web view
        /// </summary>
        [DebuggerNonUserCode]
        public static void Cleanup() {
            CefRuntime.Shutdown(); // must shutdown cef to free cache files (so that cleanup is able to delete files)

            if (globalSettings.PersistCache) {
                return;
            }

            try {
                var dirInfo = new DirectoryInfo(globalSettings.CachePath);
                if (dirInfo.Exists) {
                    dirInfo.Delete(true);
                }
            } catch (UnauthorizedAccessException) {
                // ignore
            } catch (IOException) {
                // ignore
            }
        }

    }
}

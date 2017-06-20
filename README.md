# WebView

WebView is a WPF control that wraps CefSharp web view control. Provides the following additional features:
- Strongly-typed javascript evaluation: results of javascript evaluation the appropriate object type
- Scripts are aggregated and executed in bulk for improved performance
- Synchronous javascript evaluation
- Javascript error handling with stack information
- Events to intercept and respond to resources load
- Events to track files download progress
- Ability to load embedded resources using a custom protocol
- Ability to disable history navigation
- Error handling
- Proxy configuration support
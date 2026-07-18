namespace Gym26 {
    public static class UrlGenerator {
        public const string ToggleSidebarName = "toggledSidebar";
        public static string GetUrl(string baseUrl, bool toggledSidebar) {
            return $"{baseUrl}?{ToggleSidebarName}={toggledSidebar}";
        }
    }
}
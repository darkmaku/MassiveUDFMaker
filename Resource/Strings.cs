namespace MassiveUDFMaker.Resource;

public static class Strings
{
    public static class Http
    {
        public const string BASE_URI = "https://{0}:{1}";
    }

    public static class Error
    {
        public const string APPLICATION_SETTINGS_NOT_FOUND = "File 'appsettings.json' not found or is incorrect.";
        public const string HEADER_NOT_FOUND = "Header 'Set-Cookie' not found.";
    }

    public static class Cookie
    {
        public const string HEADER = "Set-Cookie";
        public const string B1_SESSION = "B1SESSION";
        public const string ROUTE_ID = "ROUTEID";
        public const string BASE_PATH = "/";
    }
}
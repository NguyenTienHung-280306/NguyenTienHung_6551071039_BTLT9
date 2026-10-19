namespace SoChiTieu.Configuration
{
    public class AppSettings
    {
        public ConnectionStringsOptions ConnectionStrings { get; set; } = new();
    }

    public class ConnectionStringsOptions
    {
        public string DefaultConnection { get; set; } = string.Empty;
    }
}

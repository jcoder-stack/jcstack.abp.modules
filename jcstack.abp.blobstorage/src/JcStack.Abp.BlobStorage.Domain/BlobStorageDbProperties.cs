namespace JcStack.Abp.BlobStorage;

public static class BlobStorageDbProperties
{
    public static string DbTablePrefix { get; set; } = "FileStorage";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "FileStorage";
}

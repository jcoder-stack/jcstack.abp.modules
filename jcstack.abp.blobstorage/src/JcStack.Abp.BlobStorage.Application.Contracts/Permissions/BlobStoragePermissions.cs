using Volo.Abp.Reflection;

namespace JcStack.Abp.BlobStorage.Permissions;

/// <summary>
/// 文件存储权限定义
/// </summary>
public static class BlobStoragePermissions
{
    public const string GroupName = "FileStorage";

    /// <summary>
    /// 文件操作权限
    /// </summary>
    public static class Files
    {
        public const string Default = GroupName + ".Files";
        public const string Upload = Default + ".Upload";
        public const string Download = Default + ".Download";
        public const string Delete = Default + ".Delete";
        public const string Lookup = Default + ".Lookup";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(BlobStoragePermissions));
    }
}

using JcStack.Abp.BlobStorage.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace JcStack.Abp.BlobStorage.Permissions;

public class FileStoragePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var fileStorageGroup = context.AddGroup(
            BlobStoragePermissions.GroupName,
            L("Permission:FileStorage"));

        var filesPermission = fileStorageGroup.AddPermission(
            BlobStoragePermissions.Files.Default,
            L("Permission:Files"));

        filesPermission.AddChild(
            BlobStoragePermissions.Files.Upload,
            L("Permission:Files.Upload"));

        filesPermission.AddChild(
            BlobStoragePermissions.Files.Download,
            L("Permission:Files.Download"));

        filesPermission.AddChild(
            BlobStoragePermissions.Files.Delete,
            L("Permission:Files.Delete"));

        // Lookup 权限（独立权限，不是 Default 的子权限）
        fileStorageGroup.AddPermission(
            BlobStoragePermissions.Files.Lookup,
            L("Permission:Files.Lookup"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BlobStorageResource>(name);
    }
}

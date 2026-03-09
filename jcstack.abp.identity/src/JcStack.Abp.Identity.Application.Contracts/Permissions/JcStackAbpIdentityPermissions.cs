namespace JcStack.Abp.Identity.Permissions;

public static class JcStackAbpIdentityPermissions
{
    public const string GroupName = "JcStackAbpIdentity";

    public static class OrganizationUnits
    {
        public const string Default = GroupName + ".OrganizationUnits";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Move = Default + ".Move";
        public const string ManageMembers = Default + ".ManageMembers";
        public const string ManageRoles = Default + ".ManageRoles";
        public const string Lookup = Default + ".Lookup";
    }
}

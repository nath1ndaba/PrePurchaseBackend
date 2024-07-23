namespace BackendServices.JWT
{
    public static class AuthRoles
    {
        public const string Admin = "admin";
        public const string Manager = "manager";
        public const string Owner = "owner";
        public const string User = "user";

        public const string RefreshToken = "refresh_token";
    }

    public static class AuthPolicies
    {
        public const string Shop = "shop";

    }

    public static class PrePurchaseJwtConstants
    {
        public const string UserId = "userId";
    }
}

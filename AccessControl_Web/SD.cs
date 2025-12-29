namespace AccessControl_Web
{
    public static class SD
    {
        public static string AccessControlAPIBase { get; set; } = string.Empty;

        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        // Auth Endpoints
        public const string AuthAPIBase = "/api/auth/";
        public const string AuthRegister = AuthAPIBase + "register";
        public const string AuthLogin = AuthAPIBase + "login";

        // User Endpoints
        public const string UserAPIBase = "/api/users/";
        public const string UserCount = UserAPIBase + "count";

        // Group Endpoints
        public const string GroupAPIBase = "/api/groups/";
        public const string GroupUsersCount = GroupAPIBase + "users-count";

        // Visit Log Endpoints
        public const string VisitLogAPIBase = "/api/vist-logs/";
        public const string VisitLogCheckIn = VisitLogAPIBase + "check-in";
        public const string VisitLogCheckOut = VisitLogAPIBase + "check-out/";
        public const string VisitLogActive = VisitLogAPIBase + "active";
    }
}

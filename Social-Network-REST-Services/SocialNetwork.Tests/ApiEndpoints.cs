namespace SocialNetwork.Tests
{
    public static class ApiEndpoints
    {
        #region api/Users
        public const string UserRegister = "/api/users/register";
        public const string UserLogin = "/api/users/login";
        public const string UserLogout = "/api/users/logout";
        public const string UserPreview = "/api/users/{0}";
        public const string UserWall = "/api/users/{0}/wall";
        public const string UserFriends = "/api/users/{0}/friends";
        public const string UserFriendsPreview = "/api/users/{0}/friends/preview";
        #endregion

        #region api/me
        public const string GetOwnProfileInfo = "/api/me";
        public const string EditProfileInfo = "/api/me";
        public const string ChangePassword = "/api/me/changepassword";
        public const string GetOwnFriends = "/api/me/friends";
        public const string ChangeRequestStatus = "/api/me/requests/{0}?status={1}";
        public const string GetFriendRequests = "/api/me/requests";
        public const string SendFriendRequest = "/api/me/requests/{0}";
        #endregion

        #region api/posts
        public const string GetPostById = "/api/posts/{id}";
        public const string EditPostById = "/api/posts/{id}";
        public const string DeletePostById = "/api/posts/{id}";
        #endregion

        #region api/posts/{id}/comments
        public const string GetCommentsByPostId = "/api/posts/{id}/comments";
        public const string AddCommentToPost = "/api/posts/{id}/comments";
        public const string EditPostComment = "/api/posts/{id}/comments/{commentId}";
        public const string DeletePostComment = "/api/posts/{id}/comments/{commentId}";
        #endregion
    }
}

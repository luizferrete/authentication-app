namespace AuthenticationApp.Interfaces.Business
{
    public interface IUserContextService
    {
        /// Gets the current user's username.
        /// </summary>
        /// <returns>The username of the current user.</returns>
        string UserName { get; }
        /// <summary>
        /// Gets the IP address of the user.
        /// </summary>
        /// <returns>The IP address of the user.</returns>
        string UserIpAddress { get; }
    }
}

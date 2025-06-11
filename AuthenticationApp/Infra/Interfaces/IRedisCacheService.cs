namespace AuthenticationApp.Infra.Interfaces
{
    public interface IRedisCacheService
    {
        /// <summary>
        /// Grava uma string no cache com chave e TTL informados.
        /// </summary>
        Task SetAsync(string key, string value, TimeSpan? expiration = null);

        /// <summary>
        /// Lê uma string do cache pela chave. Retorna null se não existir.
        /// </summary>
        Task<string?> GetAsync(string key);

        /// <summary>
        /// Remove do cache o item identificado pela chave.
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Faz logout em massa do usuário (remove todas as sessões desse email+IP).
        /// </summary>
        Task MassLogoutAsync(string email, string ip);
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationApp.Infra;
using AuthenticationApp.Infra.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace AuthenticationApp.Infra.Tests
{
    public class RedisCacheServiceTests
    {
        private readonly Mock<IDistributedCache> _distributedCacheMock;
        private readonly Mock<IConnectionMultiplexer> _multiplexerMock;
        private readonly Mock<IServer> _serverMock;
        private readonly RedisCacheService _cacheService;

        public RedisCacheServiceTests()
        {
            _distributedCacheMock = new Mock<IDistributedCache>();
            _multiplexerMock = new Mock<IConnectionMultiplexer>();
            _serverMock = new Mock<IServer>();

            // Configura o multiplexer para retornar nosso servidor mockado
            var endpoint = new DnsEndPoint("localhost", 6379);
            _multiplexerMock
                .Setup(m => m.GetEndPoints(It.IsAny<bool>()))
                .Returns(new EndPoint[] { endpoint });
            _multiplexerMock
                .Setup(m => m.GetServer(endpoint, It.IsAny<object>()))
                .Returns(_serverMock.Object);

            // Default setups para garantir que as extensões funcionem sem NullRef
            _distributedCacheMock
                .Setup(c => c.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _distributedCacheMock
                .Setup(c => c.GetAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);
            _distributedCacheMock
                .Setup(c => c.RemoveAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _cacheService = new RedisCacheService(
                _distributedCacheMock.Object,
                _multiplexerMock.Object
            );
        }

        [Fact]
        public async Task SetAsync_WithExplicitExpiration_UsesProvidedExpiration()
        {
            // Arrange
            var key = "mykey";
            var value = "myvalue";
            var expiration = TimeSpan.FromMinutes(5);

            _distributedCacheMock
                .Setup(c => c.SetAsync(
                    key,
                    It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == value),
                    It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == expiration),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _cacheService.SetAsync(key, value, expiration);

            // Assert
            _distributedCacheMock.Verify();
        }

        [Fact]
        public async Task SetAsync_WithoutExpiration_UsesDefaultExpiration()
        {
            // Arrange
            var key = "key";
            var value = "value";
            var defaultExpiration = TimeSpan.FromHours(8);

            _distributedCacheMock
                .Setup(c => c.SetAsync(
                    key,
                    It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == value),
                    It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == defaultExpiration),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _cacheService.SetAsync(key, value);

            // Assert
            _distributedCacheMock.Verify();
        }

        [Fact]
        public async Task GetAsync_ReturnsValueFromCache()
        {
            // Arrange
            var key = "anotherKey";
            var expected = "val";

            // Mocka o GetAsync (bytes) que será usado pela extensão GetStringAsync
            _distributedCacheMock
                .Setup(c => c.GetAsync(
                    key,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(expected))
                .Verifiable();

            // Act
            var result = await _cacheService.GetAsync(key);

            // Assert
            Assert.Equal(expected, result);
            _distributedCacheMock.Verify();
        }

        [Fact]
        public async Task RemoveAsync_CallsCacheRemove()
        {
            // Arrange
            var key = "removeKey";

            _distributedCacheMock
                .Setup(c => c.RemoveAsync(
                    key,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _cacheService.RemoveAsync(key);

            // Assert
            _distributedCacheMock.Verify();
        }

        [Fact]
        public async Task MassLogoutAsync_WithExistingRefreshToken_RemovesRefreshAndSessionKeys()
        {
            // Arrange
            var email = "test@test.com";
            var ip = "1.2.3.4";
            const string prefix = "AuthenticationApp:";
            var fullKey = $"{prefix}loggedUser:{email}:{ip}session1";
            var cacheKey = fullKey[prefix.Length..];
            var refreshToken = "token123";

            // Mocka a varredura de chaves
            _serverMock
                .Setup(s => s.KeysAsync(
                    It.IsAny<int>(),
                    It.Is<RedisValue>(v => v == $"{prefix}loggedUser:{email}:{ip}*"),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<CommandFlags>()))
                .Returns(GetAsyncEnumerable(new[] { (RedisKey)fullKey }));

            // Mocka a leitura do token (GetAsync em bytes)
            _distributedCacheMock
                .Setup(c => c.GetAsync(
                    cacheKey,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(refreshToken))
                .Verifiable();

            // Espera remoção de ambos: refresh:{token} e session key
            _distributedCacheMock
                .Setup(c => c.RemoveAsync(
                    $"refresh:{refreshToken}",
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _distributedCacheMock
                .Setup(c => c.RemoveAsync(
                    cacheKey,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _cacheService.MassLogoutAsync(email, ip);

            // Assert
            _distributedCacheMock.Verify(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
            _distributedCacheMock.Verify(c => c.RemoveAsync($"refresh:{refreshToken}", It.IsAny<CancellationToken>()), Times.Once);
            _distributedCacheMock.Verify(c => c.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MassLogoutAsync_WithoutRefreshToken_OnlyRemovesSessionKey()
        {
            // Arrange
            var email = "user@domain.com";
            var ip = "5.6.7.8";
            const string prefix = "AuthenticationApp:";
            var fullKey = $"{prefix}loggedUser:{email}:{ip}session2";
            var cacheKey = fullKey[prefix.Length..];

            _serverMock
                .Setup(s => s.KeysAsync(
                    It.IsAny<int>(),
                    It.Is<RedisValue>(v => v == $"{prefix}loggedUser:{email}:{ip}*"),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<CommandFlags>()))
                .Returns(GetAsyncEnumerable(new[] { (RedisKey)fullKey }));

            // Retorna null (token não encontrado)
            _distributedCacheMock
                .Setup(c => c.GetAsync(
                    cacheKey,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null)
                .Verifiable();

            // Só remove a chave de sessão
            _distributedCacheMock
                .Setup(c => c.RemoveAsync(
                    cacheKey,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _cacheService.MassLogoutAsync(email, ip);

            // Assert
            _distributedCacheMock.Verify(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
            _distributedCacheMock.Verify(c => c.RemoveAsync(
                It.Is<string>(k => k.StartsWith("refresh:")), It.IsAny<CancellationToken>()), Times.Never);
            _distributedCacheMock.Verify(c => c.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        }

        // Helper para simular o IAsyncEnumerable<RedisKey>
        private static async IAsyncEnumerable<RedisKey> GetAsyncEnumerable(IEnumerable<RedisKey> keys)
        {
            foreach (var key in keys)
                yield return key;
        }
    }
}

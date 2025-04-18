using AuthenticationApp.Utils.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApp.Tests.Service
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_GenerateDifferentHashesForSamePassword()
        {
            // Arrange
            string password = "TestPassword123";
            // Act
            string hash1 = PasswordHasher.HashPassword(password);
            string hash2 = PasswordHasher.HashPassword(password);
            // Assert
            Assert.NotEqual(hash1, hash2);

            Assert.Equal(3, hash1.Split('.').Length);
            Assert.Equal(3, hash2.Split('.').Length);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {  
            // Arrange
            string password = "TestPassword123";
            string hash = PasswordHasher.HashPassword(password);
            // Act
            bool result = PasswordHasher.VerifyPassword(password, hash);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            string password = "TestPassword123";
            string incorrect = "WrongPassword123";
            string hash = PasswordHasher.HashPassword(password);
            // Act
            bool result = PasswordHasher.VerifyPassword(incorrect, hash);
            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("algoritmo_invalido$blabla$#")]
        [InlineData("1000invalidBase64$%!")]
        public void VerifyPassword_InvalidHashFormat_ReturnsFalse(string malformattedHash)
        {
            // Arrange
            string password = "TestPassword123";
            // Act
            bool result = PasswordHasher.VerifyPassword(password, malformattedHash);
            // Assert
            Assert.False(result);
        }
    }
}

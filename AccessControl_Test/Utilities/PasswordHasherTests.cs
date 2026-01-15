using AccessControl_API.Utilities;

namespace AccessControl_Test.Utilities
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_ValidPassword_ReturnsNonEmptyString()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash = PasswordHasher.Hash(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Hash_SamePassword_ReturnsSameHash()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash1 = PasswordHasher.Hash(password);
            var hash2 = PasswordHasher.Hash(password);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Hash_DifferentPasswords_ReturnsDifferentHashes()
        {
            // Arrange
            var password1 = "TestPassword123";
            var password2 = "DifferentPassword456";

            // Act
            var hash1 = PasswordHasher.Hash(password1);
            var hash2 = PasswordHasher.Hash(password2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Hash_EmptyString_ReturnsHash()
        {
            // Arrange
            var password = "";

            // Act
            var hash = PasswordHasher.Hash(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Hash_CaseSensitive_ReturnsDifferentHashes()
        {
            // Arrange
            var password1 = "Password";
            var password2 = "password";

            // Act
            var hash1 = PasswordHasher.Hash(password1);
            var hash2 = PasswordHasher.Hash(password2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "TestPassword123";
            var hash = PasswordHasher.Hash(password);

            // Act
            var result = PasswordHasher.Verify(password, hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Verify_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var correctPassword = "TestPassword123";
            var incorrectPassword = "WrongPassword456";
            var hash = PasswordHasher.Hash(correctPassword);

            // Act
            var result = PasswordHasher.Verify(incorrectPassword, hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Verify_EmptyPassword_ReturnsFalse()
        {
            // Arrange
            var password = "TestPassword123";
            var hash = PasswordHasher.Hash(password);

            // Act
            var result = PasswordHasher.Verify("", hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Verify_CaseSensitive_ReturnsFalse()
        {
            // Arrange
            var password = "Password";
            var hash = PasswordHasher.Hash(password);

            // Act
            var result = PasswordHasher.Verify("password", hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Verify_EmptyHash_ReturnsFalse()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var result = PasswordHasher.Verify(password, "");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Hash_SpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var password = "P@ssw0rd!#$%^&*()";

            // Act
            var hash = PasswordHasher.Hash(password);
            var isValid = PasswordHasher.Verify(password, hash);

            // Assert
            Assert.NotNull(hash);
            Assert.True(isValid);
        }

        [Fact]
        public void Hash_LongPassword_HandlesCorrectly()
        {
            // Arrange
            var password = new string('a', 1000);

            // Act
            var hash = PasswordHasher.Hash(password);
            var isValid = PasswordHasher.Verify(password, hash);

            // Assert
            Assert.NotNull(hash);
            Assert.True(isValid);
        }

        [Fact]
        public void Hash_UnicodeCharacters_HandlesCorrectly()
        {
            // Arrange
            var password = "??????123??";

            // Act
            var hash = PasswordHasher.Hash(password);
            var isValid = PasswordHasher.Verify(password, hash);

            // Assert
            Assert.NotNull(hash);
            Assert.True(isValid);
        }

        [Fact]
        public void Hash_ReturnsBase64EncodedString()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash = PasswordHasher.Hash(password);

            // Assert - Try to decode as Base64
            var isBase64 = TryConvertFromBase64(hash, out _);
            Assert.True(isBase64);
        }

        [Fact]
        public void Hash_ConsistentHashLength()
        {
            // Arrange
            var shortPassword = "123";
            var longPassword = "ThisIsAVeryLongPasswordWithManyCharacters123456789";

            // Act
            var hash1 = PasswordHasher.Hash(shortPassword);
            var hash2 = PasswordHasher.Hash(longPassword);

            // Assert - SHA256 always produces same length output
            Assert.Equal(hash1.Length, hash2.Length);
        }

        private bool TryConvertFromBase64(string base64String, out byte[] result)
        {
            try
            {
                result = Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                result = Array.Empty<byte>();
                return false;
            }
        }
    }
}

using AccessControl_API.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace AccessControl_Test.Authorization
{
    public class PermissionRequirementTests
    {
        [Fact]
        public void Constructor_ValidPermission_SetsPermissionProperty()
        {
            // Arrange
            var permissionName = "READ_USERS";

            // Act
            var requirement = new PermissionRequirement(permissionName);

            // Assert
            Assert.Equal(permissionName, requirement.Permission);
        }

        [Fact]
        public void Constructor_EmptyPermission_SetsEmptyString()
        {
            // Arrange
            var permissionName = "";

            // Act
            var requirement = new PermissionRequirement(permissionName);

            // Assert
            Assert.Equal("", requirement.Permission);
        }

        [Fact]
        public void Constructor_NullPermission_SetsNull()
        {
            // Arrange
            string? permissionName = null;

            // Act
            var requirement = new PermissionRequirement(permissionName!);

            // Assert
            Assert.Null(requirement.Permission);
        }

        [Fact]
        public void PermissionRequirement_ImplementsIAuthorizationRequirement()
        {
            // Arrange
            var requirement = new PermissionRequirement("TEST");

            // Assert
            Assert.IsAssignableFrom<IAuthorizationRequirement>(requirement);
        }

        [Fact]
        public void Constructor_PermissionWithSpaces_PreservesSpaces()
        {
            // Arrange
            var permissionName = "READ USERS";

            // Act
            var requirement = new PermissionRequirement(permissionName);

            // Assert
            Assert.Equal("READ USERS", requirement.Permission);
        }

        [Fact]
        public void Constructor_PermissionWithSpecialCharacters_PreservesCharacters()
        {
            // Arrange
            var permissionName = "READ_USERS:ALL";

            // Act
            var requirement = new PermissionRequirement(permissionName);

            // Assert
            Assert.Equal("READ_USERS:ALL", requirement.Permission);
        }

        [Fact]
        public void Permission_IsReadOnly()
        {
            // Arrange
            var requirement = new PermissionRequirement("READ_USERS");

            // Assert - Verify that Permission property has no setter
            var propertyInfo = typeof(PermissionRequirement).GetProperty("Permission");
            Assert.NotNull(propertyInfo);
            Assert.Null(propertyInfo!.SetMethod);
        }

        [Fact]
        public void Constructor_DifferentPermissions_CreatesDistinctObjects()
        {
            // Arrange & Act
            var requirement1 = new PermissionRequirement("READ_USERS");
            var requirement2 = new PermissionRequirement("WRITE_USERS");

            // Assert
            Assert.NotEqual(requirement1.Permission, requirement2.Permission);
        }

        [Fact]
        public void Constructor_SamePermission_CreatesObjectsWithSameValue()
        {
            // Arrange & Act
            var requirement1 = new PermissionRequirement("READ_USERS");
            var requirement2 = new PermissionRequirement("READ_USERS");

            // Assert
            Assert.Equal(requirement1.Permission, requirement2.Permission);
            Assert.NotSame(requirement1, requirement2); // Different instances
        }

        [Fact]
        public void Constructor_LongPermissionName_HandlesCorrectly()
        {
            // Arrange
            var longPermission = new string('A', 500);

            // Act
            var requirement = new PermissionRequirement(longPermission);

            // Assert
            Assert.Equal(longPermission, requirement.Permission);
            Assert.Equal(500, requirement.Permission.Length);
        }
    }
}

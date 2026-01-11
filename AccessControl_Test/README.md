# AccessControl API Unit Tests

This project contains comprehensive unit tests for the AccessControl API, covering services and controllers.

## Test Coverage

### Services
- **AuthService** (8 tests)
  - Email existence validation
  - User registration (valid, duplicate email, invalid group)
  - User login (valid credentials, invalid email, invalid password)

### Controllers
- **AuthController** (6 tests)
  - Registration endpoint (valid, duplicate email, invalid group, registration failure)
  - Login endpoint (valid credentials, invalid credentials)

- **UserController** (12 tests)
  - Create user (valid, null input, invalid group, duplicate email, duplicate ID number, default password)
  - Update user (valid, user not found)
  - Delete user (valid, user not found)
  - Get all users
  - Get user count

## Technologies Used

- **xUnit** - Testing framework
- **Moq** - Mocking framework for dependencies
- **EF Core InMemory** - In-memory database for testing
- **AutoMapper** - Object-to-object mapping

## Running the Tests

### From Command Line

```bash
# Run all tests
dotnet test AccessControl_Test/AccessControl_Test.csproj

# Run with detailed output
dotnet test AccessControl_Test/AccessControl_Test.csproj --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter FullyQualifiedName~AuthServiceTests

# Run specific test method
dotnet test --filter FullyQualifiedName~AuthServiceTests.RegisterAsync_ValidRequest_ReturnsUserDTO
```

### From Visual Studio

1. Open **Test Explorer** (Test > Test Explorer)
2. Click **Run All** to run all tests
3. Or right-click individual tests/classes to run specific tests

### From Visual Studio Code

1. Install the .NET Core Test Explorer extension
2. Tests will appear in the Test Explorer panel
3. Click the play button to run tests

## Test Results

All 26 tests passing ?

```
Test summary: total: 26, failed: 0, succeeded: 26, skipped: 0
```

## Test Structure

Each test follows the **Arrange-Act-Assert** pattern:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and mocks
    var testData = new SomeData();
    
    // Act - Execute the method being tested
    var result = await _service.MethodUnderTest(testData);
    
    // Assert - Verify the results
    Assert.Equal(expected, result);
}
```

## Key Testing Patterns

### 1. Using In-Memory Database

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;

_context = new AppDbContext(options);
```

Each test gets its own isolated database instance.

### 2. Mocking Dependencies

```csharp
_mockMapper = new Mock<IMapper>();
_mockMapper.Setup(m => m.Map<UserDTO>(It.IsAny<User>()))
    .Returns(expectedUserDTO);
```

### 3. Testing Controllers

```csharp
var result = await _controller.CreateUser(userDto);

var actionResult = Assert.IsType<OkObjectResult>(result.Result);
var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
Assert.Equal(201, response.StatusCode);
```

### 4. Testing Services

```csharp
var result = await _authService.RegisterAsync(registrationRequest);

Assert.NotNull(result);
Assert.Equal(expectedEmail, result.Email);
```

## Adding New Tests

### 1. Create a new test class

```csharp
namespace AccessControl_Test.Controllers
{
    public class YourControllerTests
    {
        private readonly YourController _controller;
        
        public YourControllerTests()
        {
            // Setup dependencies
        }
        
        [Fact]
        public async Task YourTest()
        {
            // Arrange
            // Act
            // Assert
        }
    }
}
```

### 2. Run the new tests

```bash
dotnet test --filter FullyQualifiedName~YourControllerTests
```

## Continuous Integration

These tests can be integrated into your CI/CD pipeline:

```yaml
# Example for GitHub Actions
- name: Run tests
  run: dotnet test --configuration Release --logger "trx;LogFileName=test-results.trx"
  
- name: Publish test results
  uses: dorny/test-reporter@v1
  if: always()
  with:
    name: Test Results
    path: '**/*.trx'
    reporter: dotnet-trx
```

## Best Practices Followed

1. ? **Isolated Tests** - Each test is independent and uses its own database
2. ? **Descriptive Names** - Test names clearly describe what is being tested
3. ? **Arrange-Act-Assert** - Consistent structure across all tests
4. ? **Mock External Dependencies** - Only test the unit being tested
5. ? **Test Both Success and Failure** - Cover happy path and edge cases
6. ? **Clean Up Resources** - IDisposable pattern for database cleanup

## Troubleshooting

### Tests fail with database connection errors

**Solution**: Tests use in-memory databases, no SQL Server connection needed.

### Tests fail to build

**Solution**: Ensure all NuGet packages are restored:
```bash
dotnet restore
```

### Mock setup errors

**Solution**: Verify that the method signatures match between the mock setup and actual implementation.

## Future Enhancements

- [ ] Add integration tests
- [ ] Add tests for GroupsController
- [ ] Add tests for VisitLogsController
- [ ] Add code coverage reporting
- [ ] Add performance tests

## Contributing

When adding new features to the API:

1. Write tests first (TDD approach)
2. Ensure all tests pass before committing
3. Maintain at least 80% code coverage
4. Follow the existing test patterns

## License

This project is part of the AccessControl system. See the main project for license information.

# Firebase Unity WebGL Tests

This directory contains comprehensive unit and integration tests for the Firebase Unity WebGL wrapper.

## Test Structure

### Assembly Definitions
- **Firebase.asmdef**: Main Firebase library assembly (in Runtime/)
- **Firebase.Editor.Tests.asmdef**: Edit mode unit tests assembly  
- **Firebase.Runtime.Tests.asmdef**: Play mode integration tests assembly

### Editor Tests (Unit Tests)
Located in `Editor/` directory. These tests run quickly in the Unity Editor without requiring runtime:

- **FirebaseConfigTests**: Tests Firebase configuration validation and URL generation
- **FirebaseUtilsTests**: Tests utility functions like JSON serialization, URL encoding, query parameters
- **FirebaseExceptionTests**: Tests custom Firebase exception handling
- **FirebaseAuthTests**: Tests authentication flows with mocked HTTP responses

### Runtime Tests (Integration Tests)
Located in `Runtime/` directory. These tests run during Unity runtime and can test Unity-specific functionality:

- **FirebaseIntegrationTests**: Tests full Firebase service workflows, async/await patterns, and Unity integration

## Test Infrastructure

### Mock System
- **MockHttpClient**: Replaces UnityWebRequest for testing, allows configuring responses
- **IHttpClient Interface**: Abstraction layer enabling dependency injection for testing

### Test Utilities
- **FirebaseTestData**: Predefined test data including valid/invalid configurations, mock responses
- **TestHelpers**: Common assertion helpers for HTTP requests, JSON validation, error scenarios

### Key Testing Patterns

#### Dependency Injection
```csharp
var mockClient = new MockHttpClient();
var auth = new FirebaseAuth(config, mockClient); // Inject mock for testing
```

#### Response Configuration
```csharp
mockClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
mockClient.ConfigureDatabaseResponse(FirebaseTestData.DatabaseGetResponse);
```

#### Request Verification
```csharp
var request = mockClient.GetLastRequest();
TestHelpers.AssertValidHttpRequest(request, "POST");
TestHelpers.AssertUrlContains(request.Url, "identitytoolkit.googleapis.com");
```

#### Async Testing in Play Mode
```csharp
[UnityTest]
public IEnumerator MyAsyncTest()
{
    var task = service.DoSomethingAsync();
    yield return new WaitUntil(() => task.IsCompleted);
    Assert.IsFalse(task.IsFaulted);
}
```

## Running Tests

### In Unity Editor
1. Open Unity Test Runner (Window > General > Test Runner)
2. Switch to EditMode tab for unit tests (Tests/Editor)
3. Switch to PlayMode tab for integration tests (Tests/Runtime)
4. Click "Run All" or select specific tests

### Command Line (Headless)
```bash
# Run Editor tests
Unity -runTests -batchmode -quit -testPlatform EditMode -testResults results.xml

# Run Runtime tests  
Unity -runTests -batchmode -quit -testPlatform PlayMode -testResults results.xml
```

## Test Coverage

### Core Functionality
- ✅ Firebase configuration validation
- ✅ JSON serialization/deserialization
- ✅ URL encoding and query parameter building
- ✅ Random string generation and timestamp utilities
- ✅ Error handling and exception creation

### Authentication
- ✅ Email/password sign in
- ✅ Anonymous authentication
- ✅ User creation
- ✅ Password reset
- ✅ Token management
- ✅ State change events
- ✅ Sign out functionality

### HTTP Layer
- ✅ Request/response handling
- ✅ Mock client for testing
- ✅ Unity WebRequest integration
- ✅ Error response mapping

### Integration Scenarios
- ✅ Full authentication workflows
- ✅ Async/await patterns in Unity runtime
- ✅ Event handling and callbacks
- ✅ Unity-specific functionality

## Best Practices

### Test Organization
- Keep unit tests fast and focused
- Use integration tests for complex workflows
- Mock external dependencies consistently
- Test both success and error scenarios

### Naming Conventions
- `MethodName_Scenario_ExpectedResult`
- Example: `SignInAsync_ValidCredentials_ReturnsUser`

### Error Testing
```csharp
TestHelpers.AssertFirebaseException(async () => {
    await auth.SignInAsync(invalidEmail, password);
}, "invalid_email");
```

### Mock Configuration
```csharp
// Configure successful response
mockClient.ConfigureFirebaseAuthResponse(validResponse);

// Configure error response  
mockClient.ConfigureFirebaseAuthResponse(errorResponse, isSuccess: false);
```

## Adding New Tests

### For New Firebase Services
1. Create service-specific test file in `EditMode/`
2. Add integration tests in `PlayMode/`
3. Create mock responses in `FirebaseTestData`
4. Add test utilities in `TestHelpers` if needed

### For New Features
1. Write unit tests first (TDD approach)
2. Mock HTTP responses for the feature
3. Test error scenarios
4. Add integration test for end-to-end workflow

This testing infrastructure ensures the Firebase Unity WebGL wrapper is reliable, maintainable, and works correctly across different Unity platforms.
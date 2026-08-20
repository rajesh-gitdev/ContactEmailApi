using ContactEmailApi.Shared.Models;
using Xunit;

namespace ContactEmailApi.UnitTests;

public sealed class ApiResponseTests
{
    [Fact]
    public void Ok_SetsSuccessAndStatusCode()
    {
        var response = ApiResponse.Ok("done", statusCode: 200, requestId: "abc");

        Assert.True(response.Success);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("done", response.Message);
        Assert.Equal("abc", response.RequestId);
        Assert.Null(response.Errors);
    }

    [Fact]
    public void Fail_CarriesErrorsAndStatusCode()
    {
        var errors = new[] { "Email is required.", "Message must contain at least 20 characters." };

        var response = ApiResponse.Fail("Validation failed.", statusCode: 422, errors: errors);

        Assert.False(response.Success);
        Assert.Equal(422, response.StatusCode);
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors!.Count);
    }

    [Fact]
    public void GenericOk_CarriesTypedData()
    {
        var response = ApiResponse<int>.Ok(42, "ok");

        Assert.True(response.Success);
        Assert.Equal(42, response.Data);
    }
}

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Salvation.Lambda.Annotations;

public class Function
{
    /// <summary>
    ///  https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/LowLevelDotNetItemsExample.html
    /// </summary>
    private readonly DynamoDBContext _dynamoDBContext;
    public Function()
    {
        _dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [LambdaFunction()]
    [HttpApi(LambdaHttpMethod.Get, "/users/{userId}")]
    public async Task<User> FunctionHandler(string userId, ILambdaContext context)
    {
        // request.PathParameters.TryGetValue("userId", out var userIdString);
        Guid.TryParse(userId, out var id);
        var user = await _dynamoDBContext.LoadAsync<User>(id);
        return user;
    }

    [LambdaFunction()]
    [HttpApi(LambdaHttpMethod.Post, "/users")]
    public async Task PostFunctionHandler([FromBody] User user, ILambdaContext context)
    {
        await _dynamoDBContext.SaveAsync<User>(user);
    }

    [LambdaFunction()]
    [HttpApi(LambdaHttpMethod.Delete, "/users/{userId}")]
    public async Task DeleteFunctionHandler(string userId, ILambdaContext context)
    {
        Guid.TryParse(userId, out var id);
        await _dynamoDBContext.DeleteAsync<User>(id);
    }

    [LambdaFunction()]
    [HttpApi(LambdaHttpMethod.Put, "/users")]
    public async Task UpdateFunctionHandler([FromBody] User user,  ILambdaContext context)
    {
        var existingUser = await _dynamoDBContext.LoadAsync<User>(user.Id);
        if ( existingUser != null)
        {
            await _dynamoDBContext.SaveAsync<User>(user);
        }
    }

    private static APIGatewayHttpApiV2ProxyResponse OkResponse()
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = 200,
        };
    }

    private static APIGatewayHttpApiV2ProxyResponse BadReponse(string message)
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = message,
            StatusCode = 400,
        };
    }
}


public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

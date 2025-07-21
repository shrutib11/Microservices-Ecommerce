namespace UserService.API.GrpcServices;
using Grpc.Core;
using Microservices.Shared.Protos;
using UserService.Application.Interfaces;

public class UserGrpcService : User.UserBase
{
    private readonly IUserService _userService;

    public UserGrpcService(IUserService userService)
    {
        _userService = userService;
    }

    public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        var user = await _userService.GetUserById(request.UserId);
        if (user == null)
            return new GetUserByIdResponse { IsFound = false };
            
        return new GetUserByIdResponse
        {
            UserId = user.Id,
            IsFound = true
        };
    }
}

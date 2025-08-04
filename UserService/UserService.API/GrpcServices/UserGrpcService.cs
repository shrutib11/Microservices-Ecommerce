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
            IsFound = true,
            UserName = user.FirstName + " " + user.LastName,
            UserImage = user.ProfileImage
        };
    }

    public override async Task<GetUsersByIdsResponse> GetUsersByIds(GetUsersByIdsRequest request, ServerCallContext context)
    {
        var userIds = request.UserIds.ToList();
        var users = await _userService.GetUsersByIds(userIds);

        var response = new GetUsersByIdsResponse();

        foreach (var user in users)
        {
            response.Users.Add(new GetUserByIdResponse
            {
                UserId = user.Id,
                UserName = user.FirstName + " " + user.LastName,
                UserImage = user.ProfileImage,
                IsFound = true
            });
        }

        var foundIds = users.Select(u => u.Id).ToHashSet();
        var missingIds = userIds.Except(foundIds);

        foreach (var missingId in missingIds)
        {
            response.Users.Add(new GetUserByIdResponse
            {
                UserId = missingId,
                IsFound = false
            });
        }

        return response;
    }
}

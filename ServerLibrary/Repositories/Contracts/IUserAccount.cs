using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IUserAccount
    {
        Task<GeneralResponse> CreateAsync(Register User);
        Task<LoginResponse> SignInAsync(Login User);
        Task<LoginResponse> RefreshTokenAsync(RefreshToken token);
    }
}

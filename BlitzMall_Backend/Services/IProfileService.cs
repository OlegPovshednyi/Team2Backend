using BlitzMall_Backend.DTOs.User;

namespace BlitzMall_Backend.Services
{
    public interface IProfileService
    {
        Task<ProfileDto> GetMyProfileAsync();
        Task<ProfileDto> UpdateMyProfileAsync(UpdateProfileDto dto);
    }
}

using System.ComponentModel.DataAnnotations;

namespace SuperGamesApi.Models.Requests
{
    public class LoginUserRequest
    {
        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}

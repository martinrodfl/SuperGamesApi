
using SuperGamesApi.Models.Requests;

namespace SuperGamesApi.Models.Responses
{
    public class LoginUserResponse
    {
        public int Status { get; set; }
        public string? Token { get; set; }
        public UserLoginData? User { get; set; }
        public List<int>? MyGames { get; set; } = new List<int>();

    }
}

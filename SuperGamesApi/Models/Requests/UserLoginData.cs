namespace SuperGamesApi.Models.Requests
{
    public class UserLoginData
    {
        //Clase para LoginResponse - datos mapeados para no entregar todo el usuario

        public int? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}

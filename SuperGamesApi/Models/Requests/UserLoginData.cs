namespace SuperGamesApi.Models.Requests
{
    public class UserLoginData
    {
        //Clase para usar con LoginResponse - datos mapeados para no entregar todo el usuario

        public int? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SuperGamesApi.Contexts;
using SuperGamesApi.Helpers;
using SuperGamesApi.Models;
using SuperGamesApi.Models.Requests;
using SuperGamesApi.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace SuperGamesApi.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly SuperGamesContext _SuperGamesContext;
        private readonly IConfiguration _config;
        public AuthController(SuperGamesContext userDBContext, IConfiguration config)
        {
            _SuperGamesContext = userDBContext;
            _config = config;
        }

        //---------------------LOGIN--------------------------
        /// <summary>
        /// Loguear un Usuario.
        /// </summary>
        /// <returns>Usuario logueado</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "email": "correo@correo.com",
        ///         "password": "P@ssw0rd"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">El usuario se logue correctamente</response>
        /// <response code="400">Error en los datos de login</response>
        /// <response code="404">No existe el ususario o las credenciales son incorrectas</response> 

        [HttpPost("login")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(LoginUserResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
        {
            // Validar que ningun campo este vacio
            if (request.Email.Length < 1 || request.Password.Length < 1)
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "All fields are required" });
            }
            // Validar email
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "Invalid Email" });
            }

            // Validar contraseña
            if (!IsValidPassword(request.Password))
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "Invalid Password" });
            }

            var encryptedpass = Encrypt.GetSHA256(request.Password);
            var user = await _SuperGamesContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == encryptedpass);

            if (user == null)
            {
                return NotFound(new ErrorResponse { Status = 404, Message = "User does not exist or Incorrect Credentials" });
            }

            var tokenString = GenerateTokenString(user);

            var myGames = await _SuperGamesContext.GameIds.Where(g => g.UserId == user.Id)
                           .Select(g => g.GameId)
                           .ToListAsync();

            return Ok(new LoginUserResponse
            {
                Status = 200,
                Token = tokenString,
                User = user.GetTokenAndName(),
                MyGames = myGames
            });
        }

        //---------------------REGISTER--------------------------
        /// <summary>
        /// Crear un nuevo Usuario.
        /// </summary>
        /// <returns>Un nuevo Usuario</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "firstName": "Martin",
        ///         "lastName": "Rodriguez",
        ///         "email": "correo@correo.com",
        ///         "password": "P@ssw0rd",
        ///         "confirmPassword": "P@ssw0rd"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Crea el Usuario y devuelve LoginUserResponse</response>
        /// <response code="400">No se pudo crear el Usuario</response>
        /// 
        [HttpPost("register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(LoginUserResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest user)
        {

            // Validar todos los campos
            if (user.Email.Length == 0 || user.Password.Length == 0 || user.FirstName.Length == 0 || user.LastName.Length == 0 || user.ConfirmPassword.Length == 0)
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "All fields are required" });
            }

            // Validar que nombre y apellido tengan mas de 2 caracteres
            if (!IsValidFirstAndLastName(user.FirstName) || !IsValidFirstAndLastName(user.LastName))
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "First./Lastname 2 characters minimum" });
            }

            // Validar si email existe
            var emailExist = EmailExists(user.Email);

            if (emailExist)
            {
                return Unauthorized(new ErrorResponse { Status = 401, Message = "This email already exists" });
            }

            // Validar email
            if (!IsValidEmail(user.Email))
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "Invalid Email" });
            }

            // Validar contraseña
            if (!IsValidPassword(user.Password))
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "Invalid Password" });
            }


            // validar que password y con firmacion son iguales
            if (user.Password != user.ConfirmPassword)
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "Password and Password confirmation do not match" });
            }

            // -> Add new user
            UserEntity? result = await _SuperGamesContext.Add(user);

            //-> continuous whit login
            var encryptedpass = Encrypt.GetSHA256(user.Password);

            var userRegistered = await _SuperGamesContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email && u.Password == encryptedpass);

            if (userRegistered == null )
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "Password and Password confirmation do not match" });
            }

            var tokenString = GenerateTokenString(userRegistered);

            return new CreatedResult($"https://localhost:7122/api/user/{userRegistered?.Id}", new LoginUserResponse
            {
                Status = 201,
                Token = tokenString,
                User = userRegistered?.GetTokenAndName(),
            });
        }


        private string GenerateTokenString(UserEntity user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FirstName),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));

            var signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var securityToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(6000),
                issuer: _config.GetSection("Jwt:Issuer").Value,
                audience: _config.GetSection("Jwt:Audience").Value,
                signingCredentials: signingCred
                );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);
            return tokenString;

        }


        // VERIFICACIONES
        public static bool IsValidFirstAndLastName(string name)
        {
            // Expresión regular para validar nombres con un espacio opcional entre el primer y el segundo nombre
            string pattern = @"^[a-zA-Z]+(?: [a-zA-Z]+)?$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(name) && name.Length >= 2 && name.Length <= 20;
        }
        public bool EmailExists(string? email)
        {
            return _SuperGamesContext.Users.Any(e => e.Email == email);
        }

        public static bool IsValidEmail(string email)
        {
            // Expresión regular para validar email
            string pattern = @"^[a-zA-Z0-9_\.-]+@([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,6}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

        public static bool IsValidPassword(string password)
        {
            // Reglas básicas para la contraseña: al menos 8 caracteres, una letra mayúscula, una letra minúscula y un número
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(password);
        }

    }

}
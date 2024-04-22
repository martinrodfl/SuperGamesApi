using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperGamesApi.Contexts;
using SuperGamesApi.Models;
using SuperGamesApi.Models.Responses;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SuperGamesApi.Controllers
{
    [Route("/mygames")]
    [ApiController]
    [Authorize]
    public class GamesController : ControllerBase
    {

        private readonly SuperGamesContext _SuperGamesContext;
        public GamesController(SuperGamesContext userDBContext)
        {
            _SuperGamesContext = userDBContext;
        }

        // GET: api/<GamesController>
        [HttpGet("{userId}")]
        public async Task<ActionResult<List<int>>> GetGameIdsByUserId(int userId)
        {
            var gameIdsList = await _SuperGamesContext.GetGamesIds(userId);
            return Ok(gameIdsList);
        }



        // POST api/<GamesController>
        /// <summary>
        /// Crear un nuevo GameIds.
        /// </summary>
        /// <returns>Un nuevo GameIds</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "userId":56,
        ///         "gameId":78
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Crea el GameIds</response>
        /// <response code="400">No se pudo crear el GameIds</response>
        /// 
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(GameIds), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<ActionResult> CreateGameId(GameIds gameId)
        {

            var game = await _SuperGamesContext.GameIds.FirstOrDefaultAsync(gi => gi.UserId == gameId.UserId && gi.GameId == gameId.GameId);

            if (game != null)
            {
                return BadRequest(new ErrorResponse { Status = 400, Message = "Game exists" });
            }

                
            _SuperGamesContext.GameIds.Add(gameId);
               
            await _SuperGamesContext.SaveChangesAsync();
                
            return new CreatedResult($"https://localhost:7122/api/mygames/1{gameId?.UserId}", new GameIdsResponse
               
            {
                    
                Status = 201,
                    
                Message = $"GameIds Created"
                
            });

        }

        // DELETE
        /// <summary>
        /// Elimina un nuevo GameIds.
        /// </summary>
        /// <returns>204</returns>
        /// <response code="204">GameIds se ha eliminado correctamente</response>
        /// <response code="400">No se pudo crear el GameIds</response>
        /// 
        [HttpDelete("{userId}/{gameId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(GameIds), 200)]
        [ProducesResponseType(typeof(GameIds), 204)]
        public async Task<IActionResult> DeleteGameId(int userId,int gameId)
        {
            var gameIds = await _SuperGamesContext.GameIds.FirstOrDefaultAsync(gi => gi.UserId == userId && gi.GameId == gameId);

            if (gameIds == null)
            {
                return NotFound();
            }

            _SuperGamesContext.GameIds.Remove(gameIds);
            await _SuperGamesContext.SaveChangesAsync();

            return Ok(new { status = 200, Message = $"Eliminated gameids {gameIds.UserId}{gameIds.GameId}" });
        }
    }
}

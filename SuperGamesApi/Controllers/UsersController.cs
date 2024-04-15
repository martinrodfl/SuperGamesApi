using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperGamesApi.Contexts;
using SuperGamesApi.Models;

namespace SuperGamesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SuperGamesContext _SuperGamesContext;

        public UsersController(SuperGamesContext context)
        {
            _SuperGamesContext = context;
        }

        // GET: api/Users ---------------------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserEntity>>> GetUsers()
        {
            return await _SuperGamesContext.Users.ToListAsync();
        }

        // GET: api/Users/5 ---------------------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<UserEntity>> GetUser(int id)
        {
            var user = await _SuperGamesContext.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5 ---------------------------------------------------------------------------------
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _SuperGamesContext.Entry(user).State = EntityState.Modified;

            try
            {
                await _SuperGamesContext.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)

            {

                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users ---------------------------------------------------------------------------------
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserEntity>> PostUser(UserEntity user)
        {
            _SuperGamesContext.Users.Add(user);
            await _SuperGamesContext.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5 ---------------------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _SuperGamesContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _SuperGamesContext.Users.Remove(user);
            await _SuperGamesContext.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _SuperGamesContext.Users.Any(e => e.Id == id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SuperGamesApi.Helpers;
using SuperGamesApi.Models;
using SuperGamesApi.Models.Requests;

namespace SuperGamesApi.Contexts
{
    public class SuperGamesContext : DbContext
    {
        public SuperGamesContext(DbContextOptions<SuperGamesContext> options) : base(options) { }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<GameIds> GameIds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameIds>()
                .HasKey(e => new { e.UserId, e.GameId });
        }

        // GET GAMES from 1 user-------------------------------
        public async Task<List<int>> GetGamesIds(int userId)
        {
            return await GameIds
                            .Where(g => g.UserId == userId)
                            .Select(g => g.GameId)
                            .ToListAsync();
        }

        public async Task<int> GetLastGameId(int userId)
        {
            return await GameIds
                            .Where(g => g.UserId == userId)
                            .OrderByDescending(g => g.GameId)
                            .Select(g => g.GameId)
                            .FirstOrDefaultAsync();
        }
        // GET ONE USER-------------------------------
        public async Task<UserEntity?> Get(long id)
        {
            var user = await Users.FirstOrDefaultAsync(x => x.Id == id);
            return user;
        }

        // CREATE USER--------------------------------
        public async Task<UserEntity?> Add(RegisterUserRequest user)
        {
            UserEntity entity = new()
            {
                Id = null,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = Encrypt.GetSHA256(user.Password),
                CreatedDate = DateTime.UtcNow,
            };

            EntityEntry<UserEntity> response = await Users.AddAsync(entity);

            await SaveChangesAsync();

            return await Get(response.Entity.Id ?? throw new Exception("Not saved"));
        }
    }

    public class UserEntity
    {
        public int? Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public User ToCompleteData()
        {
            return new User()
            {
                Id = Id ?? throw new Exception("The Id cannot be null"),
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Password = Password,
                CreatedDate = CreatedDate,
            };
        }

        public UserLoginData GetTokenAndName()
        {
            return new UserLoginData
            {
                Id = Id ?? throw new Exception("The Id cannot be null"),
                UserName = $"{FirstName} {LastName}",
                Email = Email,
            };
        }

    }

    
   



}

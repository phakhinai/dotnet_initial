using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using dotnet_hero.Data;
using dotnet_hero.Entities;
using dotnet_hero.Installers;
using dotnet_hero.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_hero.Services
{
    public class AccountService : IAccountService
    {
        private readonly DatabaseContext databaseContext;
        private readonly JwtSettings jwtSettings;
        public AccountService(DatabaseContext databaseContext, JwtSettings jwtSettings)
        {
            this.jwtSettings = jwtSettings;
            this.databaseContext = databaseContext;
        }

        public async Task Register(Account account)
        {
            var existingAccount = await databaseContext.Accounts
                    .SingleOrDefaultAsync(a => a.Username == account.Username);

            if (existingAccount != null)
            {
                throw new Exception("Exitsting Account");
            }

            account.Password = CreatePasswordHash(account.Password);
            databaseContext.Accounts.Add(account);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<Account> Login(string username, string password)
        {
            var account = await databaseContext.Accounts.Include(a => a.Role)
                    .SingleOrDefaultAsync(a => a.Username == username);

            if (account != null && VerifyPassword(account.Password, password))
            {
                return account;
            }
            return null;
        }

        private string CreatePasswordHash(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 258 / 8
            ));
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        private bool VerifyPassword(string hash, string password)
        {
            var parts = hash.Split('.', 2);
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var passwordHash = parts[1];

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 258 / 8
            ));

            return passwordHash == hashed;
        }

        public string GenerateToken(Account account)
        {
            // ! key is case-sensitive

            var claims = new[]{
                new Claim(JwtRegisteredClaimNames.Sub, account.Username),
                new Claim("role", account.Role.Name),
                new Claim("additional", "todo"),
                new Claim("todo day", "11/22/33"),
            };

            return BuildToken(claims);
        }

        public Account GetInfo(string accessToken)
        {
            var token = new JwtSecurityTokenHandler().ReadToken(accessToken) as JwtSecurityToken;

            // ! key is case-sensitive

            var username = token.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            var role = token.Claims.First(claim => claim.Type == "role").Value;

            var account = new Account
            {
                Username = username,
                Role = new Role
                {
                    Name = role
                }
            };

            return account;
        }

        private string BuildToken(Claim[] claims)
        {
            var expires = DateTime.Now.AddDays(Convert.ToDouble(jwtSettings.Expire));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
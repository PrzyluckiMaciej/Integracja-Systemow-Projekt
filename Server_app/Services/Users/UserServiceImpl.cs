﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Server_app.Entities;
using Server_app.Model;
using Server_app.Entities;
using Server_app.Model;
using Server_app.Services.Users;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Server_app.Sevices.Users
{
    public class UserServiceImpl : IUserService
    {
        private static List<User> users = new List<User>
        {
            new User{Id = 1, Username = "Viewer", Password =
            "viewer", Roles = new List<Role>{ new Role { roleName = "read" } } },
            new User{Id = 2, Username = "Admin", Password =
            "admin", Roles = new List<Role>{ new Role { roleName =
            "read" }, new Role { roleName = "write" } } }
        };
        private IConfiguration configuration;
        public UserServiceImpl(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public AuthenticationResponse Authenticate(AuthenticationRequest request)
        {
            User user = GetByUsername(request.Username);
            if (user == null || user.Password != request.Password)
            {
                return null;
            }
            string token = generateJwtToken(user);
            return new AuthenticationResponse(user, token);
        }
        public User GetById(int id)
        {
            return users.FirstOrDefault(x => x.Id == id);
        }
        public User GetByUsername(string username)
        {
            return users.FirstOrDefault(x => x.Username == username);
        }
        public IEnumerable<User> GetUsers()
        {
            return users;
        }
        public string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:key"]);
            var claims = new List<Claim>();
            claims.Add(new Claim("id", user.Id.ToString()));
            foreach (var role in user.Roles)
            {
                claims.Add(new
               Claim(ClaimTypes.Role, role.roleName));
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.ToArray()),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token =
           tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }
    }
}

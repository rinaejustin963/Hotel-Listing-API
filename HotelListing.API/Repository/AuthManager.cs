using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Repository
{
    public class AuthManager : IAuthManager
    {

        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager)
        {
            this._mapper = mapper;
            this._userManager = userManager;
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
           var user = await _userManager.FindByEmailAsync(loginDto.Email);
           bool isValidUser = await _userManager.CheckPasswordAsync(user, loginDto.Password);
           
            if(user == null || isValidUser == false)
            {
                return null;
            }
            var token = await GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
            };
           

        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            //Get the Dto and Map it
            var user = _mapper.Map<ApiUser>(userDto);
            //Make sure the email and username have thhe same value
            user.UserName = userDto.Email;
            //try to create the user with the created password
            var result = await _userManager.CreateAsync(user, userDto.Password);
            
            //If it was successful then add them to the roles
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user,"User");
            }
            //Else return error message
            return result.Errors;
        }

        private async Task<string> GenerateToken(ApiUser _user)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(_user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
            var userClaims = await _userManager.GetClaimsAsync(_user);

           
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id),
            }
            .Union(userClaims).Union(roleClaims);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

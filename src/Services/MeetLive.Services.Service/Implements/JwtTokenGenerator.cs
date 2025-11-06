using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.IService.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MeetLive.Services.Service.Implements
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }
        public string GenerateToken(UserInfoDto userDto)
        {
            var tokenHandle = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);

            var claimList = new List<Claim>
            {
                new Claim("NickName", userDto.NickName),
                new Claim("Email",userDto.Email),
                new Claim("UserId",userDto.UserId.ToString()),
                new Claim("IsAdmin",userDto.IsAdmin.ToString())
            };

            var tokenDescript = new SecurityTokenDescriptor
            {
                Audience = _jwtOptions.Audience,
                Issuer = _jwtOptions.Issuer,
                Subject = new ClaimsIdentity(claimList),
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.Expires),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandle.CreateToken(tokenDescript);

            return tokenHandle.WriteToken(token);
        }
    }
}

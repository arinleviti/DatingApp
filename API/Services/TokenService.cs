using API.Entities;
using API.Interfaces;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;


namespace API.Services;

public class TokenService (IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        //config is a dictionary that contains all the key-value pairs from the appsettings.json file
        //?? is the null-coalescing operator, which returns the value of its left-hand operand if the operand is not null; 
        // otherwise, it returns the right-hand operand.
        var tokenKey = config["TokenKey"] ?? throw new Exception("Token key not found");
        if (tokenKey.Length < 64 ) throw new Exception("Token key is too short");

        //Here, the tokenKey (the string value from the config) is converted into a byte[] using UTF-8 encoding. 
        // This byte array is then used to create a SymmetricSecurityKey (which is a key that can be used for both encryption and decryption).
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        //Claims are pieces of information about the user that will be included in the token.
        //In this case, the ClaimTypes.NameIdentifier is used to store the username of the user.
        //Claims can include any other user-related data you might need (such as roles or email), but in this example, it’s just the UserName.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserName)
        };
        //SigningCredentials specify the algorithm used for signing the token and the key that will be used for the signing. 
        //Here, the algorithm used is HmacSha512Signature, which is an HMAC (Hash-based Message Authentication Code) using SHA-512.
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        //A SecurityTokenDescriptor is an object that defines how the token should be created.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        //JwtSecurityTokenHandler is a class that provides methods for creating, validating, and reading JWT tokens.
        //CreateToken takes the tokenDescriptor and uses it to generate the actual JWT.
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

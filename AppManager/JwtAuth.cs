using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
    
namespace AppManager;

public static class JwtAuth
{
    private static TokenValidationParameters GetValidationParameters()
    {
        var certificate = Certificate.TryGetCertificate("path");
        return new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "App_Manager_Backend",
            ValidAudience = "App_Manager_Audience",
            IssuerSigningKey = certificate.PublicKey
        };
    }


    public static string GenerateToken(int userId)
    {
        var certificate = Certificate.TryGetCertificate("path");
        var token = new JwtSecurityToken
        (
            issuer: "App_Manager_Backend",
            audience: "App_Manager_Audience",
            claims:
            [
                new Claim("user_id", $"{userId}")
            ],
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(certificate.PrivateKey, SecurityAlgorithms.Sha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public static bool IsTokenLegit(string token)
    {
        var validated = new JwtSecurityTokenHandler().ValidateToken
        (
            token,
            GetValidationParameters()
            ,out var validatedToken
        );

        return validated == null;
    }
}

file readonly record struct Certificate(RsaSecurityKey PublicKey, RsaSecurityKey PrivateKey)
{

    public RSA Rsa => PublicKey.Rsa;
    
    public static Certificate TryGetCertificate(string path)
    {
        var rsa = RSA.Create(2048);
        
        var publicKeyPath = $"{path}.public.pem";
        var privateKeyPath = $"{path}.private.pem";

        if (File.Exists(publicKeyPath) && File.Exists(privateKeyPath))
        {
            try
            {
                rsa.ImportFromPem(File.ReadAllText(publicKeyPath));
                rsa.ImportFromPem(File.ReadAllText(privateKeyPath));

                return new Certificate()
                {
                    PublicKey = new RsaSecurityKey(rsa.ExportParameters(true)),
                    PrivateKey = new RsaSecurityKey(rsa.ExportParameters(false))
                };

            }
            catch (Exception ex)
            {
                
            }
        }

        var certificate = new Certificate()
        {
            PublicKey = new RsaSecurityKey(rsa.ExportParameters(true)),
            PrivateKey = new RsaSecurityKey(rsa.ExportParameters(true))
        };

        File.WriteAllText(publicKeyPath,rsa.ExportRSAPublicKeyPem());
        File.WriteAllText(privateKeyPath,rsa.ExportRSAPrivateKeyPem());
        
        return certificate;
    }
    
    
    
    
}
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace AppManager;

public static class ControllerBaseExtensions
{
    public static string GenerateSalt(this ControllerBase controllerBase)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}
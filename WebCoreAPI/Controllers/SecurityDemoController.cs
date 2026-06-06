using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Services;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates password hashing, AES symmetric encryption, and HMAC integrity.
/// (These are building blocks behind the JWT auth used elsewhere in this project.)
/// </summary>
[ApiController]
[Route("api/security")]
public class SecurityDemoController : ControllerBase
{
    private readonly SecurityService _security;

    public SecurityDemoController(SecurityService security)
    {
        _security = security;
    }

    // POST /api/security/hash  { "password": "secret123" }
    [HttpPost("hash")]
    public IActionResult Hash([FromBody] PasswordRequest req)
    {
        var hash = _security.HashPassword(req.Password);
        return Ok(new
        {
            note = "Salt and hash are stored together. The same password hashes differently each time (random salt).",
            stored = hash,
            verifiesCorrect = _security.VerifyPassword(req.Password, hash),
            verifiesWrong = _security.VerifyPassword(req.Password + "x", hash)
        });
    }

    // POST /api/security/encrypt  { "text": "sensitive data" }
    [HttpPost("encrypt")]
    public IActionResult Encrypt([FromBody] TextRequest req)
    {
        var cipher = _security.Encrypt(req.Text);
        var roundTrip = _security.Decrypt(cipher);
        return Ok(new
        {
            algorithm = "AES (symmetric)",
            original = req.Text,
            encrypted = cipher,
            decrypted = roundTrip
        });
    }

    // POST /api/security/hmac  { "text": "message to sign" }
    [HttpPost("hmac")]
    public IActionResult Hmac([FromBody] TextRequest req)
    {
        var signature = _security.ComputeHmac(req.Text);
        return Ok(new
        {
            note = "HMAC proves the message wasn't tampered with, using a shared secret.",
            message = req.Text,
            signature,
            verifies = _security.VerifyHmac(req.Text, signature),
            tamperedVerifies = _security.VerifyHmac(req.Text + "!", signature)
        });
    }

    public class PasswordRequest { public string Password { get; set; } = string.Empty; }
    public class TextRequest { public string Text { get; set; } = string.Empty; }
}

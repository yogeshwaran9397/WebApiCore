using System.Security.Cryptography;
using System.Text;

namespace WebCoreAPI.Services;

/// <summary>
/// Demonstrates the core cryptography primitives used to secure an API:
///   - Password hashing (PBKDF2 with a per-password salt)
///   - Symmetric encryption (AES) — same key encrypts and decrypts
///   - HMAC — message integrity / authenticity with a shared secret
/// </summary>
public class SecurityService
{
    // ---------- PASSWORD HASHING (PBKDF2) ----------
    // NEVER store plain-text passwords. Hash with a random salt + many iterations.
    // Real apps often use BCrypt; PBKDF2 is built into .NET so needs no package.

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        // Store salt + hash together (salt is not secret, just unique).
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 2) return false;
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expected = Convert.FromBase64String(parts[1]);
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        // Constant-time comparison avoids timing attacks.
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    // ---------- SYMMETRIC ENCRYPTION (AES) ----------
    // Same secret key encrypts and decrypts. Fast; good for data at rest.

    private static readonly byte[] AesKey = SHA256.HashData(Encoding.UTF8.GetBytes("demo-symmetric-key-please-change"));

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = AesKey;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        // Prepend the IV (not secret) so we can decrypt later.
        return $"{Convert.ToBase64String(aes.IV)}.{Convert.ToBase64String(cipher)}";
    }

    public string Decrypt(string cipherText)
    {
        var parts = cipherText.Split('.');
        if (parts.Length != 2) throw new ArgumentException("Invalid cipher format.");
        using var aes = Aes.Create();
        aes.Key = AesKey;
        aes.IV = Convert.FromBase64String(parts[0]);
        using var decryptor = aes.CreateDecryptor();
        byte[] cipher = Convert.FromBase64String(parts[1]);
        byte[] plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }

    // ---------- HMAC (message integrity) ----------
    // Proves a message wasn't tampered with, using a shared secret.

    private static readonly byte[] HmacKey = Encoding.UTF8.GetBytes("demo-hmac-shared-secret");

    public string ComputeHmac(string message)
    {
        using var hmac = new HMACSHA256(HmacKey);
        byte[] sig = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(sig);
    }

    public bool VerifyHmac(string message, string signature)
    {
        var expected = ComputeHmac(message);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
    }
}

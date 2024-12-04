using System;
using BCrypt.Net;

namespace Cuttr.Business.Utilities
{
    public static class PasswordHasher
    {
        // Work factor for bcrypt (higher means more secure but slower)
        private const int WorkFactor = 12;

        public static string HashPassword(string password)
        {
            // Generate the hashed password using bcrypt
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Verify the password against the stored hash
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}

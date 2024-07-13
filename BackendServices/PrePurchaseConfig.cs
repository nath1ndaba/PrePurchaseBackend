using Microsoft.IdentityModel.Tokens;
using System;

namespace BackendServices
{
    public class PrePurchaseConfig
    {
        public static string DATABASE_NAME => GetEnv(nameof(DATABASE_NAME));
        public static string DATABASE_HOST => GetEnv(nameof(DATABASE_HOST));
        public static string DATABASE_AUTH_DB => GetEnv(nameof(DATABASE_AUTH_DB));
        public static string DATABASE_USERNAME => GetEnv(nameof(DATABASE_USERNAME));
        public static string DATABASE_USER_PASSWORD => GetEnv(nameof(DATABASE_USER_PASSWORD));

        #region Peach Payment
        public static string PAYMENT_AUTHORIZATION => GetEnv(nameof(PAYMENT_AUTHORIZATION));
        public static string PAYMENT_3D_SECURE_ID => GetEnv(nameof(PAYMENT_3D_SECURE_ID));
        public static string PAYMENT_CHECKOUT_URL => GetEnv(nameof(PAYMENT_CHECKOUT_URL));
        public static string PAYMENT_PAYROLL_URL => GetEnv(nameof(PAYMENT_PAYROLL_URL));
        public static string PAYMENT_PAYROLL_APIKEY => GetEnv(nameof(PAYMENT_PAYROLL_APIKEY));
        #endregion

        // number of threads
        public static int ARGON_PARALLELISM_FACTOR => GetIntEnv(nameof(ARGON_PARALLELISM_FACTOR), 1);
        // in KB
        public static int ARGON_MEMORY_COST => GetIntEnv(nameof(ARGON_MEMORY_COST), 16);
        public static int ARGON_ITERATIONS => GetIntEnv(nameof(ARGON_ITERATIONS), 4);
        public static int ARGON_HASH_LENGTH => GetIntEnv(nameof(ARGON_HASH_LENGTH), 16);

        // JWT
        public static string JWT_SECRET_KEY => GetEnv(nameof(JWT_SECRET_KEY));
        public static string JWT_SECURITY_ALGORITHM => GetEnv(nameof(JWT_SECURITY_ALGORITHM), SecurityAlgorithms.HmacSha512Signature);
        public static string JWT_ISSUER => GetEnv(nameof(JWT_ISSUER), "http://localhost:5000");
        public static string JWT_AUDIENCE => GetEnv(nameof(JWT_AUDIENCE), "http://localhost:5000");
        public static bool JWT_VALIDATE_ISSUER => GetBoolEnv(nameof(JWT_VALIDATE_ISSUER), true);
        public static bool JWT_VALIDATE_AUDIENCE => GetBoolEnv(nameof(JWT_VALIDATE_AUDIENCE), true);
        // in minutes
        public static long JWT_TOKEN_EXPIRES_IN => GetTime(nameof(JWT_TOKEN_EXPIRES_IN), "5m");
        public static long JWT_REFRESH_TOKEN_EXPIRES_IN => GetTime(nameof(JWT_REFRESH_TOKEN_EXPIRES_IN), "7d");

        // System Configs

        public static int CLOCK_IN_BEFORE_ALLOWED_TOLERANCE_MINUTES => (int)GetTime(nameof(CLOCK_IN_BEFORE_ALLOWED_TOLERANCE_MINUTES), "5M");
        public static float CLOCK_IN_OUT_RADIUS_IN_METERS => GetFloatEnv(nameof(CLOCK_IN_OUT_RADIUS_IN_METERS), 25f);

        // EmployeeId configs

        public static string NANOID_ALPHABET => GetEnv(nameof(NANOID_ALPHABET), "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
        public static int NANOID_ID_LENGTH => GetIntEnv(nameof(NANOID_ID_LENGTH), 7);

        public static long GetTime(string key, string @default)
        {
            TimeSpan time;
            try
            {
                time = GetEnv(key).ParseTime();
            }
            catch
            {
                time = @default.ParseTime();
            }
            return (long)time.TotalSeconds;
        }

        public static long GetTime(string key, long @default)
        {
            try
            {
                return (long)GetEnv(key).ParseTime().TotalSeconds;
            }
            catch
            {
                return @default;
            }
        }

        public static string GetEnv(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }

        public static void SetEnv(string key, string value, EnvironmentVariableTarget target)
            => Environment.SetEnvironmentVariable(key, value, target);

        public static string GetEnv(string key, string @default)
        {
            var value = GetEnv(key);
            return string.IsNullOrWhiteSpace(value) ? @default : value;
        }

        public static int GetIntEnv(string key)
        {
            return GetIntEnv(key, 0);
        }

        public static int GetIntEnv(string key, int @default)
        {
            return int.TryParse(GetEnv(key), out int result) ? result : @default;
        }

        public static float GetFloatEnv(string key)
        {
            return GetFloatEnv(key,0);
        }

        public static float GetFloatEnv(string key, float @default)
        {
            return float.TryParse(GetEnv(key), out float result) ? result : @default;
        }

        public static bool GetBoolEnv(string key)
        {
            return GetBoolEnv(key, false);
        }

        public static bool GetBoolEnv(string key, bool @default)
        {
            return bool.TryParse(GetEnv(key), out bool result) ? result : @default;
        }

    }
}

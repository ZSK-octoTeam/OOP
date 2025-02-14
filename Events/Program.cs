using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace project_zdarzenia
{
    internal class Program
    {
        public enum Role
        {
            Administrator,
            Menager,
            User
        }

        public class User
        {
            public string Username { get; set; }
            public List<Role> Roles { get; set; }

            public User(string username) 
            { 
                Roles = new List<Role>();
                Username = username;
            }

            public void AddRole(Role role)
            {
                if (!Roles.Contains(role))
                {
                    Roles.Add(role);
                }
            }
        }

        public class RBAC
        {
            private readonly Dictionary<Role, List<string>> _rolePermission;

            public RBAC()
            {
                _rolePermission = new Dictionary<Role, List<string>> 
                {
                    {Role.Administrator, new List<string> {"Read", "Write", "Delete"} },
                    {Role.Menager, new List<string> {"Read", "Write"} },
                    {Role.User, new List<string> {"Read"} }
                };
            }

            public bool HasPermission(User user, string perimission) 
            { 
                foreach(var role in user.Roles) 
                {
                    if (_rolePermission.ContainsKey(role) && _rolePermission[role].Contains(perimission))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public class PasswordManeger
        {
            private const string _passwordFilePath = "userPasswords.txt";
            public static event Action<string, bool> PasswordVerify;
            
            static PasswordManeger()
            {
                if (!File.Exists(_passwordFilePath))
                {
                    File.Create(_passwordFilePath).Dispose();
                }
            }

            public static void SavePassword(string username,  string password)
            {
                if (File.ReadLines(_passwordFilePath).Any(line => line.Split(',')[0] == username))
                {
                    Console.WriteLine($"Użytkownik {username} już istnieje w systemie");
                    return;
                }

                string hashedPassword = HashPassword(password);
                File.AppendAllText(_passwordFilePath, $"{username},{hashedPassword}\n");
                Console.WriteLine($"Użytkownik {username} został zapisany");
            }

            public static bool VerifyPassword(string username, string password)
            {
                string hashedPassword = HashPassword(password);
                foreach(var line in File.ReadLines(_passwordFilePath))
                {
                    var parts = line.Split(',');
                    if (parts[0] == username && parts[1] == hashedPassword)
                    {
                        PasswordVerify?.Invoke(username, true);
                        return true;
                    }   
                }
                PasswordVerify?.Invoke(username, false);
                return false;
            }

            private static string HashPassword(string password)
            {
                using (var sha256 = SHA256.Create())
                {
                    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return Convert.ToBase64String(bytes);
                }
            }
        }
        static void Main(string[] args)
        {
            PasswordManeger.PasswordVerify += (Username, success) => Console.WriteLine
            ($"Logowanie użytkownika o nazwie {Username}: {(success ? "udane" : "nieudane")}");

            PasswordManeger.SavePassword("admin", "pass");

            Console.WriteLine("\nWprowadź nazwę użytkownika: ");
            string username = Console.ReadLine();

            Console.WriteLine("Wprowadź hasło: ");
            string password = Console.ReadLine();
            Console.WriteLine();

            var user = new User(username);

            if(username == "admin")
            {
                user.AddRole(Role.Administrator);
            }

            var rbacSystem = new RBAC();

            Console.WriteLine("\nSprawdzanie dostępu do różnych zasobów");
            Console.WriteLine("Read " + rbacSystem.HasPermission(user, "Read"));
            Console.WriteLine("Write " + rbacSystem.HasPermission(user, "Write"));
            Console.WriteLine("Delete " + rbacSystem.HasPermission(user, "Delete"));
            Console.WriteLine();

            Console.ReadKey();
        }
    }
}

using System.Security.Cryptography;
using System.Text;

namespace _5_1_zdarzenia
{
    internal class Program
    {
        public enum Role
        {
            Administrator,
            Manager,
            User
        }

        public enum Permission
        {
            Read,
            Write,
            Delete,
            ManageUsers
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
            private readonly Dictionary<Role, List<Permission>> _rolePermissions;


            public RBAC()
            {
                _rolePermissions = new Dictionary<Role, List<Permission>>
                {
                    { Role.Administrator, new List<Permission>{ Permission.Read, Permission.Write, Permission.Delete, Permission.ManageUsers }},
                    { Role.Manager, new List<Permission>{ Permission.Read, Permission.Write}},
                    { Role.User, new List<Permission>{ Permission.Read } }
                };
            }

            public bool HasPermission(User user, Permission permission)
            {
                foreach (var role in user.Roles)
                {
                    if (_rolePermissions.ContainsKey(role) && _rolePermissions[role].Contains(permission))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public class PasswordManager
        {
            private const string _passwordFilePath = "userPasswords.txt";
            public static event Action<string, bool> PasswordVerified;

            static PasswordManager()
            {
                if (!File.Exists(_passwordFilePath))
                {
                    File.Create(_passwordFilePath).Dispose();
                }
            }

            public static void SavePassword(string username, string password)
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
                foreach (var line in File.ReadLines(_passwordFilePath))
                {
                    var parts = line.Split(',');
                    if (parts[0] == username && parts[1] == hashedPassword)
                    {
                        PasswordVerified?.Invoke(username, true);
                        return true;
                    }
                }
                PasswordVerified?.Invoke(username, false);
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

        public delegate void FileOperation(string filePath);

        public class FileMenager{
            
        }

        
        static void Main(string[] args)
        {
            PasswordManager.PasswordVerified += (username, success) => Console.WriteLine($"Logowanie użytkownika {username} : {(success ? "udane" : "nieudane")}");

            PasswordManager.SavePassword("AdminUser", "adminPassword");
            PasswordManager.SavePassword("MenagerUser", "MenagerPassword");
            PasswordManager.SavePassword("NormalUser", "NormalPassword");
            PasswordManager.SavePassword("xyz", "userPassword");

            bool exitProgram = false;

            while (!exitProgram)
            {
                Console.Clear();
                Console.WriteLine("=== System logowania ===");

                Console.Write("\nWprowadź nazwę użytkownika: ");
                string username = Console.ReadLine();

                Console.Write("Wprowadź hasło: ");
                string password = Console.ReadLine();
                Console.WriteLine();

                if (!PasswordManager.VerifyPassword(username, password))
                {
                    Console.WriteLine("Niepoprawna nazwa użytkownika lub hasło");
                    Console.ReadKey();
                    continue;
                }

                var user = new User(username);
                if(username == "AdminUser"){
                    user.AddRole(Role.Administrator);
                }else if(username =="MenagerUser"){
                    user.AddRole(Role.Manager);
                }else if(username =="NormalUser"){
                    user.AddRole(Role.User);
                }

                var rbacSystem = new RBAC();
                string filePath = "testFile.txt";

                while(true){
                    Console.Clear();
                    System.Console.WriteLine($"Zalogowano jako {username}");
                    System.Console.WriteLine("1. Odczytaj plik");
                    if(rbacSystem.HasPermission(user, Permission.Read)){
                        System.Console.WriteLine("1. Odczytaj plik");
                    }
                    if(rbacSystem.HasPermission(user, Permission.Write)){
                        System.Console.WriteLine("2. Zapisz do pliku");
                    }
                    if(rbacSystem.HasPermission(user, Permission.Delete)){
                        System.Console.WriteLine("3. Modyfikuj plik");
                    }
                    if(rbacSystem.HasPermission(user, Permission.ManageUsers)){
                        System.Console.WriteLine("4. Dodaj użytkownika");
                    }
                    System.Console.WriteLine("5. Wyloguj się");
                    System.Console.WriteLine("6. Wyjdź z programu");

                    int choice;
                    if(!int.TryParse(Console.ReadLine(), out choice)){
                        continue;
                    }

                    switch(choice){
                        
                    }

                    Console.ReadKey();
                }
            }
        }
    }
}
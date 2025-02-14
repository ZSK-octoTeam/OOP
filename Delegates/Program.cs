using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        public delegate void Notificationhandler(string message);

        public interface INotifier
        {
            void Notify(string message);
        }

        public class EmailNotifier : INotifier
        {
            public void Notify(string message)
            {
                Console.WriteLine($"Email wysłany: {message}");
            }
        }

        public class SMSNotifier : INotifier
        {
            public void Notify(string message)
            {
                Console.WriteLine($"SMS wysłany: {message}");
            }
        }

        public class PushNotifier : INotifier
        {
            public void Notify(string message)
            {
                Console.WriteLine($"Powiadomienie Push wysłane: {message}");
            }
        }

        public class NotificationManager
        {
            public Notificationhandler Notify;

            public void AddNotificationMethod(Notificationhandler handler)
            {
                if (Notify != null && Notify.GetInvocationList().Contains(handler))
                {
                    Console.WriteLine("Ta metoda powiadomienia jest już dodana...");
                    return;
                }
                Console.WriteLine("Dodano metodę powiadomienia!");
                Notify += handler;
            }

            public void RemoveNotificationMethod(Notificationhandler handler)
            {
                if (Notify != null && Notify.GetInvocationList().Contains(handler))
                {
                    Notify -= handler;
                    Console.WriteLine("Usunięto metodę powiadomienia!");
                    return;
                }
                Console.WriteLine("Nie można usunąc metody powiadomienia...");
            }

            public void SendNotification(string message)
            {
                if (Notify == null)
                {
                    Console.WriteLine("Brak dostępnych metod powiadomień... Dodaj conajmniej jedną metodę!");
                    return;
                }

                foreach (var handler in Notify.GetInvocationList())
                {
                    //Console.WriteLine(handler.Target.GetType().Name);
                    try
                    {
                        handler.DynamicInvoke(message);
                        string LogEntry = $"[{DateTime.Now:yyyy-MM-dd-HH-mm-ss}] wysłano: {handler.Method.Name}, wiadomość: {message}\n";
                        File.AppendAllText("Log.txt", LogEntry);
                    }
                    catch
                    {
                        Console.WriteLine("Błąd podczas wysyłania powiadomienia...");
                    }
                }
            }

            public void ListNotificationMethod()
            {
                if (Notify == null)
                {
                    Console.WriteLine("Brak zarejestrowanych metod powiadomień...");
                    return;
                }

                Console.WriteLine("Zarajestrowano metody powiadomień!");

                var displayhadler = new HashSet<string>();

                foreach (var handler in Notify.GetInvocationList())
                {
                    var target = handler.Target;
                    var methodName = handler.Method.Name;
                    var className = target.GetType().FullName;

                    var uniqeKey = $"{className}, {methodName}";

                    //Console.WriteLine(uniqeKey);

                    //if (!displayhadler.Contains(uniqeKey))
                    //{
                    displayhadler.Add(uniqeKey);
                    // Console.WriteLine($"Klasa: {className}, metoda: {methodName}");
                    //}
                }

                foreach (var handler in displayhadler)
                {
                    Console.WriteLine(handler);
                }
            }
        }

        public static void ShowMenu()
        {
            Console.WriteLine("Menu");
            Console.WriteLine("1. Dodaj powiadomienie Email");
            Console.WriteLine("2. Dodaj powiadomienie SMS");
            Console.WriteLine("3. Dodaj powiadomienie Push");
            Console.WriteLine("4. Usuń powiadomienie Email");
            Console.WriteLine("5. Usuń powiadomienie SMS");
            Console.WriteLine("6. Usuń powiadomienie Push");
            Console.WriteLine("7. Wyślij powiadomienia");
            Console.WriteLine("8. Pokaż zarejestrowane metody");
            Console.WriteLine("9. Wyjdź");
            Console.Write("Wybierz opcję: ");
        }

        static void Main(string[] args)
        {
            var emailNotifier = new EmailNotifier();
            var smsNotifier = new SMSNotifier();
            var pushNotifier = new PushNotifier();

            var notificationManager = new NotificationManager();

            while (true)
            {
                try
                {
                    ShowMenu();
                    var choice = int.Parse(Console.ReadLine());

                    switch (choice)
                    {
                        case 1:
                            notificationManager.AddNotificationMethod(emailNotifier.Notify);
                            break;
                        case 2:
                            notificationManager.AddNotificationMethod(smsNotifier.Notify);
                            break;
                        case 3:
                            notificationManager.AddNotificationMethod(pushNotifier.Notify);
                            break;
                        case 4:
                            notificationManager.RemoveNotificationMethod(emailNotifier.Notify);
                            break;
                        case 5:
                            notificationManager.RemoveNotificationMethod(smsNotifier.Notify);
                            break;
                        case 6:
                            notificationManager.RemoveNotificationMethod(pushNotifier.Notify);
                            break;
                        case 7:
                            Console.Write("Wpisz wiadomość do wysłania: ");
                            var message = Console.ReadLine();

                            //walidacja wiadomości
                            if (string.IsNullOrWhiteSpace(message))
                            {
                                Console.WriteLine("\nWiadomość nie może być pusta\n");
                                break;
                            }

                            if (message.Length > 20)
                            {
                                Console.WriteLine("Wiadomość jest zbyt długa (max 20 znaków)");
                                break;
                            }

                            notificationManager.SendNotification(message);
                            break;
                        case 8:
                            notificationManager.ListNotificationMethod();
                            break;
                        case 9:
                            return;
                        default:
                            Console.WriteLine("Nieprawidłowa opcja. Spróbuj ponownie");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
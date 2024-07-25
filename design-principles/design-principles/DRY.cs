





public class UserManager
{
    public void ProcessUser(User user)
    {
        if (IsUserUnderage(user.Age))
            return;
        
        Console.WriteLine("Processing User");
        // Process user...
    }

    public void RegisterUser(User user)
    {
        if (IsUserUnderage(user.Age))
            return;
        
        Console.WriteLine("Registering User");
        // Register user...
    }
    
    private bool IsUserUnderage(User user)
    {
        if (user.Age < 21)
        {
            Console.WriteLine("User is underage");
            LogAction("Underage user detected");
            return true;
        }
        return false;
    }

    #region Other Methods

    public class User
    {
        public int Age { get; set; }
    }

    public void LogAction(string message){}

    #endregion
}
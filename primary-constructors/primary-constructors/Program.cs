// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


public class Person(string name, int age)
{
    public string Name { get; set; } = name;
    public int Age { get; set; } = age;

    public Person() : this("Rahul", 30)
    {
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"Name: {name}, Age: {age}");
    }
}
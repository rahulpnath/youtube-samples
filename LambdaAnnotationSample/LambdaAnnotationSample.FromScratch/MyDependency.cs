namespace LambdaAnnotationSample.FromScratch
{
    public interface IMyDependency
    {
        string GetValue(string key);
    }

    public class MyDependency : IMyDependency
    {
        public MyDependency()
        {
            Value = Guid.NewGuid();
        }

        public Guid Value { get; set; }

        public string GetValue(string key)
        {
            return $"MyDependency {key} {Value}";
        }
    }
}

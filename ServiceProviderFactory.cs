namespace TimeTracker
{
    public static class ServiceProviderFactory
    {
        public static IServiceProvider Container { get; private set; }

        public static void SetContainer(IServiceProvider container)
        {
            Container = container;
        }
    }
}

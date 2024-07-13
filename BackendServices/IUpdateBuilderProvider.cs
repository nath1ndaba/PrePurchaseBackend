namespace BackendServices
{
    public interface IUpdateBuilderProvider
    {
        IUpdateBuilder<TType> For<TType>();
    }
}

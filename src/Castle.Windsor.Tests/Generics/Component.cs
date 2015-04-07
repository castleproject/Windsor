namespace CastleTests.Generics
{
    public interface ISingleton
    {
    }

    public interface IOuterService<T1, T2> : ISingleton
    {
    }

    public interface IWrap1<T> : ISingleton
    {
    }

    public interface IWrap2<T> : ISingleton
    {
    }

    public interface IService1<T1, T2> : IOuterService<IWrap1<T1>, T2>
    {
    }

    public interface IService2<T1, T2> : IOuterService<IWrap2<T1>, T2>
    {
    }

    public class Component<T1, T2> : IService1<T1, T2>, IService2<T1, T2>
    {
    }
}

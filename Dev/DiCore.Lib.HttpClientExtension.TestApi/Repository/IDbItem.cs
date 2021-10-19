namespace DiCore.Lib.RestClient.TestCore.Api.Repository
{
    public interface IDbItem<T>
    {
        T Id { get; set; }
    }
}
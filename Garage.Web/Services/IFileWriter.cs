namespace Garage.Web.Services
{
    public interface IFileWriter<TFile, TData>
    {
        Task WriteAsync(
        TFile file,
        TData data,
        CancellationToken cancellationToken = default);
    }
}

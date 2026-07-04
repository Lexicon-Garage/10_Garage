namespace Garage.Web.Services
{
    public interface IFileReader<TFile, TResult>
    {
      Task<TResult> ReadAsync(
      TFile file,
      CancellationToken cancellationToken = default);
    }
}

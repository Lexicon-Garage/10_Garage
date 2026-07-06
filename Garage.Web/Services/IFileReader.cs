namespace Garage.Web.Services
{
    public interface IFileReader<TFile, TResult>
    {
        //In case the requirement to read from a local file and extract data
        Task<TResult> ReadAsync(
      TFile file,
      CancellationToken cancellationToken = default);
    }
}

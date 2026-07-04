namespace Garage.Web.Services
{
    public interface IFileHandler<TFile, TData> :
    IFileReader<TFile, TData>,
    IFileWriter<TFile, TData>
    {
    }
}

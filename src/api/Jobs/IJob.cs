namespace ADAM.API.Jobs;

public interface IJob
{
    Task ExecuteAsync();
}

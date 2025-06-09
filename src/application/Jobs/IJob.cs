namespace ADAM.Application.Jobs;

public interface IJob
{
    Task ExecuteAsync();
}

using System.Threading.Tasks;

namespace PresentationBus
{
    public interface IHandlePresentationCommand<in T> : IHandlePresentationCommands
        where T : IPresentationCommand
    {
        void Handle(T presentationCommand);
    }

    public interface IHandlePresentationCommandAsync<in T> : IHandlePresentationCommands
        where T : IPresentationCommand
    {
        Task HandleAsync(T presentationCommand);
    }
}
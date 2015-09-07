namespace PresentationBus
{
    public interface IPresentationBusConfiguration
    {
        void Subscribe(IHandlePresentationMessages instance);

        void UnSubscribe(IHandlePresentationMessages instance);
    }
}
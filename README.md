PresentationBus
====================

In-process messaging for .NET rich clients.  PresentationBus is compatible with WPF, Windows 8 Store Apps, Windows Phone 8/8.1, Xamarin and Xamarin Forms.

#Getting started
One of the key benefits in using messaging within your UI is decoupling.  The two keys scenarios the PresentationBus covers are "I just did something that others might want to know about" and "I want/need to know something but don't know where to get it".
##Events
Events cover the "I just did something" scenario.  The PresentationBus will multicast any events that you publish.
##Requests
Requests cover the "I want/need to know something" scenario.  There are a couple of options involved with requests, as described below.
###MustBeHandled
Use this property of your request to identity whether you want (false) or need (true) a response.
The provided PresentationRequest base class has a default of MustBeHandled = true.  The best place to change it is in the constructor of your derived request.
###IsHandled
Set this property to true in your handler when the request has been fulfilled.
If the request **MustBeHandle**d and the PresentationBus gets to the end of its list of subscribers without one of them setting IsHandled to true it will throw an exception.
The PresentationBus will also stop publishing to any further subscribers once IsHandled is set to true.
##Threading
The PresentationBus uses an async publish, so the UI thread doesn't block.  Therefore the subscribers get called on a background thread, and if they have to do something back on the UI thread it is their responsibility to marshal what they need onto the UI thread.
#Some examples
##Publisher
	public PersonViewModel SelectedPerson
	{
		get { return _selectedPerson; }
		set 
		{
			_selectedPerson = value;
			PublishSelectedPersonChanged();
		}
	}

	private async void PublishSelectedPersonChanged()
	{
		await _bus.PublishAsync(new SelectedPersonChanged(_selectedPerson));
	}
I've used the publishing of events like this with great success in the past to decouple UIs that have a list and details panel.  Beware the "async void" though, exceptions from the subscribers will not surface.
##Subscriber
	public class PersonDetailsPanelViewModel : IHandlePresentationEvent<SelectedPersonChanged>
	{
		...
		public void Handle(SelectedPersonChanged selectionChangedEvent)
		{
			// update the panel details with those of the person in the event
		}
	}
If you have a subscriber that needs to await further calls, e.g. to load data, then use the IHandlePresentationEventAsync interface.

	public class PersonDetailsPanelViewModel : IHandlePresentationEventAsync<SelectedPersonChanged>
	{
		...
		public async Task Handle(SelectedPersonChanged selectionChangedEvent)
		{
			// await whatever data load you need
			// update the panel details
		}
	}
##Request provider
	public class JukeboxViewModel : IHandlePresentationRequest<PlayTrackRequest>
	{
		...
		public void Handle(PlayTrackRequest request)
		{
			// do whatever it is that makes the track play
			request.IsHandled = true;
		}
	}
So in this example you've got a ViewModel somewhere in the UI that knows how to play a music track, e.g using a MediaElement or something like that.  You might have any number of other ViewModels in the UI that represent a track, and might want to request that their track gets played, in which case all they have to do is publish a request.

##Requestor

NOTE: I [highly encourage Commands over Execute implementations in ViewModels](http://blog.shannonlewis.me/2014/06/xaml-commands/), but for the sake of brevity let's illustrate as follows.

	public class TrackViewModel
	{
		...
		public async void OnPlayMeExecuted()
		{
			await _bus.PublishAsync(new PlayTrackRequest(this));
		}
	}
#Subscribing
The last step is to understand how to get the subscribers wired up.  In short, you have to pass an instance of the subscibing class into the PresentationBus' Subscribe method.  In most of the apps I build I'm using an IoC container, and we can get it to do some of the dirty work.
##Autofac
If you're using Autofac you may want something like the following to register the bus itself into the container

    public class PresentationBusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PresentationBus>().As<IPresentationBus>().SingleInstance();
        }
    }

So now you can take a dependency on an IPresentationBus, which is certainly what you'll want for publishing.  You could also take an IPresentationBus and Subscribe yourself if you handle events/requests, but why have to write that code all the time?  I prefer to use the following 

    public class PresentationBusSubscriptionModule : Autofac.Module
    {
        protected override void AttachToComponentRegistration(IComponentRegistry registry, IComponentRegistration registration)
        {
            registration.Activated += OnComponentActivated;
        }

        static void OnComponentActivated(object sender, ActivatedEventArgs<object> e)
        {
            if (e == null)
                return;

            var handler = e.Instance as IHandlePresentationEvents;
            if (handler == null)
                return;
            var bus = e.Context.Resolve<IPresentationBus>();
            bus.Subscribe(handler);
        }
    }
This will hook into the activation of all of object being resolved from the container, and if they handle any events/requests they will be subscribed.

Something to always remember in rich client apps is that objects can live for a lot longer than you might think.  The PresentationBus uses WeakReferences internally so it will allow them to be collected once they are no longer being used, however until they are collected they will still receive events/requests unless you explicitly Unsubscribe them!  Usually this isn't an issue, but I have hit these scenarios on occasion.
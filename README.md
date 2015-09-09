PresentationBus
====================

In-process messaging for .NET rich clients.  PresentationBus is compatible with WPF, Windows 8 Store Apps, Windows Phone 8/8.1, Xamarin and Xamarin Forms.

#Getting started
One of the key benefits in using messaging within your UI is decoupling.  The key scenarios the PresentationBus covers are "I just did something that others might want to know about", "I want something done but I don't know how to do it" and "I want/need to know something but don't know where to get it".
##Events
Events cover the "I just did something" scenario.  The PresentationBus will multicast any events that you publish.
##Commands
Command cover the "I want something done" scenarion. 
##Requests
Requests cover the "I want/need to know something" scenario.  There are a couple of options involved with requests, as described below.
###Request
Use Request when you know there'll be 0..1 handlers.  Expect a null back if there are no handlers.
###Multicast
Use MulticastRequest when there could be 0..* handlers.  Expect an empty set of responses if there are no handlers.
##Threading
The PresentationBus uses an async publish, so the UI thread doesn't block.  Therefore the subscribers get called on a background thread, and if they have to do something back on the UI thread it is their responsibility to marshal what they need onto the UI thread.
#Some examples
##Publisher
```csharp
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
		await _bus.Publish(new SelectedPersonChanged(_selectedPerson));
	}
```
I've used the publishing of events like this with great success in the past to decouple UIs that have a list and details panel.  Beware the "async void" though, exceptions from the subscribers will not surface.
##Subscriber
```csharp
	public class PersonDetailsPanelViewModel : IHandlePresentationEvent<SelectedPersonChanged>
	{
		...
		public void Handle(SelectedPersonChanged selectionChangedEvent)
		{
			// update the panel details with those of the person in the event
		}
	}
```
If you have a subscriber that needs to await further calls, e.g. to load data, then use the IHandlePresentationEventAsync interface.

```csharp
	public class PersonDetailsPanelViewModel : IHandlePresentationEventAsync<SelectedPersonChanged>
	{
		...
		public async Task HandleAsync(SelectedPersonChanged selectionChangedEvent)
		{
			// await whatever data load you need
			// update the panel details
		}
	}
```
##Command handler
In this example we've got a ViewModel (JukeboxViewModel) that knows how to play a music track, e.g using a MediaElement or something like that.  You might have any number of other ViewModels in the UI that represent a track, and might want to have their track played, in which case all they have to do is send a command.
```csharp
	public class JukeboxViewModel : IHandlePresentationCommand<PlayTrackCommand>
	{
		...
		public void Handle(PlayTrackCommand command)
		{
		}
	}
```
###Command sender
NOTE: I [highly encourage Commands over Execute implementations in ViewModels](http://blog.shannonlewis.me/2014/06/xaml-commands/), but for the sake of brevity let's illustrate as follows.
```csharp
	public class TrackViewModel
	{
		...
		public async void OnPlayMeExecuted()
		{
			await _bus.Send(new PlayTrackCommand(this));
		}
	}
```
##Request provider
In this example we've got a ViewModel (AuthViewModel) that knows the details of whether the user is currently logged in, and their name details etc if they are.
```csharp
	public class AuthViewModel : IHandlePresentationRequest<CurrentlyLoggedInRequest, CurrentlyLoggedInResponse>
	{
		...
		public CurrentlyLoggedInResponse Handle(CurrentlyLoggedInRequest request)
		{
			...
		}
	}
```
###Requestor
Now we might have a ViewModel that needs to know if the user is logged in, and changes it's behaviour based on whether they are/aren't.
```csharp
	public class FavouritesViewModel
	{
		...
		public async void OnInitialize()
		{
			var response = await _bus.Request(new CurrentlyLoggedInRequest(this));

			if (response.IsLoggedIn)
			{
				// load favourites from server
			}
			else
			{
				// load favourites from local cache
			}
		}
	}
```
Note that the definition of the request objects will probably look a little unusual at first.  Let's look at the request class for this example.
```csharp
	public class CurrentlyLoggedInRequest : PresentationRequest<CurrentlyLoggedInRequest, CurrentlyLoggedInResponse>
	{
		...
	}
```
The thing that'll probably look a little unusual with this is that the class it the first parameter in it's own generics definition.  The reason behind this is to make the Request call syntax simpler, because the compiler can infer types.  Without this setup the Request call would read as follows, which I find is noisy when you're trying to read the code.
```csharp
			var response = await _bus.Request<CurrentlyLoggedInRequest, CurrentlyLoggedInResponse>(new CurrentlyLoggedInRequest(this));
```

#Subscribing
The last step is to understand how to get the subscribers wired up.  In short, you have to pass an instance of the subscibing class into the PresentationBus' Subscribe method.  In most of the apps I build I'm using an IoC container, and we can get it to do some of the dirty work.
##Autofac
If you're using Autofac you may want something like the following to register the bus itself into the container

```csharp
    public class PresentationBusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
				.RegisterType<PresentationBus>()
				.As<IPresentationBus>()
				.As<IPresentationBusConfiguration>()
				.SingleInstance();
        }
    }
```

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
            var bus = e.Context.Resolve<IPresentationBusConfiguration>();
            bus.Subscribe(handler);
        }
    }
This will hook into the activation of all of object being resolved from the container, and if they handle any events/requests they will be subscribed.

Something to always remember in rich client apps is that objects can live for a lot longer than you might think.  The PresentationBus uses WeakReferences internally so it will allow them to be collected once they are no longer being used, however until they are collected they will still receive events/requests unless you explicitly Unsubscribe them!  Usually this isn't an issue, but I have hit these scenarios on occasion.
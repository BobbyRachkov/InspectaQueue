using System.Threading.Channels;

namespace Rachkov.InspectaQueue.Abstractions.Messaging;

public interface ICanPublish
{
    Task Connect(ChannelReader<> source, IProgressNotifier publishProgressNotifier);
}
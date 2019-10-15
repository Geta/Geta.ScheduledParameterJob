using EPiServer.Core;

namespace Geta.ScheduledParameterJob.Sample.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}

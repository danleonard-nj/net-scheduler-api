namespace NetScheduler.Clients.Abstractions;

using System.Threading.Tasks;

public interface IFeatureClient
{
    Task<bool> EvaluateFeature(string featureKey);
}
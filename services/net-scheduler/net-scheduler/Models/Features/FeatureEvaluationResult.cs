namespace NetScheduler.Models.Features;

public class FeatureEvaluationResult
{
    public FeatureEvaluationResult()
    {
    }

    public FeatureEvaluationResult(
        bool isSchedulerEnabled,
        bool isCalculationDetailEnabled,
        bool isHistoryEnabled,
        bool isForceCalculateTimestampsEnabled)
    {
        IsSchedulerEnabled = isSchedulerEnabled;
        IsCalculationDetailEnabled = isCalculationDetailEnabled;
        IsHistoryEnabled = isHistoryEnabled;
        IsForceCalculateTimestampsEnabled = isForceCalculateTimestampsEnabled;
    }

    public bool IsSchedulerEnabled { get; set; }

    public bool IsCalculationDetailEnabled { get; set; }

    public bool IsHistoryEnabled { get; set; }

    public bool IsForceCalculateTimestampsEnabled { get; set; }
}

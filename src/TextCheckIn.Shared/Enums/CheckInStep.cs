namespace TextCheckIn.Shared.Enums;

/// <summary>
/// Enumeration of check-in workflow steps
/// </summary>
public enum CheckInStep
{
    /// <summary>
    /// Initial step: License plate selection or entry
    /// </summary>
    LicensePlateSelection = 1,

    /// <summary>
    /// Vehicle details confirmation and correction
    /// </summary>
    VehicleDetails = 2,

    /// <summary>
    /// Additional vehicle information (mileage, condition)
    /// </summary>
    VehicleInfo = 3,

    /// <summary>
    /// Personal information collection
    /// </summary>
    PersonalInfo = 4,

    /// <summary>
    /// Service selection based on recommendations
    /// </summary>
    ServiceSelection = 5,

    /// <summary>
    /// Service confirmation and pricing
    /// </summary>
    ServiceConfirmation = 6,

    /// <summary>
    /// Check-in completion
    /// </summary>
    Completion = 7,

    /// <summary>
    /// Technician assistance requested
    /// </summary>
    TechnicianAssistance = 8
}

using System;

namespace TextCheckIn.Core.Models.Domain
{
    public class SessionPayload
    {
        public Guid? CheckInUuid { get; set; }
        public OtpData? OtpData { get; set; }
        public OtpCooldown? OtpCooldown { get; set; }
        public OtpSendCount? OtpSendCount { get; set; }
    }

    public class OtpData
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public DateTime OtpGenerated { get; set; }
        public DateTime OtpExpiry { get; set; }
        public int Attempts { get; set; }
    }

    public class OtpCooldown
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CooldownUntil { get; set; }
        public int SendCount { get; set; }
    }

    public class OtpSendCount
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime LastSendTime { get; set; }
    }
}

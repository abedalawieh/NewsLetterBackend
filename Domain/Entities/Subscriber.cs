using System;
using System.Collections.Generic;
using NewsletterApp.Domain.Interfaces;

namespace NewsletterApp.Domain.Entities
{
    public class Subscriber : ISoftDelete, IAuditEntity
    {
        public Guid Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string Type { get; private set; }
        public List<string> CommunicationMethods { get; private set; } = new();
        public List<string> Interests { get; private set; } = new();
        public bool IsActive { get; private set; }
        
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        private Subscriber() { }

        public static Subscriber Create(
            string firstName,
            string lastName,
            string email,
            string type,
            IEnumerable<string> communicationMethods,
            IEnumerable<string> interests)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required");
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");

            var subscriber = new Subscriber
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Type = type,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            subscriber.CommunicationMethods.AddRange(communicationMethods);
            subscriber.Interests.AddRange(interests);

            return subscriber;
        }

        public void UpdateDetails(
            string firstName,
            string lastName,
            string type,
            IEnumerable<string> communicationMethods,
            IEnumerable<string> interests)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required");
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required");

            FirstName = firstName;
            LastName = lastName;
            Type = type;
            
            CommunicationMethods.Clear();
            CommunicationMethods.AddRange(communicationMethods);
            
            Interests.Clear();
            Interests.AddRange(interests);
            
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate(string reason = "")
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void UpdateType(string newType)
        {
            if (string.IsNullOrWhiteSpace(newType)) throw new ArgumentException("Type cannot be empty");
            Type = newType;
        }
    }

    public class SubscriptionHistory : IAuditEntity
    {
        public Guid Id { get; set; }
        public Guid SubscriberId { get; set; }
        public string Action { get; set; } // Subscribe, Unsubscribe
        public string Reason { get; set; }
        /// <summary>Optional free-text comment (e.g. for unsubscribe feedback).</summary>
        public string Comment { get; set; }
        public DateTime Timestamp { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public static SubscriptionHistory Create(Guid subscriberId, string action, string reason = "", string comment = null)
        {
            return new SubscriptionHistory
            {
                Id = Guid.NewGuid(),
                SubscriberId = subscriberId,
                Action = action,
                Reason = reason ?? "",
                Comment = comment,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}

namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents the abstract base class for all domain entities, 
    /// providing a unique identifier and audit metadata.
    /// </summary>
    public abstract class AuditableEntity
    {
        /// <summary>
        /// Gets the unique identifier of the entity.
        /// </summary>
        public Guid Id { get; protected set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the UTC timestamp indicating when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the UTC timestamp indicating when the entity was last updated, if applicable.
        /// </summary>
        public DateTime? UpdatedAt { get; protected set; }

        /// <summary>
        /// Updates the <see cref="UpdatedAt"/> timestamp to the current UTC time,
        /// indicating that the entity has been modified.
        /// </summary>
        protected void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

    }
}

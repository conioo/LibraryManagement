#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class ArchivalReservation : BaseEntity
    {
        public DateOnly BeginDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly? CollectionDate { get; set; }
        public virtual ProfileHistory ProfileHistory { get; set; }
        public virtual CopyHistory CopyHistory { get; set; }
    }
}
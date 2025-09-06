using Microsoft.AspNetCore.Identity;
using OrderLagerSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace OrderLagerSystem.Data
{
    /// <summary>
    /// Utökad användare med roller och navigation properties
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(GlobalRules.NameMaxLen)]
        public string? FullName { get; set; }

        [MaxLength(GlobalRules.NameMaxLen)]
        public string? FirstName { get; set; }

        [MaxLength(GlobalRules.NameMaxLen)]
        public string? LastName { get; set; }

        /// <summary>
        /// Om användarkontot är aktivt
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// När användaren skapades
        /// </summary>
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Senast uppdaterad
        /// </summary>
        public DateTime? UpdatedUtc { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public ICollection<OrderHistory> OrderHistoryEntries { get; set; } = new List<OrderHistory>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<UserRole> AssignedRoles { get; set; } = new List<UserRole>();
    }
}
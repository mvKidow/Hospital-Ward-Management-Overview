using Microsoft.EntityFrameworkCore;
using CareConnect.Models;
using System.Data;
using Connect.Models;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Connect.Data
{
    public class CareConnectDbContext : DbContext
    {
        public CareConnectDbContext(DbContextOptions<CareConnectDbContext> options) : base(options)
        {

        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Allergy> Allergy { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Condidtion> Condidtion { get; set; }
        public DbSet<Consumable> Consumables { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        //public DbSet<DoctorVisit> DoctorVisits { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Instrustion> Instrustions { get; set; }
        public DbSet<Medication> Medication { get; set; }
        public DbSet<Movement> Movements { get; set; }
        public DbSet<Nurse> Nurses { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientFile> PatientFiles { get; set; }
        public DbSet<PatientFileVitals> PatientFilesVitals { get; set; }
        public DbSet<Prescription> Prescription { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<StockRequest> stockRequests { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<Vitals> Vitals { get; set; }
        public DbSet<Ward> Ward { get; set; }
        public DbSet<WardAdmin> WardAdmin { get; set; }
        public DbSet<WardConsumablesStock> WardConsumablesStock { get; set; }
        public DbSet<MedicationAdministration> MedicationAdministrations { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<PatientTreatmen> PatientTreatment { get; set; }
        public DbSet<PurchaseOrderConsumablesDetails> PurchaseOrderConsumablesDetails { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<MedsPerscription> MedsPerscriptions { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<CareConnectInfor> CareConnectInfors { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }  
        public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
        public DbSet<UserWard> UserWards { get; set; }
        public  DbSet<DischargeReport> DischargeReports { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<OtpToken> OtpTokens {  get; set; }






        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Surname)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Phone)
                    .HasMaxLength(15);
                entity.Property(e => e.Role)
                    .IsRequired();
                entity.Property(e => e.Title)
                    .IsRequired();
                entity.Property(e => e.Status)
                    .HasMaxLength(20);
                entity.Property(e => e.ProfilePhoto)
                    .HasMaxLength(255)
                    .IsRequired(false);
                entity.Property(e => e.WardId)
                    .IsRequired(false);
                entity.HasOne(u => u.Ward)
                    .WithMany()
                    .HasForeignKey(u => u.WardId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UserWard>(entity =>
            {
                entity.ToTable("UserWards");
                entity.HasKey(e => new { e.UserId, e.WardId });

                entity.HasOne(uw => uw.User)
                    .WithMany(u => u.UserWards)
                    .HasForeignKey(uw => uw.UserId)
                    .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(uw => uw.Ward)
                    .WithMany(w => w.UserWards)
                    .HasForeignKey(uw => uw.WardId)
                    .OnDelete(DeleteBehavior.Cascade); 
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.ToTable("Doctors");
                entity.HasBaseType<User>();
                entity.Property(e => e.Specialization)
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Nurse>(entity =>
            {
                entity.ToTable("Nurses");
                entity.HasBaseType<User>();
                entity.Property(e => e.Department)
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patients");
                entity.HasBaseType<User>();
                entity.Property(e => e.PatientType)
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admins");
                entity.HasBaseType<User>();
                entity.Property(e => e.AdminSpecificField)
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<WardAdmin>(entity =>
            {
                entity.ToTable("WardAdmins");
                entity.HasBaseType<User>();
                entity.Property(e => e.AdminSpecificField)
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(p => p.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(p => p.UserId);

            base.OnModelCreating(modelBuilder);
        }



    }
}


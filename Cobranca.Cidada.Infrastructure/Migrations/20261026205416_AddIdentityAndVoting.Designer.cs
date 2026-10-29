
using System;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cobranca.Cidada.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261026205416_AddIdentityAndVoting")]
    partial class AddIdentityAndVoting
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Audit.Entities.AuditLog", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Actor")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("character varying(450)");

                    b.Property<string>("Details")
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.Property<string>("EntityId")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("OccurredAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("OccurredAtUtc");

                    b.HasIndex("EntityType", "EntityId");

                    b.ToTable("AuditLogs", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Catalog.Entities.Area", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("Areas", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Catalog.Entities.Origin", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("IsInOfficialPlan")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("Origins", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("AreaId")
                        .HasColumnType("uuid");

                    b.Property<string[]>("Classification")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<DateTime>("CreatedAtUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("character varying(450)");

                    b.Property<string>("CurrentStatus")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("character varying(40)");

                    b.Property<DateOnly?>("CurrentStatusDate")
                        .HasColumnType("date");

                    b.Property<DateOnly?>("Date")
                        .HasColumnType("date");

                    b.Property<string>("DateKind")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateOnly?>("DeadlineDate")
                        .HasColumnType("date");

                    b.Property<string>("DeadlineText")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string[]>("Evidence")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("ExternalId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("GoalOrCondition")
                        .HasColumnType("text");

                    b.Property<bool>("HasPlanDetail")
                        .HasColumnType("boolean");

                    b.Property<string>("HowItWillBeDone")
                        .HasColumnType("text");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("boolean");

                    b.Property<Guid>("MandateId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("OriginId")
                        .HasColumnType("uuid");

                    b.Property<string>("PlanPage")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime?>("PublishedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ShortDescription")
                        .HasColumnType("text");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("character varying(120)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.Property<DateTime>("UpdatedAtUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("Version")
                        .HasColumnType("integer");

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.HasIndex("AreaId");

                    b.HasIndex("ExternalId")
                        .IsUnique();

                    b.HasIndex("MandateId");

                    b.HasIndex("OriginId");

                    b.HasIndex("Slug")
                        .IsUnique();

                    b.HasIndex("IsPublished", "AreaId");

                    b.HasIndex("IsPublished", "CurrentStatus");

                    b.ToTable("Commitments", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Commitment.Entities.CommitmentRevision", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("AuthorId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("character varying(450)");

                    b.Property<Guid>("CommitmentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("SnapshotJson")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("Version")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CommitmentId", "Version")
                        .IsUnique();

                    b.ToTable("CommitmentRevisions", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Commitment.Entities.CommitmentSource", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CommitmentId")
                        .HasColumnType("uuid");

                    b.Property<string>("Kind")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Page")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("Position")
                        .HasColumnType("integer");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.HasKey("Id");

                    b.HasIndex("CommitmentId");

                    b.ToTable("CommitmentSources", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Commitment.Entities.StatusEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("AuthorId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("character varying(450)");

                    b.Property<Guid>("CommitmentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsFormalAct")
                        .HasColumnType("boolean");

                    b.Property<string>("Note")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<DateOnly>("ReferenceDate")
                        .HasColumnType("date");

                    b.Property<string>("SourceUrl")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("character varying(40)");

                    b.HasKey("Id");

                    b.HasIndex("CommitmentId", "ReferenceDate");

                    b.ToTable("StatusEntries", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Mandate.Aggregates.Mandate", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateOnly>("EndDate")
                        .HasColumnType("date");

                    b.Property<Guid>("OfficialId")
                        .HasColumnType("uuid");

                    b.Property<DateOnly>("StartDate")
                        .HasColumnType("date");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.HasIndex("OfficialId");

                    b.HasIndex("Title")
                        .IsUnique();

                    b.ToTable("Mandates", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Mandate.Entities.Official", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Party")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("Officials", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.User.Entities.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<DateTime?>("ConsentAcceptedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ConsentVersion")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAtUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("LockoutLevel")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("UpdatedAtUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.User.Entities.UserSession", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DeviceName")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("character varying(160)");

                    b.Property<DateTime>("ExpiresAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("FamilyId")
                        .HasColumnType("uuid");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("LastUsedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RefreshTokenHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<Guid?>("ReplacedBySessionId")
                        .HasColumnType("uuid");

                    b.Property<string>("RevocationReason")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTime?>("RevokedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UserAgent")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("character varying(450)");

                    b.HasKey("Id");

                    b.HasIndex("RefreshTokenHash")
                        .IsUnique();

                    b.HasIndex("UserId", "FamilyId");

                    b.HasIndex("UserId", "RevokedAtUtc", "ExpiresAtUtc");

                    b.ToTable("UserSessions", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Voting.Aggregates.Vote", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<bool>("Charge")
                        .HasColumnType("boolean");

                    b.Property<Guid>("CommitmentId")
                        .HasColumnType("uuid");

                    b.Property<string>("Rating")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<DateTime>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("VoterKey")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character(64)")
                        .IsFixedLength();

                    b.HasKey("Id");

                    b.HasIndex("CommitmentId");

                    b.HasIndex("VoterKey", "CommitmentId")
                        .IsUnique();

                    b.ToTable("Votes", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Voting.Entities.VoterIdentity", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("character(64)")
                        .IsFixedLength();

                    b.Property<DateTime>("LastVerifiedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("TokensValidAfterUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("VerifiedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("VoterIdentities", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.Catalog.Entities.Area", "Area")
                        .WithMany()
                        .HasForeignKey("AreaId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Cobranca.Cidada.Domain.Modules.Mandate.Aggregates.Mandate", null)
                        .WithMany()
                        .HasForeignKey("MandateId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Cobranca.Cidada.Domain.Modules.Catalog.Entities.Origin", "Origin")
                        .WithMany()
                        .HasForeignKey("OriginId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Area");

                    b.Navigation("Origin");
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Commitment.Entities.CommitmentRevision", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment", null)
                        .WithMany("Revisions")
                        .HasForeignKey("CommitmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Commitment.Entities.CommitmentSource", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment", null)
                        .WithMany("Sources")
                        .HasForeignKey("CommitmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Commitment.Entities.StatusEntry", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment", null)
                        .WithMany("StatusEntries")
                        .HasForeignKey("CommitmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Mandate.Aggregates.Mandate", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.Mandate.Entities.Official", null)
                        .WithMany()
                        .HasForeignKey("OfficialId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.User.Entities.UserSession", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.User.Entities.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Voting.Aggregates.Vote", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment", null)
                        .WithMany()
                        .HasForeignKey("CommitmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.User.Entities.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.User.Entities.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Cobranca.Cidada.Domain.Modules.User.Entities.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Cobranca.Cidada.Domain.Modules.User.Entities.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment", b =>
                {
                    b.Navigation("Revisions");

                    b.Navigation("Sources");

                    b.Navigation("StatusEntries");
                });
#pragma warning restore 612, 618
        }
    }
}

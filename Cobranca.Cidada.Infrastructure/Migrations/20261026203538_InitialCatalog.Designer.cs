
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
    [Migration("20261026203538_InitialCatalog")]
    partial class InitialCatalog
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

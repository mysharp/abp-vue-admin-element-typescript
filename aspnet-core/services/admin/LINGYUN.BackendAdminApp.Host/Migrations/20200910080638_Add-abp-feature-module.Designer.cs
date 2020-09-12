﻿// <auto-generated />
using System;
using LINGYUN.BackendAdmin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Volo.Abp.EntityFrameworkCore;

namespace LINGYUN.BackendAdmin.Migrations
{
    [DbContext(typeof(BackendAdminHostMigrationsDbContext))]
    [Migration("20200910080638_Add-abp-feature-module")]
    partial class Addabpfeaturemodule
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("_Abp_DatabaseProvider", EfCoreDatabaseProvider.MySql)
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Volo.Abp.FeatureManagement.FeatureValue", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<string>("ProviderName")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.HasIndex("Name", "ProviderName", "ProviderKey");

                    b.ToTable("AbpFeatureValues");
                });

            modelBuilder.Entity("Volo.Abp.PermissionManagement.PermissionGrant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .IsRequired()
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<string>("ProviderName")
                        .IsRequired()
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<Guid?>("TenantId")
                        .HasColumnName("TenantId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("Name", "ProviderName", "ProviderKey");

                    b.ToTable("AbpPermissionGrants");
                });

            modelBuilder.Entity("Volo.Abp.SettingManagement.Setting", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<string>("ProviderName")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4")
                        .HasMaxLength(2048);

                    b.HasKey("Id");

                    b.HasIndex("Name", "ProviderName", "ProviderKey");

                    b.ToTable("AbpSettings");
                });

            modelBuilder.Entity("Volo.Abp.TenantManagement.Tenant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnName("ConcurrencyStamp")
                        .HasColumnType("varchar(40) CHARACTER SET utf8mb4")
                        .HasMaxLength(40);

                    b.Property<DateTime>("CreationTime")
                        .HasColumnName("CreationTime")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid?>("CreatorId")
                        .HasColumnName("CreatorId")
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("DeleterId")
                        .HasColumnName("DeleterId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime?>("DeletionTime")
                        .HasColumnName("DeletionTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ExtraProperties")
                        .HasColumnName("ExtraProperties")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("IsDeleted")
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("LastModificationTime")
                        .HasColumnName("LastModificationTime")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid?>("LastModifierId")
                        .HasColumnName("LastModifierId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("AbpTenants");
                });

            modelBuilder.Entity("Volo.Abp.TenantManagement.TenantConnectionString", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("varchar(1024) CHARACTER SET utf8mb4")
                        .HasMaxLength(1024);

                    b.HasKey("TenantId", "Name");

                    b.ToTable("AbpTenantConnectionStrings");
                });

            modelBuilder.Entity("Volo.Abp.TenantManagement.TenantConnectionString", b =>
                {
                    b.HasOne("Volo.Abp.TenantManagement.Tenant", null)
                        .WithMany("ConnectionStrings")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}

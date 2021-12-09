﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetCoreTemp.WebApi.Models.DatabaseContext;

namespace NetCoreTemp.WebApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20211209084200_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MenuRole", b =>
                {
                    b.Property<Guid>("ArrMenuID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ArrRoleID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ArrMenuID", "ArrRoleID");

                    b.HasIndex("ArrRoleID");

                    b.ToTable("MenuRole");
                });

            modelBuilder.Entity("NetCoreTemp.WebApi.Models.Menu", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Component")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Controller")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<long>("CreateDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("CreateUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CreateUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<bool>("Hidden")
                        .HasColumnType("bit");

                    b.Property<string>("IconCls")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<long?>("ModifyDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("ModifyUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ModifyUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<Guid?>("ParentMenuId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Remark")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Resource")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Sort")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("ID");

                    b.HasIndex("ParentMenuId");

                    b.ToTable("Menu");
                });

            modelBuilder.Entity("NetCoreTemp.WebApi.Models.Role", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("CreateDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("CreateUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CreateUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<long?>("ModifyDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("ModifyUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ModifyUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Remark")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("Sort")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("Role");
                });

            modelBuilder.Entity("NetCoreTemp.WebApi.Models.RoleMenu", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("CreateDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("CreateUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CreateUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<Guid>("MenuId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long?>("ModifyDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("ModifyUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ModifyUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Remark")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("MenuId");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleMenu");
                });

            modelBuilder.Entity("NetCoreTemp.WebApi.Models.User", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("CreateDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("CreateUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CreateUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<long?>("ModifyDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("ModifyUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ModifyUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Roles")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("User");
                });

            modelBuilder.Entity("NetCoreTemp.WebApi.Models.UserRole", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("CreateDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("CreateUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CreateUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<long?>("ModifyDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("ModifyUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ModifyUserName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Remark")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ID");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("UserRole");
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.Property<Guid>("ArrRoleID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ArrUserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ArrRoleID", "ArrUserID");

                    b.HasIndex("ArrUserID");

                    b.ToTable("RoleUser");
                });

            modelBuilder.Entity("MenuRole", b =>
                {
                    b.HasOne("NetCoreTemp.WebApi.Models.Menu", null)
                        .WithMany()
                        .HasForeignKey("ArrMenuID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NetCoreTemp.WebApi.Models.Role", null)
                        .WithMany()
                        .HasForeignKey("ArrRoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NetCoreTemp.WebApi.Models.Menu", b =>
                {
                    b.HasOne("NetCoreTemp.WebApi.Models.Menu", "ParentMenu")
                        .WithMany("ChildrenMenu")
                        .HasForeignKey("ParentMenuId");

                    b.Navigation("ParentMenu");
                });

            modelBuilder.Entity("NetCoreTemp.WebApi.Models.RoleMenu", b =>
                {
                    b.HasOne("NetCoreTemp.WebApi.Models.Menu", "OMenu")
                        .WithMany()
                        .HasForeignKey("MenuId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NetCoreTemp.WebApi.Models.Role", "ORole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OMenu");

                    b.Navigation("ORole");
                });

            modelBuilder.Entity("NetCoreTemp.WebApi.Models.UserRole", b =>
                {
                    b.HasOne("NetCoreTemp.WebApi.Models.Role", "ORole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NetCoreTemp.WebApi.Models.User", "OUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ORole");

                    b.Navigation("OUser");
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.HasOne("NetCoreTemp.WebApi.Models.Role", null)
                        .WithMany()
                        .HasForeignKey("ArrRoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NetCoreTemp.WebApi.Models.User", null)
                        .WithMany()
                        .HasForeignKey("ArrUserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NetCoreTemp.WebApi.Models.Menu", b =>
                {
                    b.Navigation("ChildrenMenu");
                });
#pragma warning restore 612, 618
        }
    }
}
